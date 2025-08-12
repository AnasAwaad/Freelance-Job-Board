using FreelanceJobBoard.Application.Extensions;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Identity;
using FreelanceJobBoard.Infrastructure.Data;
using FreelanceJobBoard.Infrastructure.Extensions;
using FreelanceJobBoard.Infrastructure.Hubs;
using FreelanceJobBoard.Infrastructure.Services;
using FreelanceJobBoard.Infrastructure.Settings;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FreelanceJobBoard.Presentation;

public class Program
{
	public static async Task Main(string[] args)
	{
		// Configure Serilog early in the startup process
		var configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json")
			.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
			.Build();

		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(configuration)
			.Enrich.FromLogContext()
			.Enrich.WithCorrelationId()
			.CreateLogger();

		try
		{
			Log.Information("🚀 Starting FreelanceJobBoard Presentation application");

			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllersWithViews();

			// Configure Session (optional, for future use)
			builder.Services.AddDistributedMemoryCache();
			builder.Services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromMinutes(30);
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
				options.Cookie.Name = "FreelanceJobBoard.Session";
			});

			// Register HttpClients with IWebHostEnvironment for file handling
			builder.Services.AddHttpClient<Presentation.Services.AuthService>();
			builder.Services.AddHttpClient<Presentation.Services.UserService>();
			builder.Services.AddHttpClient<CategoryService>();
			builder.Services.AddHttpClient<JobService>();
			builder.Services.AddHttpClient<SkillService>();
			builder.Services.AddHttpClient<HomeService>();
			builder.Services.AddHttpClient<ProposalService>();

			// Configure Email Settings
			builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

			// Register Email Service
			builder.Services.AddScoped<IEmailService, EmailService>();

			// Configure Database

			// Use Serilog for logging
			builder.Host.UseSerilog();

			// Add services to the container.
			builder.Services.AddControllersWithViews();

			// Add SignalR
			builder.Services.AddSignalR(options =>
			{
				options.EnableDetailedErrors = builder.Environment.IsDevelopment();
				options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
				options.HandshakeTimeout = TimeSpan.FromSeconds(15);
				options.KeepAliveInterval = TimeSpan.FromSeconds(15);
				options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
			});

			// Add Application and Infrastructure layers
			builder.Services.AddApplication();
			builder.Services.AddInfrastructure(builder.Configuration, configureIdentity: false);

			// Configure Session (optional, for future use)
			builder.Services.AddDistributedMemoryCache();
			builder.Services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromMinutes(30);
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
				options.Cookie.Name = "FreelanceJobBoard.Session";
			});

			// Register HttpClients with IWebHostEnvironment for file handling
			builder.Services.AddHttpClient<Presentation.Services.AuthService>();
			builder.Services.AddHttpClient<Presentation.Services.UserService>();
			builder.Services.AddHttpClient<CategoryService>();
			builder.Services.AddHttpClient<JobService>();
			builder.Services.AddHttpClient<SkillService>();
			builder.Services.AddHttpClient<ProposalService>();
			builder.Services.AddHttpClient<ContractService>();
			// Configure Email Settings
			builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));


			// Configure Database
			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

			if (string.IsNullOrEmpty(connectionString))
			{
				Log.Fatal("❌ Connection string 'DefaultConnection' not found");
				throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
			}

			// Log the connection string for debugging (remove in production)
			Log.Information("🔗 Database Connection configured: {ConnectionType}",
				connectionString.Contains("localdb") ? "LocalDB" : "SQL Server");

			// Note: ApplicationDbContext is already configured in AddInfrastructure()
			// Remove duplicate registration to avoid conflicts

			builder.Services.AddHttpContextAccessor();

			// Configure Identity with proper options for cookie authentication
			// This is separate from the Infrastructure Identity configuration
			builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
			{
				// Configure password requirements (if needed)
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireNonAlphanumeric = true;
				options.Password.RequireUppercase = true;
				options.Password.RequiredLength = 8;
				options.Password.RequiredUniqueChars = 3;

				// Configure lockout settings
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
				options.Lockout.MaxFailedAccessAttempts = 5;

				// Configure user settings
				options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

				// Configure sign-in settings
				options.SignIn.RequireConfirmedAccount = true;
				options.SignIn.RequireConfirmedEmail = true;
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
					Log.Warning("🚫 Access denied for user {UserId} to path {Path}",
						context.HttpContext.User?.Identity?.Name ?? "Anonymous",
						context.Request.Path);
					context.Response.Redirect("/Auth/AccessDenied");
					return Task.CompletedTask;
				};

				options.Events.OnRedirectToLogin = context =>
				{
					Log.Information("🔑 Redirecting unauthenticated user to login from path {Path}",
						context.Request.Path);
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

			Log.Information("📦 Services configuration completed");

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

			// Add session middleware (optional)
			app.UseSession();

			// Order is important: Authentication before Authorization
			app.UseAuthentication();
			app.UseAuthorization();

			// Add antiforgery middleware
			app.UseAntiforgery();

			// Map SignalR hubs
			app.MapHub<NotificationHub>("/hubs/notifications");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			Log.Information("🌐 FreelanceJobBoard Presentation application started successfully with SignalR support");
			app.Run();
		}
		catch (Exception ex)
		{
			Log.Fatal(ex, "💥 FreelanceJobBoard Presentation application terminated unexpectedly");
		}
		finally
		{
			await Log.CloseAndFlushAsync();
		}
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

			logger.LogInformation("🔄 Starting database initialization...");

			// Check if database exists and can be connected to
			var canConnect = await context.Database.CanConnectAsync();
			logger.LogInformation("📊 Database connection check: {CanConnect}", canConnect);

			if (!canConnect)
			{
				logger.LogInformation("🆕 Database does not exist, creating database...");
				await context.Database.EnsureCreatedAsync();
				logger.LogInformation("✅ Database created successfully using EnsureCreatedAsync");
			}
			else
			{
				// Database exists, check for pending migrations
				try
				{
					var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
					var pendingMigrationsList = pendingMigrations.ToList();

					if (pendingMigrationsList.Any())
					{
						logger.LogInformation("📋 Found {Count} pending migrations: {Migrations}",
							pendingMigrationsList.Count, string.Join(", ", pendingMigrationsList));

						try
						{
							logger.LogInformation("⚡ Applying pending migrations...");
							await context.Database.MigrateAsync();
							logger.LogInformation("✅ All pending migrations applied successfully");
						}
						catch (Exception migrationEx)
						{
							logger.LogError(migrationEx, "❌ Failed to apply migrations");

							// If migrations fail, try to ensure database is created
							logger.LogInformation("🔧 Attempting to ensure database is created as fallback...");
							await context.Database.EnsureCreatedAsync();
							logger.LogInformation("✅ Database ensured created as fallback");
						}
					}
					else
					{
						logger.LogInformation("✅ Database is up to date - no pending migrations");
					}
				}
				catch (Exception migrationCheckEx)
				{
					logger.LogWarning(migrationCheckEx, "⚠️ Could not check for pending migrations, but database connection is available");
				}
			}

			// Seed initial data
			logger.LogInformation("🌱 Starting data seeding...");
			// TODO: Re-implement DataSeeder
			// await DataSeeder.SeedAsync(context, userManager, roleManager, logger);
			logger.LogInformation("✅ Database initialization completed successfully");
			var adminEmail = "Admin@gmail.com";
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
			logger.LogError(ex, "❌ An error occurred while initializing the database");
			throw;
		}
	}
}
