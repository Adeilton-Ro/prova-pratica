using Domain;
using FluentResults;
using Mediator;

namespace Application.UseCases;

public record QueryCategoriesRequest(
    Category.IRepository.QueryFilters QueryFilters
) : IRequest<Result<IEnumerable<QueryCategoriesRequest.Response>>>
{
    public record Response(
        Guid Id,
        string Name,
        string? Description,
        bool IsActive
    );

    public class Handler : IRequestHandler<QueryCategoriesRequest, Result<IEnumerable<Response>>>
    {
        private readonly Category.IRepository categoryRepository;

        public Handler(Category.IRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        public async ValueTask<Result<IEnumerable<Response>>> Handle(QueryCategoriesRequest request, CancellationToken cancellationToken)
        {
            var categories = await categoryRepository.Get(request.QueryFilters, cancellationToken);

            return categories.Select(category => 
                new Response(
                    category.Id,
                    category.Name,
                    category.Description,
                    category.IsActive
                )
            ).ToResult();
        }
    }
}
