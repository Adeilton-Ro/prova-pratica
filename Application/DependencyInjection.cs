using Application.UseCases;
using FluentValidation;
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

        services.AddValidatorsFromAssemblyContaining<CreateProductRequest.Validator>();

        return services;
    }
}
