using Domain;
using FluentResults;
using Mediator;

namespace Application.UseCases;

public record UpdateCategoryRequest(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive
) : IRequest<Result>
{
    public class Handler : IRequestHandler<UpdateCategoryRequest, Result>
    {
        private readonly Category.IRepository categoryRepository;

        public Handler(Category.IRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        public async ValueTask<Result> Handle(UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            var category = await categoryRepository.Get(request.Id, cancellationToken);

            if (category is null)
                return new ResourceNotFoundError("Categoria");

            var otherCategory = await categoryRepository.Get(request.Name, cancellationToken);

            if (otherCategory is not null && otherCategory.Id != category.Id)
                return new BusinessLogicError($"Já existe catégoria com nome: {request.Name}");

            category.Name = request.Name;
            category.Description = request.Description;
            category.IsActive = request.IsActive;

            await categoryRepository.Update(category, cancellationToken);

            return Result.Ok();
        }
    }
}
