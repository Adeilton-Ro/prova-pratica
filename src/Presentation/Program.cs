using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Presentation.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

Application.DependencyInjection.AddDependencies(builder.Services);
Infrastructure.DependencyInjection.AddDependencies(
    builder.Services,
    (sp, opt) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();

        var databaseUser = configuration.GetValue<string>("POSTGRES_USER")
            ?? throw new InvalidOperationException("Variavel POSTGRES_USER precisa ter um valor definido");
        var databasePassword = configuration.GetValue<string>("POSTGRES_PASSWORD")
            ?? throw new InvalidOperationException("Variavel POSTGRES_PASSWORD precisa ter um valor definido");

        var partialConnectionString = configuration.GetValue<string>($"ProvaPraticaDbContextConnectionStrings")
            ?? throw new InvalidOperationException("ProvaPraticaDbContextConnectionStrings n�o foi definida");

        var connectionString = $"{partialConnectionString} " +
                               $"User ID={databaseUser}; " +
                               $"Password={databasePassword};";

        opt.UseNpgsql(connectionString);
    },
    (opt, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();

        opt.BaseUrl = configuration.GetValue<string>("AmazonS3BaseUrl")!
            ?? throw new InvalidOperationException("AmazonS3BaseUrl n�o foi definida"); ;
        opt.User = configuration.GetValue<string>("MINIO_ROOT_USER")!
            ?? throw new InvalidOperationException("MINIO_ROOT_USER n�o foi definida"); ;
        opt.Password = configuration.GetValue<string>("MINIO_ROOT_PASSWORD")!
            ?? throw new InvalidOperationException("MINIO_ROOT_PASSWORD n�o foi definida"); ;
    }
);

builder.Services.AddOpenApi();

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
