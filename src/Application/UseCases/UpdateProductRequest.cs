using Domain;
using Domain.Products;
using FluentResults;
using FluentValidation;
using Mediator;

namespace Application.UseCases;

public record UpdateProductRequest(
    Guid Id,
    string Name,
    Guid CategoryId,
    decimal Price,
    bool IsActive
) : IRequest<Result>
{
    public class Validator : AbstractValidator<UpdateProductRequest>
    {
        public Validator()
        {
            RuleFor(product => product.Price)
                .GreaterThanOrEqualTo(0);

            RuleFor(product => product.CategoryId)
                .NotEmpty();

            RuleFor(product => product.Name)
                .NotEmpty();
        }
    }

    public class Handler : IRequestHandler<UpdateProductRequest, Result>
    {
        private readonly Product.IRepository productRepository;
        private readonly Category.IRepository categoryRepository;

        public Handler(
            Product.IRepository productRepository,
            Category.IRepository categoryRepository
        )
        {
            this.productRepository = productRepository;
            this.categoryRepository = categoryRepository;
        }

        public async ValueTask<Result> Handle(UpdateProductRequest request, CancellationToken cancellationToken)
        {
            var product = await productRepository.Get(request.Id, cancellationToken);

            if (product is null) return new ResourceNotFoundError("Produto");

            var category = await categoryRepository.Get(request.CategoryId, cancellationToken);

            if (category is null) return new ResourceNotFoundError("Categoria");

            if (category.IsActive is false)
                return new BusinessLogicError("Categoria indisponível");

            product.Name = request.Name;
            product.Price = request.Price;
            product.Category = category;
            product.IsActive = request.IsActive;

            await productRepository.Update(product, cancellationToken);

            return Result.Ok();
        }
    }
}
