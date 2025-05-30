using Domain.Images;
using Domain.Products;
using FluentResults;
using Mediator;

namespace Application.UseCases;

public record DeleteProductRequest(Guid Id) : IRequest<Result>
{
    public class Handler : IRequestHandler<DeleteProductRequest, Result>
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

        public async ValueTask<Result> Handle(DeleteProductRequest request, CancellationToken cancellationToken)
        {
            var product = await productRepository.Get(request.Id, cancellationToken);

            if (product is null) return new ResourceNotFoundError("Produto");

            await productRepository.Delete(product, cancellationToken);
            await imageStorageServices.DeleteProductImages(
                product.Images.Select(image => image.Uri), 
                cancellationToken
            );

            return Result.Ok();
        }
    }
}

