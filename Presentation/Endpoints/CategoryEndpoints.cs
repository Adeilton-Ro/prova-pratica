using Application.UseCases;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategory(this IEndpointRouteBuilder endpoint)
    {
        var category = endpoint.MapGroup("category");

        category.MapPost(string.Empty, async (
            [FromBody] EnsurerCategoryRequest request,
            [FromServices] ISender sender,
            CancellationToken cancellationToken
        ) =>
        {
            var result = await sender.Send(request, cancellationToken);

            return result.Serialize();
        })
            .Produces<EnsurerCategoryRequest.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return endpoint;
    }
}
