using Domain.Products;
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
        private readonly Product.Categories.IRepository productCategoriesRepository;

        public Handler(
            Product.Categories.IRepository productCategoriesRepository
        )
        {
            this.productCategoriesRepository = productCategoriesRepository;
        }

        public async ValueTask<Result<Response>> Handle(EnsurerCategoryRequest request, CancellationToken cancellationToken)
        {
            var categoryId = Guid.CreateVersion7();

            await productCategoriesRepository.Ensurer(
                new()
                {
                    Id = categoryId,
                    Name = request.Name,
                    Description = request.Description
                },
                cancellationToken
            );

            return new Response(categoryId);
        }
    }
}