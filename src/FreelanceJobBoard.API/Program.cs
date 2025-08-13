using FreelanceJobBoard.API.Hubs;
using FreelanceJobBoard.API.Middlewares;
using FreelanceJobBoard.Application.Extensions;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Identity;
using FreelanceJobBoard.Infrastructure.Data;
using FreelanceJobBoard.Infrastructure.Data.Seed;
using FreelanceJobBoard.Infrastructure.Extensions;
using FreelanceJobBoard.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;
using System.Text;

namespace FreelanceJobBoard.API
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
				.Build();

			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(configuration)
				.Enrich.FromLogContext()
				.CreateLogger();

			try
			{
				Log.Information("Starting FreelanceJobBoard API");

				var builder = WebApplication.CreateBuilder(args);


				builder.Services.AddCors(options =>
				{
					options.AddPolicy("Default", policy =>
					{
						policy.WithOrigins("https://localhost:7117")
							  .AllowAnyHeader()
							  .AllowAnyMethod()
							  .AllowCredentials();
					});
				});
				builder.Logging.ClearProviders();
				builder.Logging.AddSerilog(Log.Logger);

				#region Add services to the container

				builder.Logging.ClearProviders();
				builder.Logging.AddSerilog(Log.Logger);
				builder.Services.AddDistributedMemoryCache();

				builder.Services.AddHttpContextAccessor();

				builder.Services.AddScoped<ErrorHandlingMiddleware>();
				builder.Services.AddScoped<RequestResponseLoggingMiddleware>();
				builder.Services.AddDistributedMemoryCache();
				builder.Services.AddScoped<ErrorHandlingMiddleware>();
				builder.Services.AddScoped<RequestResponseLoggingMiddleware>();
				builder.Services.AddScoped<IAuthService, AuthService>();
				builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
				builder.Services.AddSignalR();

				// your app services
				builder.Services.AddScoped<INotificationService, NotificationService>();
				builder.Services
					.AddApplication()
					.AddInfrastructure(builder.Configuration);

				// JWT Authentication
				var jwtSettings = builder.Configuration.GetSection("JwtSettings");
				var secretKey = jwtSettings["SecretKey"];

				if (string.IsNullOrEmpty(secretKey))
					throw new Exception("JWT SecretKey is missing in configuration.");

				var key = Encoding.UTF8.GetBytes(secretKey);

				builder.Services.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				})
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateLifetime = true,
						ValidateIssuerSigningKey = true,
						ValidIssuer = jwtSettings["Issuer"],
						ValidAudience = jwtSettings["Audience"],
						IssuerSigningKey = new SymmetricSecurityKey(key),
						ClockSkew = TimeSpan.Zero,
						RoleClaimType = ClaimTypes.Role

					};
				});

				builder.Services.AddControllers();
				builder.Services.AddEndpointsApiExplorer();

				// Swagger with JWT Support
				builder.Services.AddSwaggerGen(options =>
				{
					options.SwaggerDoc("v1", new OpenApiInfo { Title = "FreelanceJobBoard API", Version = "v1" });

					options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
					{
						Name = "Authorization",
						Type = SecuritySchemeType.ApiKey,
						Scheme = "Bearer",
						BearerFormat = "JWT",
						In = ParameterLocation.Header,
						Description = "Enter 'Bearer' followed by your JWT token.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
					});

					options.AddSecurityRequirement(new OpenApiSecurityRequirement
					{
						{
							new OpenApiSecurityScheme
							{
								Reference = new OpenApiReference
								{
									Type = ReferenceType.SecurityScheme,
									Id = "Bearer"
								}
							},
							Array.Empty<string>()
						}
					});
				});

				#endregion

				var app = builder.Build();

				#region Configure the HTTP request pipeline

				app.UseMiddleware<RequestResponseLoggingMiddleware>();
				app.UseMiddleware<ErrorHandlingMiddleware>();
				app.UseMiddleware<RateLimitMiddleware>();

				if (app.Environment.IsDevelopment())
				{
					app.UseDeveloperExceptionPage();
					app.UseSwagger();
					app.UseSwaggerUI();
				}

				app.UseHttpsRedirection();

				app.UseAuthentication();
				app.UseAuthorization();

				#region Seed Roles and Users
				var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
				using var scope = scopeFactory.CreateScope();

				var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
				var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

				await DefaultRoles.SeedRoles(roleManager);
				await DefaultUsers.SeedUsers(userManager, dbContext);
				#endregion


				app.MapControllers();
				app.UseCors("Default");

				app.MapHub<NotificationHub>("/notifyHub");

				Log.Information("FreelanceJobBoard API started successfully");
				app.Run();

				#endregion
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Application terminated unexpectedly");
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}
	}
}
