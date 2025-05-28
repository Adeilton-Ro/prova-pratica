using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories;

class ProductCategoriesRepository : Product.Categories.IRepository
{
    private readonly ProvaPraticaDbContext context;

    public ProductCategoriesRepository(ProvaPraticaDbContext context)
    {
        this.context = context;
    }

    public async Task Create(Product.Categories category, CancellationToken cancellationToken = default)
    {
        await context.AddAsync(category, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product.Categories>> Get(CancellationToken cancellationToken = default)
    {
        return await context.ProductCategories.ToListAsync(cancellationToken: cancellationToken);
    }

    public Task<Product.Categories?> Get(Guid id, CancellationToken cancellationToken = default)
    {
        return context.ProductCategories.FirstOrDefaultAsync(productCategory => productCategory.Id == id, cancellationToken);
    }
}
