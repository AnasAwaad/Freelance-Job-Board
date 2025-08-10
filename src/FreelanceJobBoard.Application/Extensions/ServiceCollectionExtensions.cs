using FluentValidation;
using FluentValidation.AspNetCore;
using FreelanceJobBoard.Application.Behaviors;
using FreelanceJobBoard.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;


namespace FreelanceJobBoard.Application.Extensions;
public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{

		var applicationAssembly = typeof(ServiceCollectionExtensions).Assembly;

		services.AddMediatR(config => 
		{
			config.RegisterServicesFromAssembly(applicationAssembly);
			config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
		});

		services.AddAutoMapper(applicationAssembly);

		services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
	}
}
