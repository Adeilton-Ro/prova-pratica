using Application.UseCases;
using Domain;
using Domain.Products;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace Presentation.Endpoints;

public static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProducts(this IEndpointRouteBuilder endpoint)
    {
        var products = endpoint
            .MapGroup("products")
            .WithTags("Product");

        products.MapPost(string.Empty, async (
            [FromForm] CreateProductRequest request,
            [FromServices] ISender sender,
            CancellationToken cancellationToken
        ) =>
        {
            var result = await sender.Send(request, cancellationToken);

            return result.Serialize();
        })
            .Produces<CreateProductRequest.Response>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        products.MapGet(string.Empty, async (
            [AsParameters] Product.IRepository.QueryFilters filters,
            [FromServices] ISender sender,
            CancellationToken cancellationToken
        ) =>
        {
            var result = await sender.Send(new QueryProductsRequest(filters), cancellationToken);

            return result.Serialize();
        })
            .Produces<PaginatedEnumerable<QueryProductsRequest.Response>>(StatusCodes.Status200OK);

        products.MapPut("{id}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateProductEndpointRequest endpointRequest,
            [FromServices] ISender sender,
            CancellationToken cancellationToken
        ) =>
        {
            var request = new UpdateProductRequest(
                id,
                endpointRequest.Name,
                endpointRequest.CategoryId,
                endpointRequest.Price,
                endpointRequest.IsActive
            );

            var result = await sender.Send(request, cancellationToken);

            return result.Serialize();
        })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        products.MapDelete("{id}", async (
            [FromRoute] Guid id,
            [FromServices] ISender sender,
            CancellationToken cancellationToken
        ) =>
        {
            var result = await sender.Send(new DeleteProductRequest(id), cancellationToken);

            return result.Serialize();
        })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        products.MapPost("{id}/image", async (
            [FromRoute] Guid id,
            [FromForm] IFormFile image,
            [FromServices] ISender sender,
            CancellationToken cancellationToken
        ) =>
        {
            var request = new CreateProductImageRequest(
                id,
                (image.OpenReadStream(), image.ContentType)
            );

            var result = await sender.Send(request, cancellationToken);

            return result.Serialize();
        })
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<CreateProductRequest.Response>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return endpoint;
    }

    public record UpdateProductEndpointRequest(
        string Name,
        Guid CategoryId,
        decimal Price,
        bool IsActive
    );
}
