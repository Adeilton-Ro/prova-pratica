using Domain.Products;
using Infrastructure.Database;
using Infrastructure.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(
        IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder>? configureDbContextOptionsAction
    )
    {
        services.AddDbContext<ProvaPraticaDbContext>(configureDbContextOptionsAction);

        AddRepositories(services);

        return services;
    }

    private static IServiceCollection AddRepositories(IServiceCollection services)
    {
        services.AddScoped<Product.IRepository, ProductRepository>();
        services.AddScoped<Product.Categories.IRepository, ProductCategoriesRepository>();

        return services;
    }
}
