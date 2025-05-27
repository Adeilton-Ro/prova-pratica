using Domain.Images;
using Domain.Products;
using FluentResults;
using FluentValidation;
using Mediator;

namespace Application.UseCases;

public record CreateProductRequest(
    string Name,
    int CategoryId,
    decimal Price,
    IEnumerable<Image> Images
) : IRequest<Result<CreateProductRequest.Response>>
{
    public class Validator : AbstractValidator<CreateProductRequest>
    {
        public Validator()
        {
            RuleFor(product => product.Price)
                .GreaterThanOrEqualTo(0);

            RuleFor(product => product.CategoryId)
                .GreaterThan(0);

            RuleForEach(product => product.Images)
                .ChildRules(image => {
                    image.RuleFor(i => i.Extension).NotEmpty();
                    image.RuleFor(i => i.Name).NotEmpty();
                    image.RuleFor(i => i.Stream); // TODO: Adiciona validação de tipo por assinatura do binario
                });

            RuleFor(product => product.Name)
                .NotEmpty();
        }
    }

    public record Response(int Id);

    public class Handler : IRequestHandler<CreateProductRequest, Result<Response>>
    {
        private readonly Product.IRepository productRepository;
        private readonly Image.IRepository imageRepository;
        private readonly Product.Categories.IRepository categoriesRepository;

        public Handler(
            Product.IRepository productRepository,
            Image.IRepository imageRepository,
            Product.Categories.IRepository categoriesRepository
        )
        {
            this.productRepository = productRepository;
            this.imageRepository = imageRepository;
            this.categoriesRepository = categoriesRepository;
        }

        public async ValueTask<Result<Response>> Handle(CreateProductRequest request, CancellationToken cancellationToken)
        {
            var category = await categoriesRepository.Get(request.CategoryId, cancellationToken);

            if (category is null) return new ResourceNotFoundError("Categoria");

            var imagesUrisByImageName = await imageRepository.Create(request.Images, cancellationToken);

            var product = new Product
            {
                Category = category,
                Name = request.Name,
                Price = request.Price,
                Images = request.Images.Select(image => 
                    new Product.Image
                    {
                        Uri = imagesUrisByImageName[image.Name]
                    }
                ).ToArray()
            };

            await productRepository.Create(product, cancellationToken);

            return new Response(product.Id);
        }
    }
}

