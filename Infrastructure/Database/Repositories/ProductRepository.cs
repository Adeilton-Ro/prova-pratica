using Domain.Products;

namespace Infrastructure.Database.Repositories;

class ProductRepository : Product.IRepository
{
    private readonly ProvaPraticaDbContext context;

    public ProductRepository(ProvaPraticaDbContext context)
    {
        this.context = context;
    }

    public async Task Create(Product product, CancellationToken cancellationToken = default)
    {
        await context.AddAsync(product, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
