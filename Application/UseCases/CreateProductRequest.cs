using Domain;
using Domain.Images;
using Domain.Products;
using FluentResults;
using FluentValidation;
using Mediator;

namespace Application.UseCases;

public record CreateProductRequest(
    string Name,
    Guid CategoryId,
    decimal Price,
    IEnumerable<(Stream content, string contentyType)> Images
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

            RuleForEach(product => product.Images)
                .ChildRules(image => {
                    
                });

            RuleFor(product => product.Name)
                .NotEmpty();
        }
    }

    public record Response(Guid Id);

    public class Handler : IRequestHandler<CreateProductRequest, Result<Response>>
    {
        private readonly Product.IRepository productRepository;
        private readonly IImageStorageServices imageStorageServices;
        private readonly Category.IRepository categoryRepository;

        public Handler(
            Product.IRepository productRepository,
            IImageStorageServices imageStorageServices,
            Category.IRepository categoryRepository
        )
        {
            this.productRepository = productRepository;
            this.imageStorageServices = imageStorageServices;
            this.categoryRepository = categoryRepository;
        }

        public async ValueTask<Result<Response>> Handle(CreateProductRequest request, CancellationToken cancellationToken)
        {
            var category = await categoryRepository.Get(request.CategoryId, cancellationToken);

            if (category is null) return new ResourceNotFoundError("Categoria");

            var productId = Guid.CreateVersion7();
            
            var imagesUris = await imageStorageServices.StoreProductImage(productId, request.Images, cancellationToken);

            var product = new Product
            {
                Category = category,
                Name = request.Name,
                Price = request.Price,
                Images = imagesUris.Select(imageUri => 
                    new Product.Image
                    {
                        Uri = imageUri
                    }
                ).ToArray()
            };

            await productRepository.Create(product, cancellationToken);

            return new Response(product.Id);
        }
    }
}

