using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories;

class CategoryRepository : Category.IRepository
{
    private readonly ProvaPraticaDbContext context;

    public CategoryRepository(ProvaPraticaDbContext context)
    {
        this.context = context;
    }

    public async Task Ensure(Category category, CancellationToken cancellationToken = default)
    {
        var categoryWithSameName = context.Categories.FirstOrDefault(c => c.Name == category.Name);

        if (categoryWithSameName is null)
        {
            category.Id = Guid.CreateVersion7();
            await context.AddAsync(category, cancellationToken);
        }
        else
        {
            categoryWithSameName.IsActive = category.IsActive;
            categoryWithSameName.Description = category.Description;

            context.Update(categoryWithSameName);
            category.Id = categoryWithSameName.Id;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> Get(
        Category.IRepository.QueryFilters queryFilters,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<Category> query = context.Categories;

        var (name, description, active) = queryFilters;

        if (name is not null)
            query = query.Where(category => category.Name.StartsWith(name));

        if (description is not null)
            query = query.Where(category => category.Description!.Contains(description));

        if (active is not null)
            query = query.Where(category => category.IsActive == active);

        return await query.ToArrayAsync(cancellationToken: cancellationToken);
    }

    public Task<Category?> Get(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Categories.FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
    }

    public Task<Category?> Get(string name, CancellationToken cancellationToken = default)
    {
        return context.Categories.FirstOrDefaultAsync(
            category => category.Name == name,
            cancellationToken: cancellationToken
        );
    }

    public async Task Update(Category category, CancellationToken cancellationToken = default)
    {
        context.Update(category);
        await context.SaveChangesAsync(cancellationToken);
    }
}
