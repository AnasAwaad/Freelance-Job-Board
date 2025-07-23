using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Infrastructure.Data;
using FreelanceJobBoard.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FreelanceJobBoard.Infrastructure.Extensions;
public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("DefaultConnection")
			?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

		services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

		services.AddScoped<IUnitOfWork, UnitOfWork>();

		return services;
	}
}
