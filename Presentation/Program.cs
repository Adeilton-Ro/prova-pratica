using Application.UseCases;
using Domain.Images;
using Infrastructure.Database;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Presentation;
using Scalar.AspNetCore;
using System.Text;

 var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            In = ParameterLocation.Header,
            Scheme = "bearer"
        };
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add(JwtBearerDefaults.AuthenticationScheme, securityScheme);

        var referenceScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Id = JwtBearerDefaults.AuthenticationScheme,
                Type = ReferenceType.SecurityScheme
            }
        };

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [referenceScheme] = []
        });

        return Task.CompletedTask;
    });
});

builder.Services
    .AddOptions<JwtBearerOptions>()
    .Configure<IConfiguration>((options, cfg) =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg.GetValue<string>("JWT_DECRYPT")!))
        };
    });

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

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider.GetRequiredService<ProvaPraticaDbContext>();

    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
}

app.UseHttpsRedirection();

var api = app.MapGroup("api");

api.MapPost("produtos", async (
    [FromForm] CreateProductEndpointRequest endpointRequest,
    [FromServices] ISender sender,
    CancellationToken cancellationToken
) =>
{
    var request = new CreateProductRequest(
        endpointRequest.Name,
        endpointRequest.CategoryId,
        endpointRequest.Price,
        endpointRequest.Images.Select(image => (image.OpenReadStream(), image.ContentType)).ToArray()
    );

    var result = await sender.Send(request, cancellationToken);

    return result.Serialize();
})
    .DisableAntiforgery()
    .Accepts<CreateProductEndpointRequest>("multipart/form-data")
    .Produces<CreateProductRequest.Response>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status403Forbidden)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

app.Run();

public record CreateProductEndpointRequest(
    string Name,
    Guid CategoryId,
    decimal Price,
    IFormFileCollection Images
);
