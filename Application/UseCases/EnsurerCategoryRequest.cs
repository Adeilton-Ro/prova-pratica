using Domain;
using FluentResults;
using Mediator;

namespace Application.UseCases;

public record EnsurerCategoryRequest(
    string Name,
    string? Description
) : IRequest<Result<EnsurerCategoryRequest.Response>>
{
    public record Response(Guid Id);

    public class Handler : IRequestHandler<EnsurerCategoryRequest, Result<Response>>
    {
        private readonly Category.IRepository categoryRepository;

        public Handler(
            Category.IRepository categoryRepository
        )
        {
            this.categoryRepository = categoryRepository;
        }

        public async ValueTask<Result<Response>> Handle(EnsurerCategoryRequest request, CancellationToken cancellationToken)
        {
            var category = new Category
            {
                Name = request.Name,
                Description = request.Description
            };

            await categoryRepository.Ensurer(
                category,
                cancellationToken
            );

            return new Response(category.Id);
        }
    }
}