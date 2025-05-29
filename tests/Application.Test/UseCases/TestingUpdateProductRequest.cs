using Application.UseCases;
using Domain.Products;
using Domain;

namespace Application.Test.UseCases;

public class TestingUpdateProductRequest
{
    private readonly Product.IRepository productRepository;
    private readonly Category.IRepository categoryRepository;
    private readonly UpdateProductRequest.Handler handler;

    public TestingUpdateProductRequest()
    {
        productRepository = Substitute.For<Product.IRepository>();
        categoryRepository = Substitute.For<Category.IRepository>();
        handler = new UpdateProductRequest.Handler(productRepository, categoryRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var cancellationToken = CancellationToken.None;
        var request = CreateValidRequest();

        productRepository.Get(request.Id, cancellationToken).Returns((Product?)null);

        var result = await handler.Handle(request, cancellationToken);

        Assert.True(result.IsFailed);
        Assert.IsType<ResourceNotFoundError>(result.Errors[0]);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {

        var cancellationToken = CancellationToken.None;
        var request = CreateValidRequest();

        var product = new Product
        {
            Id = request.Id,
            Name = "Old Name",
            Price = 10,
            IsActive = false,
            Category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Some category",
                IsActive = true
            }
        };
        productRepository.Get(request.Id, cancellationToken).Returns(product);
        categoryRepository.Get(request.CategoryId, cancellationToken).Returns((Category?)null);

        var result = await handler.Handle(request, cancellationToken);

        Assert.True(result.IsFailed);
        Assert.IsType<ResourceNotFoundError>(result.Errors[0]);
    }

    [Fact]
    public async Task Handle_ShouldReturnBusinessLogicError_WhenCategoryIsInactive()
    {

        var cancellationToken = CancellationToken.None;
        var request = CreateValidRequest();

        var product = new Product
        {
            Id = request.Id,
            Name = "Old Name",
            Price = 10,
            IsActive = false,
            Category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Some category",
                IsActive = true
            }
        };
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Some category",
            IsActive = false
        };

        productRepository.Get(request.Id, cancellationToken).Returns(product);
        categoryRepository.Get(request.CategoryId, cancellationToken).Returns(category);

        var result = await handler.Handle(request, cancellationToken);

        Assert.True(result.IsFailed);
        Assert.IsType<BusinessLogicError>(result.Errors[0]);
    }

    [Fact]
    public async Task Handle_ShouldUpdateProductSuccessfully_WhenValidRequest()
    {

        var cancellationToken = CancellationToken.None;
        var request = CreateValidRequest();

        var product = new Product
        {
            Id = request.Id,
            Name = "Old Name",
            Price = 10,
            IsActive = false,
            Category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Some category",
                IsActive = true
            }
        };

        var category = new Category
        {
            Id = request.CategoryId,
            Name = "Some other category",
            IsActive = true
        };

        productRepository.Get(request.Id, cancellationToken).Returns(product);
        categoryRepository.Get(request.CategoryId, cancellationToken).Returns(category);

        var result = await handler.Handle(request, cancellationToken);

        Assert.False(result.IsFailed);
        await productRepository.Received(1).Update(Arg.Is<Product>(p =>
            p.Id == request.Id &&
            p.Name == request.Name &&
            p.Price == request.Price &&
            p.Category == category &&
            p.IsActive == request.IsActive
        ), cancellationToken);
    }

    private static UpdateProductRequest CreateValidRequest()
    {
        return new UpdateProductRequest(
            Id: Guid.NewGuid(),
            Name: "Produto Atualizado",
            CategoryId: Guid.NewGuid(),
            Price: 99.90m,
            IsActive: true
        );
    }
}
