using Domain;
using FluentResults;
using Mediator;

namespace Application.UseCases;

public record EnsureCategoryRequest(
    string Name,
    string? Description
) : IRequest<Result<EnsureCategoryRequest.Response>>
{
    public record Response(Guid Id);

    public class Handler : IRequestHandler<EnsureCategoryRequest, Result<Response>>
    {
        private readonly Category.IRepository categoryRepository;

        public Handler(
            Category.IRepository categoryRepository
        )
        {
            this.categoryRepository = categoryRepository;
        }

        public async ValueTask<Result<Response>> Handle(EnsureCategoryRequest request, CancellationToken cancellationToken)
        {
            var category = new Category
            {
                Name = request.Name,
                Description = request.Description
            };

            await categoryRepository.Ensure(
                category,
                cancellationToken
            );

            return new Response(category.Id);
        }
    }
}