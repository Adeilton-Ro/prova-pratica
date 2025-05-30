using Domain.Images;
using Domain.Products;
using FluentResults;
using Mediator;

namespace Application.UseCases;

public record DeleteProductImageRequest(
    Guid ProductId,
    string Uri
) : IRequest<Result>
{
    public class Handler : IRequestHandler<DeleteProductImageRequest, Result>
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

        public async ValueTask<Result> Handle(DeleteProductImageRequest request, CancellationToken cancellationToken)
        {
            var product = await productRepository.Get(request.ProductId, cancellationToken);

            if (product is null) return new ResourceNotFoundError("Produto");

            var imageToRemove = product.Images.FirstOrDefault(image => image.Uri == request.Uri);

            if (imageToRemove is null) return new ResourceNotFoundError("Uri da imagem");

            product.Images.Remove(imageToRemove);

            await productRepository.Update(product, cancellationToken);
            await imageStorageServices.DeleteProductImage(imageToRemove.Uri, cancellationToken);

            return Result.Ok();
        }
    }
}