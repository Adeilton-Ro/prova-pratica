using Application.UseCases;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Endpoints;

public static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProducts(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGroup("products");

        endpoint.MapPost("produtos", async (
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
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoint;
    }


    public record CreateProductEndpointRequest(
        string Name,
        Guid CategoryId,
        decimal Price,
        IFormFileCollection Images
    );
}
