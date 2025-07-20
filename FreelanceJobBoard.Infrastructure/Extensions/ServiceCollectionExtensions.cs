using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Restaurants.Infrastructure.Extensions;
public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("DefaultConnection")
			?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

		services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

		return services;
	}
}
