using Application.UseCases;
using Domain.Images;
using Domain.Products;

namespace Application.Test.UseCases;

public class TestingDeleteProductRequest
{
    private readonly DeleteProductRequest.Handler handler;
    private readonly Product.IRepository productRepository = Substitute.For<Product.IRepository>();
    private readonly IImageStorageServices imageStorageServices = Substitute.For<IImageStorageServices>();

    public TestingDeleteProductRequest()
    {
        handler = new DeleteProductRequest.Handler(productRepository, imageStorageServices);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var cancellationToken = CancellationToken.None;
        var request = new DeleteProductRequest(Guid.NewGuid());

        productRepository.Get(request.Id, cancellationToken)
                         .Returns((Product?)null);

        var result = await handler.Handle(request, cancellationToken);

        Assert.True(result.IsFailed);
        Assert.IsType<ResourceNotFoundError>(result.Errors[0]);
    }

    [Fact]
    public async Task Handle_ShouldDeleteProduct_AndDeleteImages()
    {
        var cancellationToken = CancellationToken.None;
        var request = new DeleteProductRequest(Guid.NewGuid());

        var imageUris = new[] { "https://example.com/img1.jpg", "https://example.com/img2.jpg" };
        var product = new Product
        {
            Id = request.Id,
            Name = "Produto",
            CategoryId = Guid.NewGuid(),
            Price = 10,
            Images = imageUris.Select(uri => new Product.Image { Uri = uri }).ToList()
        };

        productRepository.Get(request.Id, cancellationToken)
                         .Returns(product);

        var result = await handler.Handle(request, cancellationToken);

        Assert.True(result.IsSuccess);

        await productRepository.Received(1).Delete(product, cancellationToken);
        await imageStorageServices.Received(1).DeleteProductImages(
            Arg.Is<IEnumerable<string>>(uris => uris.SequenceEqual(imageUris)),
            cancellationToken
        );
    }
}
