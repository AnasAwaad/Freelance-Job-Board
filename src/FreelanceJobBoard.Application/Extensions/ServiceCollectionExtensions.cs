using FluentValidation;
using FluentValidation.AspNetCore;
using FreelanceJobBoard.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;


namespace FreelanceJobBoard.Application.Extensions;
public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{

		var applicationAssembly = typeof(ServiceCollectionExtensions).Assembly;

		services.AddMediatR(config => config.RegisterServicesFromAssembly(applicationAssembly));

		services.AddAutoMapper(applicationAssembly);

		services.AddValidatorsFromAssembly(applicationAssembly)
			.AddFluentValidationAutoValidation();



        return services;
	}
}
