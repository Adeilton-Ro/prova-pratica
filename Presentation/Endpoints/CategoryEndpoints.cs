using Application.UseCases;
using Domain;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;

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

        category.MapPut(string.Empty, async (
            [FromBody] UpdateCategoryRequest request,
            [FromServices] ISender sender,
            CancellationToken cancellationToken
        ) =>
        {
            var result = await sender.Send(request, cancellationToken);

            return result.Serialize();
        })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        category.MapGet(string.Empty, async (
            [AsParameters] Category.IRepository.QueryFilters filters,
            [FromServices] ISender sender,
            CancellationToken cancellationToken
        ) =>
        {
            var result = await sender.Send(new QueryCategoriesRequest(filters), cancellationToken);

            return result.Serialize();
        });

        return endpoint;
    }
}
