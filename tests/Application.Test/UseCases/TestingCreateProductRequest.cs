using Application.UseCases;
using Domain;
using Domain.Images;
using Domain.Products;
using NSubstitute;

namespace Application.Test.UseCases;

public class TestingCreateProductRequest
{
    private readonly CreateProductRequest.Handler handler;
    private readonly Product.IRepository productRepository = Substitute.For<Product.IRepository>();
    private readonly IImageStorageServices imageStorageServices = Substitute.For<IImageStorageServices>();
    private readonly Category.IRepository categoryRepository = Substitute.For<Category.IRepository>();

    public TestingCreateProductRequest()
    {
        handler = new CreateProductRequest.Handler(productRepository, imageStorageServices, categoryRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        var cancellationToken = CancellationToken.None;
        var request = CreateValidRequest();
        categoryRepository.Get(request.CategoryId, cancellationToken)
            .Returns((Category?)null);

        var result = await handler.Handle(request, cancellationToken);

        Assert.True(result.IsFailed);
        Assert.IsType<ResourceNotFoundError>(result.Errors[0]);
    }

    [Fact]
    public async Task Handle_ShouldReturnBusinessLogicError_WhenCategoryIsInactive()
    {
        var cancellationToken = CancellationToken.None;
        var request = CreateValidRequest();
        categoryRepository.Get(request.CategoryId, cancellationToken)
            .Returns(new Category
            {
                IsActive = false,
                Name = string.Empty,
                Description = string.Empty,
                Id = Guid.Empty
            });

        var result = await handler.Handle(request, cancellationToken);

        Assert.True(result.IsFailed);
        Assert.IsType<BusinessLogicError>(result.Errors[0]);
    }

    [Fact]
    public async Task Handle_ShouldCreateProductSuccessfully_AndStoreImages_WhenProductIsCreated()
    {
        var cancellationToken = CancellationToken.None;
        var request = CreateValidRequest();

        var category = new Category
        {
            IsActive = true,
            Name = string.Empty,
            Description = string.Empty,
            Id = Guid.Empty
        };

        var imageUris = new[] { "https://example.com/image1.jpg" };

        categoryRepository.Get(request.CategoryId, cancellationToken).Returns(category);
        imageStorageServices.StoreProductImage(Arg.Any<Guid>(), request.Images, cancellationToken)
            .Returns(imageUris);

        var result = await handler.Handle(request, cancellationToken);

        Assert.True(result.IsSuccess);
        await productRepository.Received(1).Create(Arg.Is<Product>(p =>
            p.Name == request.Name &&
            p.Price == request.Price &&
            p.Images.First().Uri == imageUris[0]
        ), cancellationToken);
        await imageStorageServices.Received(1).StoreProductImage(Arg.Any<Guid>(), request.Images, cancellationToken);
    }

    private static CreateProductRequest CreateValidRequest()
    {
        return new CreateProductRequest(
            Name: "Produto Teste",
            CategoryId: Guid.NewGuid(),
            Price: 99.90m,
            Images: []
        );
    }
}
