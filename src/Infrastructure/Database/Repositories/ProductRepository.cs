using Domain;
using Domain.Products;
using Microsoft.EntityFrameworkCore;

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

    public Task<PaginatedEnumerable<Product>> Get(Product.IRepository.QueryFilters filters, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = context.Products.Include(product => product.Category);

        var (
            currentPage,
            pageSize,
            minPrice,
            maxPrice,
            categoryId,
            active
        ) = filters;

        if (active is not null)
            query = query.Where(product => product.IsActive == active);

        if (maxPrice is not null)
            query = query.Where(product => product.Price <= maxPrice);

        if (minPrice is not null)
            query = query.Where(product => product.Price >= minPrice);

        if (categoryId.HasValue)
            query = query.Where(product => product.CategoryId == categoryId);

        return Task.FromResult(filters.Paginate(query));
    }

    public Task<Product?> Get(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken: cancellationToken);
    }

    public async Task Update(Product product, CancellationToken cancellationToken = default)
    {
        context.Update(product);
        await context.SaveChangesAsync(cancellationToken);
    }
}
