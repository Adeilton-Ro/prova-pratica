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

    public async Task Ensurer(Category category, CancellationToken cancellationToken = default)
    {
        var hasAlredyExist = context.Categories.Any(c => c.Name == category.Name);

        if (!hasAlredyExist)
        {
            await context.AddAsync(category, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<Category>> Get(CancellationToken cancellationToken = default)
    {
        return await context.Categories.ToListAsync(cancellationToken: cancellationToken);
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
