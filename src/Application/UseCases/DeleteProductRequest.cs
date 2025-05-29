using Domain.Products;
using FluentResults;
using Mediator;

namespace Application.UseCases;

public record DeleteProductRequest(Guid Id) : IRequest<Result>
{
    public class Handler : IRequestHandler<DeleteProductRequest, Result>
    {
        private readonly Product.IRepository productRepository;

        public Handler(Product.IRepository productRepository)
        {
            this.productRepository = productRepository;
        }

        public async ValueTask<Result> Handle(DeleteProductRequest request, CancellationToken cancellationToken)
        {
            var product = await productRepository.Get(request.Id, cancellationToken);

            if (product is null) return new ResourceNotFoundError("Produto");

            await productRepository.Delete(product, cancellationToken);

            return Result.Ok();
        }
    }
}

