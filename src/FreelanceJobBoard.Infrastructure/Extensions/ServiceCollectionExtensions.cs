using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Identity;
using FreelanceJobBoard.Infrastructure.Data;
using FreelanceJobBoard.Infrastructure.Repositories;
using FreelanceJobBoard.Infrastructure.Services;
using FreelanceJobBoard.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FreelanceJobBoard.Infrastructure.Extensions;
public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool configureIdentity = true)
	{
		var connectionString = configuration.GetConnectionString("DefaultConnection")
			?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

		services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

		services.AddScoped<IUnitOfWork, UnitOfWork>();

		// Register HttpClient and CloudinaryService
		services.AddHttpClient<ICloudinaryService, CloudinaryService>();

		services.AddScoped<ICurrentUserService, CurrentUserService>();
		services.AddScoped<IEmailService, EmailService>();
		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

		services.AddScoped<INotificationService, NotificationService>();
		services.AddScoped<IDashboardService, DashboardService>();

		services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
		services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

		// Only configure Identity if requested (API layer needs it, Presentation configures its own)
		if (configureIdentity)
		{
			services.AddIdentity<ApplicationUser, IdentityRole>()
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddDefaultTokenProviders();
		}

		return services;
	}
}
