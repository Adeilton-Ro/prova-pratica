using Domain;
using Domain.Products;
using FluentResults;
using FluentValidation;
using Mediator;

namespace Application.UseCases;

public record CreateProductRequest(
    string Name,
    Guid CategoryId,
    decimal Price
) : IRequest<Result<CreateProductRequest.Response>>
{
    public class Validator : AbstractValidator<CreateProductRequest>
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

    public record Response(Guid Id);

    public class Handler : IRequestHandler<CreateProductRequest, Result<Response>>
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

        public async ValueTask<Result<Response>> Handle(CreateProductRequest request, CancellationToken cancellationToken)
        {
            var category = await categoryRepository.Get(request.CategoryId, cancellationToken);

            if (category is null) return new ResourceNotFoundError("Categoria");

            if (category.IsActive is false)
                return new BusinessLogicError("Categoria indisponível");

            var productId = Guid.CreateVersion7();

            var product = new Product
            {
                Category = category,
                Name = request.Name,
                Price = request.Price
            };

            await productRepository.Create(product, cancellationToken);

            return new Response(product.Id);
        }
    }
}

