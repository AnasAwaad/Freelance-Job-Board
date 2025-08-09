using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Identity;
using FreelanceJobBoard.Infrastructure.Data;
using FreelanceJobBoard.Infrastructure.Services;
using FreelanceJobBoard.Infrastructure.Settings;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Presentation;

public class Program
{
	public static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		builder.Services.AddControllersWithViews();

		// Register HttpClients with IWebHostEnvironment for file handling
		builder.Services.AddHttpClient<Presentation.Services.AuthService>();
		builder.Services.AddHttpClient<CategoryService>();
		builder.Services.AddHttpClient<JobService>();
		builder.Services.AddHttpClient<SkillService>();
		builder.Services.AddHttpClient<HomeService>();

		// Configure Email Settings
		builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

		// Register Email Service
		builder.Services.AddScoped<IEmailService, EmailService>();

		// Configure Database
		var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
		if (string.IsNullOrEmpty(connectionString))
		{
			throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
		}

		builder.Services.AddDbContext<ApplicationDbContext>(options =>
		{
			options.UseSqlServer(connectionString);
			if (builder.Environment.IsDevelopment())
			{
				options.EnableSensitiveDataLogging();
				options.EnableDetailedErrors();
			}
		});

		builder.Services.AddHttpContextAccessor();

		// Configure Identity with proper options
		builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
		{
			// Password settings
			options.Password.RequireDigit = true;
			options.Password.RequireLowercase = true;
			options.Password.RequireNonAlphanumeric = false;
			options.Password.RequireUppercase = false;
			options.Password.RequiredLength = 6;
			options.Password.RequiredUniqueChars = 1;

			// Lockout settings
			options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
			options.Lockout.MaxFailedAccessAttempts = 5;
			options.Lockout.AllowedForNewUsers = true;

			// User settings
			options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
			options.User.RequireUniqueEmail = true;

			// Sign in settings
			options.SignIn.RequireConfirmedEmail = false; // Set to true in production
			options.SignIn.RequireConfirmedPhoneNumber = false;
		})
		.AddEntityFrameworkStores<ApplicationDbContext>()
		.AddDefaultTokenProviders();

		// Configure Cookie Authentication
		builder.Services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
		})
		.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
		{
			options.LoginPath = "/Auth/Login";
			options.LogoutPath = "/Auth/Logout";
			options.AccessDeniedPath = "/Auth/AccessDenied";
			options.ExpireTimeSpan = TimeSpan.FromHours(1);
			options.SlidingExpiration = true;
			options.Cookie.HttpOnly = true;
			options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
			options.Cookie.SameSite = SameSiteMode.Lax;
			options.Cookie.Name = "FreelanceJobBoard.Auth";

			// Handle authentication failures
			options.Events.OnRedirectToAccessDenied = context =>
			{
				context.Response.Redirect("/Auth/AccessDenied");
				return Task.CompletedTask;
			};

			options.Events.OnRedirectToLogin = context =>
			{
				context.Response.Redirect("/Auth/Login");
				return Task.CompletedTask;
			};
		});

		// Configure Authorization with proper policies
		builder.Services.AddAuthorization(options =>
		{
			// Add authorization policies
			options.AddPolicy("RequireAdminRole", policy =>
			{
				policy.RequireAuthenticatedUser();
				policy.RequireRole(AppRoles.Admin);
			});

			options.AddPolicy("RequireClientRole", policy =>
			{
				policy.RequireAuthenticatedUser();
				policy.RequireRole(AppRoles.Client);
			});

			options.AddPolicy("RequireFreelancerRole", policy =>
			{
				policy.RequireAuthenticatedUser();
				policy.RequireRole(AppRoles.Freelancer);
			});

			options.AddPolicy("RequireClientOrFreelancer", policy =>
			{
				policy.RequireAuthenticatedUser();
				policy.RequireRole(AppRoles.Client, AppRoles.Freelancer);
			});
		});

		// Add antiforgery
		builder.Services.AddAntiforgery();

		var app = builder.Build();

		// Ensure database is created and seed data
		await InitializeDatabaseAsync(app);

		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Home/Error");
			app.UseHsts();
		}
		else
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseHttpsRedirection();
		app.UseStaticFiles();

		app.UseRouting();

		// Order is important: Authentication before Authorization
		app.UseAuthentication();
		app.UseAuthorization();

		// Add antiforgery middleware
		app.UseAntiforgery();

		app.MapControllerRoute(
			name: "default",
			pattern: "{controller=Auth}/{action=Login}/{id?}");

		app.Run();
	}

	private static async Task InitializeDatabaseAsync(WebApplication app)
	{
		using var scope = app.Services.CreateScope();
		var services = scope.ServiceProvider;
		var logger = services.GetRequiredService<ILogger<Program>>();

		try
		{
			// Get required services
			var context = services.GetRequiredService<ApplicationDbContext>();
			var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
			var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

			logger.LogInformation("Starting database initialization...");

			// Check if database exists and can be connected to
			var canConnect = await context.Database.CanConnectAsync();
			logger.LogInformation("Database connection check: {CanConnect}", canConnect);

			if (!canConnect)
			{
				logger.LogInformation("Database does not exist, creating database...");
				await context.Database.EnsureCreatedAsync();
				logger.LogInformation("Database created successfully using EnsureCreatedAsync");
			}
			else
			{
				// Database exists, check for pending migrations
				var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
				var pendingMigrationsList = pendingMigrations.ToList();

				if (pendingMigrationsList.Any())
				{
					logger.LogInformation("Found {Count} pending migrations: {Migrations}",
						pendingMigrationsList.Count, string.Join(", ", pendingMigrationsList));

					try
					{
						logger.LogInformation("Applying pending migrations...");
						await context.Database.MigrateAsync();
						logger.LogInformation("Migrations applied successfully");
					}
					catch (Exception migrationEx)
					{
						logger.LogWarning(migrationEx, "Migration failed, checking if tables already exist...");

						// Check if Identity tables exist (indicating database was created without migrations)
						var hasIdentityTables = await CheckIfIdentityTablesExistAsync(context);

						if (hasIdentityTables)
						{
							logger.LogInformation("Identity tables already exist, marking all migrations as applied...");

							// Get all migrations from the assembly
							var allMigrations = context.Database.GetMigrations();

							// Add migration history entries for existing migrations
							foreach (var migration in allMigrations)
							{
								await context.Database.ExecuteSqlRawAsync(
									"IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {0}) " +
									"INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ({0}, {1})",
									migration, "8.0.0");
							}

							logger.LogInformation("Migration history updated successfully");
						}
						else
						{
							// If no Identity tables, re-throw the original exception
							throw;
						}
					}
				}
				else
				{
					logger.LogInformation("No pending migrations found");
				}
			}

			// Create roles and admin user
			await CreateRolesAndAdminUserAsync(userManager, roleManager, logger);

			logger.LogInformation("Database initialization completed successfully");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An error occurred while initializing the database");

			// In development, we can try to recover from certain database issues
			if (app.Environment.IsDevelopment())
			{
				logger.LogWarning("Development environment detected, attempting recovery...");
				try
				{
					await RecoverDatabaseAsync(services, logger);
				}
				catch (Exception recoveryEx)
				{
					logger.LogError(recoveryEx, "Database recovery failed");
					throw new InvalidOperationException(
						"Database initialization failed. Please check the connection string and database permissions.", ex);
				}
			}
			else
			{
				throw new InvalidOperationException(
					"Database initialization failed. Please check the connection string and database permissions.", ex);
			}
		}
	}

	private static async Task<bool> CheckIfIdentityTablesExistAsync(ApplicationDbContext context)
	{
		try
		{
			// Check if AspNetUsers table exists by trying to query it
			await context.Database.ExecuteSqlRawAsync(
				"SELECT TOP 1 1 FROM [AspNetUsers]");
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static async Task RecoverDatabaseAsync(IServiceProvider services, ILogger logger)
	{
		var context = services.GetRequiredService<ApplicationDbContext>();
		var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
		var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

		logger.LogInformation("Attempting database recovery...");

		try
		{
			// Try to delete and recreate the database in development
			logger.LogWarning("Deleting existing database for clean recreation...");
			await context.Database.EnsureDeletedAsync();

			logger.LogInformation("Creating fresh database...");
			await context.Database.EnsureCreatedAsync();

			logger.LogInformation("Database recreated successfully");

			// Create roles and admin user
			await CreateRolesAndAdminUserAsync(userManager, roleManager, logger);

			logger.LogInformation("Database recovery completed successfully");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Database recovery failed");
			throw;
		}
	}

	private static async Task CreateRolesAndAdminUserAsync(
		UserManager<ApplicationUser> userManager,
		RoleManager<IdentityRole> roleManager,
		ILogger logger)
	{
		try
		{
			logger.LogInformation("Starting roles and admin user creation...");

			// Create roles if they don't exist
			var roles = new[] { AppRoles.Admin, AppRoles.Client, AppRoles.Freelancer };

			foreach (var roleName in roles)
			{
				if (!await roleManager.RoleExistsAsync(roleName))
				{
					var role = new IdentityRole(roleName);
					var result = await roleManager.CreateAsync(role);

					if (result.Succeeded)
					{
						logger.LogInformation("Role '{RoleName}' created successfully", roleName);
					}
					else
					{
						logger.LogError("Failed to create role '{RoleName}': {Errors}",
							roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
					}
				}
				else
				{
					logger.LogInformation("Role '{RoleName}' already exists", roleName);
				}
			}

			// Create default admin user
			const string adminEmail = "admin@freelancejobboard.com";
			const string adminPassword = "Admin@123";

			var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
			if (existingAdmin == null)
			{
				var adminUser = new ApplicationUser
				{
					UserName = "admin",
					Email = adminEmail,
					FullName = "System Administrator",
					EmailConfirmed = true,
					PhoneNumberConfirmed = true,
					LockoutEnabled = false
				};

				var createResult = await userManager.CreateAsync(adminUser, adminPassword);
				if (createResult.Succeeded)
				{
					var roleResult = await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
					if (roleResult.Succeeded)
					{
						logger.LogInformation("Default admin user created successfully");
						logger.LogInformation("Admin Credentials - Email: {Email}, Password: {Password}",
							adminEmail, adminPassword);
						logger.LogWarning("IMPORTANT: Change the default admin password after first login!");
					}
					else
					{
						logger.LogError("Failed to assign admin role: {Errors}",
							string.Join(", ", roleResult.Errors.Select(e => e.Description)));
					}
				}
				else
				{
					logger.LogError("Failed to create default admin user: {Errors}",
						string.Join(", ", createResult.Errors.Select(e => e.Description)));
				}
			}
			else
			{
				// Ensure existing admin has admin role
				if (!await userManager.IsInRoleAsync(existingAdmin, AppRoles.Admin))
				{
					var roleResult = await userManager.AddToRoleAsync(existingAdmin, AppRoles.Admin);
					if (roleResult.Succeeded)
					{
						logger.LogInformation("Admin role assigned to existing user: {Email}", adminEmail);
					}
					else
					{
						logger.LogError("Failed to assign admin role to existing user: {Errors}",
							string.Join(", ", roleResult.Errors.Select(e => e.Description)));
					}
				}
				else
				{
					logger.LogInformation("Admin user already exists and has correct role: {Email}", adminEmail);
				}
			}

			var verifyAdmin = await userManager.FindByEmailAsync(adminEmail);
			if (verifyAdmin != null)
			{
				var isInAdminRole = await userManager.IsInRoleAsync(verifyAdmin, AppRoles.Admin);
				var userRoles = await userManager.GetRolesAsync(verifyAdmin);

				logger.LogInformation("Admin user verification - Email: {Email}, IsAdmin: {IsAdmin}, Roles: {Roles}",
					verifyAdmin.Email, isInAdminRole, string.Join(", ", userRoles));

				if (isInAdminRole)
				{
					logger.LogInformation("✅ Admin role configuration completed successfully!");
				}
				else
				{
					logger.LogError("❌ Admin user exists but does not have admin role!");
				}
			}
			else
			{
				logger.LogError("❌ Admin user verification failed - user not found!");
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred while creating roles and admin user");
			throw;
		}
	}
}
