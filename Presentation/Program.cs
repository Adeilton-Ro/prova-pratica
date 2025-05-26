using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapPost("produtos", (
    [FromBody] CreateProdutoRequest request,
    CancellationToken cancellationToken
) =>
{
    return new CreateProdutoRequestResponse(2);
})
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status401Unauthorized)
    .ProducesProblem(StatusCodes.Status403Forbidden)
    .ProducesProblem(StatusCodes.Status500InternalServerError)
    .RequireAuthorization();

app.Run();

public record CreateProdutoRequest(
    string Nome,
    string Categoria,
    decimal Preco
);

public record CreateProdutoRequestResponse(int Id);