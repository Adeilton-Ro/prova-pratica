using Amazon.Runtime;
using Amazon.S3;
using Domain.Images;
using Domain.Products;
using Infrastructure.Database;
using Infrastructure.Database.Repositories;
using Infrastructure.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(
        IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder>? configureDbContextOptionsAction,
        Action<ImageStorageServices.Options, IServiceProvider> configureImageStorageServiceOptionsAction
    )
    {
        services.AddDbContext<ProvaPraticaDbContext>(configureDbContextOptionsAction);

        services
            .AddOptions<ImageStorageServices.Options>()
            .Configure(configureImageStorageServiceOptionsAction);
        services.AddScoped<IAmazonS3>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ImageStorageServices.Options>>().Value;

            return new AmazonS3Client(
                new BasicAWSCredentials(options.User, options.Password),
                new AmazonS3Config
                {
                    ServiceURL = options.BaseUrl,
                    ForcePathStyle = true
                }
            );
        });

        AddRepositories(services);
        AddServices(services);

        return services;
    }

    private static IServiceCollection AddRepositories(IServiceCollection services)
    {
        services.AddScoped<Product.IRepository, ProductRepository>();
        services.AddScoped<Product.Categories.IRepository, ProductCategoriesRepository>();

        return services;
    }
    
    private static IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddScoped<IImageStorageServices, ImageStorageServices>();
        return services;
    }
}
