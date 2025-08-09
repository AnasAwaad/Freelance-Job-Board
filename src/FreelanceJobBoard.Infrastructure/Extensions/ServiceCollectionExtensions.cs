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
using IEmailSender = Microsoft.AspNetCore.Identity.UI.Services.IEmailSender;

namespace FreelanceJobBoard.Infrastructure.Extensions;
public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("DefaultConnection")
			?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

		services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

		services.AddScoped<IUnitOfWork, UnitOfWork>();
		services.AddScoped<ICloudinaryService, CloudinaryService>();
		services.AddScoped<ICurrentUserService, CurrentUserService>();
		services.AddScoped<IEmailSender, EmailSender>();
		services.AddScoped<IEmailService, EmailService>();

		services.AddScoped<INotificationService, NotificationService>();

		services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
		services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

		services.AddIdentity<ApplicationUser, IdentityRole>()
		.AddEntityFrameworkStores<ApplicationDbContext>()
		.AddDefaultTokenProviders();

		return services;
	}
}
