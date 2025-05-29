using Domain;
using Domain.Products;
using FluentResults;
using Mediator;

namespace Application.UseCases;

public record QueryProductsRequest(
    Product.IRepository.QueryFilters Filters
) : IRequest<Result<PaginatedEnumerable<QueryProductsRequest.Response>>>
{
    public record Response(
        Guid Id,
        string Name,
        Guid CategoryId,
        decimal Price,
        IEnumerable<string> ImagesUris
    );

    public class Handler : IRequestHandler<QueryProductsRequest, Result<PaginatedEnumerable<Response>>>
    {
        private readonly Product.IRepository productRepository;

        public Handler(Product.IRepository productRepository)
        {
            this.productRepository = productRepository;
        }

        public async ValueTask<Result<PaginatedEnumerable<Response>>> Handle(QueryProductsRequest request, CancellationToken cancellationToken)
        {
            var products = await productRepository.Get(request.Filters, cancellationToken);

            return products.Select(product =>
                new Response(
                    product.Id,
                    product.Name,
                    product.CategoryId,
                    product.Price,
                    product.Images.Select(image => image.Uri)
                )
            );
        }
    }
}
