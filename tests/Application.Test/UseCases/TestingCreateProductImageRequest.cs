using Application.UseCases;
using Domain.Images;
using Domain.Products;

namespace Application.Test.UseCases;

public class TestingCreateProductImageRequest
{
    private readonly CreateProductImageRequest.Handler handler;
    private readonly Product.IRepository productRepository = Substitute.For<Product.IRepository>();
    private readonly IImageStorageServices imageStorageServices = Substitute.For<IImageStorageServices>();

    public TestingCreateProductImageRequest()
    {
        handler = new CreateProductImageRequest.Handler(productRepository, imageStorageServices);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var cancellationToken = CancellationToken.None;
        var request = CreateValidRequest();
        productRepository.Get(request.ProductId, cancellationToken)
                         .Returns((Product?)null);

        var result = await handler.Handle(request, cancellationToken);

        Assert.True(result.IsFailed);
        Assert.IsType<ResourceNotFoundError>(result.Errors[0]);
    }

    [Fact]
    public async Task Handle_ShouldStoreImage_AddToProduct_AndUpdateProduct()
    {
        var cancellationToken = CancellationToken.None;
        var request = CreateValidRequest();
        var expectedUri = "https://example.com/image.jpg";

        var product = new Product
        {
            Id = request.ProductId,
            Name = "Produto",
            CategoryId = Guid.NewGuid(),
            Price = 10,
            Images = []
        };

        productRepository.Get(request.ProductId, cancellationToken)
                         .Returns(product);

        imageStorageServices
            .StoreProductImage(request.ProductId, request.Image, cancellationToken)
            .Returns(expectedUri);

       
        var result = await handler.Handle(request, cancellationToken);

        
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedUri, result.Value.Uri);

        Assert.Single(product.Images);
        Assert.Equal(expectedUri, product.Images.First().Uri);

        await productRepository.Received(1).Update(Arg.Is<Product>(p =>
            p.Images.Count == 1 &&
            p.Images.First().Uri == expectedUri
        ), cancellationToken);
    }

    private static CreateProductImageRequest CreateValidRequest()
    {
        return new CreateProductImageRequest(
            ProductId: Guid.NewGuid(),
            Image: (new MemoryStream([1, 2, 3]), "image/jpeg")
        );
    }
}
