using Application.Behaviours;
using Application.UseCases;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(IServiceCollection services)
    {
        services.AddMediator(opts =>
        {
            opts.ServiceLifetime = ServiceLifetime.Scoped;
        });

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingPipelineBehaviour<,>));

        services.AddValidatorsFromAssemblyContaining<CreateProductRequest.Validator>();

        return services;
    }
}
