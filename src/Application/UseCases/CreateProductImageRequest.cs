using Domain.Images;
using Domain.Products;
using FluentResults;
using FluentValidation;
using Mediator;

namespace Application.UseCases;

public record CreateProductImageRequest(
    Guid ProductId,
    (Stream content, string contentType) Image
) : IRequest<Result<CreateProductImageRequest.Response>>
{
    public class Validator : AbstractValidator<CreateProductImageRequest>
    {
        public Validator()
        {
            RuleFor(product => product.Image.content); // TODO: validar tipo pela assinatura do metodo
        }
    }

    public record Response(string Uri);

    public class Handler : IRequestHandler<CreateProductImageRequest, Result<Response>>
    {
        private readonly Product.IRepository productRepository;
        private readonly IImageStorageServices imageStorageServices;

        public Handler(
            Product.IRepository productRepository,
            IImageStorageServices imageStorageServices
        )
        {
            this.productRepository = productRepository;
            this.imageStorageServices = imageStorageServices;
        }

        public async ValueTask<Result<Response>> Handle(CreateProductImageRequest request, CancellationToken cancellationToken)
        {
            var product = await productRepository.Get(request.ProductId, cancellationToken);

            if (product is null) return new ResourceNotFoundError("Produto");

            var uri = await imageStorageServices.StoreProductImage(request.ProductId, request.Image, cancellationToken);

            product.Images.Add(new() { Uri = uri});

            await productRepository.Update(product, cancellationToken);

            return new Response(uri);
        }
    }
}