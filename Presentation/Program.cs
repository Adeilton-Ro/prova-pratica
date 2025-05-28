using Application.UseCases;
using Infrastructure.Database;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Presentation;
using Presentation.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

Application.DependencyInjection.AddDependencies(builder.Services);
Infrastructure.DependencyInjection.AddDependencies(
    builder.Services,
    (sp, opt) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();

        var databaseUser = configuration.GetValue<string>("DATABASE_USER")
            ?? throw new InvalidOperationException("Variavel DATABASE_USER precisa ter um valor definido");
        var databasePassword = configuration.GetValue<string>("DATABASE_PASSWORD")
            ?? throw new InvalidOperationException("Variavel DATABASE_PASSWORD precisa ter um valor definido");

        var partialConnectionString = configuration.GetConnectionString(nameof(ProvaPraticaDbContext))
            ?? throw new InvalidOperationException("ConnectionString:ProvaPraticaDbContext não foi definida");

        var connectionString = $"{partialConnectionString} " +
                               $"User ID={databaseUser}; " +
                               $"Password={databasePassword};";

        opt.UseNpgsql(connectionString);
    },
    (opt, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();

        opt.BaseUrl = configuration.GetValue<string>("AmazonS3:BaseUrl")!
            ?? throw new InvalidOperationException("AmazonS3:BaseUrl não foi definida"); ;
        opt.User = configuration.GetValue<string>("MINIO_USER")!
            ?? throw new InvalidOperationException("MINIO_USER não foi definida"); ;
        opt.Password = configuration.GetValue<string>("MINIO_PASSWORD")!
            ?? throw new InvalidOperationException("MINIO_PASSWORD não foi definida"); ;
    }
);

DependencyInjection.AddAuth(builder.Services);

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithFavicon("https://www.ma9.com.br/static/img/ma9-favicon.png");
    });

    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider.GetRequiredService<ProvaPraticaDbContext>();

    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
}

app.UseHttpsRedirection();

var api = app.MapGroup("api")
            .ProducesProblem(StatusCodes.Status500InternalServerError);

api.MapProducts();
api.MapCategory();

app.Run();
