using Application.UseCases;
using Domain;
using NSubstitute;

namespace Application.Test.UseCases;

public class TestingUpdateCategoryHandler
{
    private readonly Category.IRepository categoryRepository = Substitute.For<Category.IRepository>();
    private readonly UpdateCategoryRequest.Handler handler;

    public TestingUpdateCategoryHandler()
    {
        handler = new UpdateCategoryRequest.Handler(categoryRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        var cancellationToken = CancellationToken.None;
        var request = CreateValidRequest();
        categoryRepository.Get(request.Id, cancellationToken).Returns((Category?)null);

        var result = await handler.Handle(request, cancellationToken);

        Assert.True(result.IsFailed);
        Assert.IsType<ResourceNotFoundError>(result.Errors[0]);
    }

    [Fact]
    public async Task Handle_ShouldReturnBusinessLogicError_WhenCategoryWithSameNameExists()
    {
        var cancellationToken = CancellationToken.None;
        var request = CreateValidRequest();
        var existingCategory = new Category { Id = request.Id, Name = "Categoria Original" };
        var otherCategory = new Category { Id = Guid.NewGuid(), Name = request.Name };

        categoryRepository.Get(request.Id, cancellationToken).Returns(existingCategory);
        categoryRepository.Get(request.Name, cancellationToken).Returns(otherCategory);

        var result = await handler.Handle(request, cancellationToken);

        Assert.True(result.IsFailed);
        Assert.IsType<BusinessLogicError>(result.Errors[0]);
    }

    [Fact]
    public async Task Handle_ShouldUpdateCategorySuccessfully_WhenRequestIsValid()
    {
        var cancellationToken = CancellationToken.None;
        var request = CreateValidRequest();
        var category = new Category
        {
            Id = request.Id,
            Name = "Antigo Nome",
            Description = "Desc",
            IsActive = false
        };

        categoryRepository.Get(request.Id, cancellationToken).Returns(category);
        categoryRepository.Get(request.Name, cancellationToken).Returns((Category?)null);

        var result = await handler.Handle(request, cancellationToken);

        Assert.True(result.IsSuccess);
        await categoryRepository.Received(1).Update(Arg.Is<Category>(c =>
            c.Name == request.Name &&
            c.Description == request.Description &&
            c.IsActive == request.IsActive
        ), cancellationToken);
    }

    [Fact]
    public async Task Handle_ShouldAllowUpdate_WhenCategoryWithSameNameIsSameEntity()
    {
        var cancellationToken = CancellationToken.None;
        var request = CreateValidRequest();

        var existingCategory = new Category
        {
            Id = request.Id,
            Name = request.Name,
            Description = "Old description",
            IsActive = false
        };

        categoryRepository.Get(request.Id, cancellationToken).Returns(existingCategory);
        categoryRepository.Get(request.Name, cancellationToken).Returns(existingCategory);

        var result = await handler.Handle(request, cancellationToken);

        Assert.True(result.IsSuccess);
        await categoryRepository.Received(1).Update(Arg.Is<Category>(c =>
            c.Name == request.Name &&
            c.Description == request.Description &&
            c.IsActive == request.IsActive
        ), cancellationToken);
    }

    private static UpdateCategoryRequest CreateValidRequest()
    {
        return new UpdateCategoryRequest(
            Id: Guid.NewGuid(),
            Name: "Nova Categoria",
            Description: "Descrição atualizada",
            IsActive: true
        );
    }
}
