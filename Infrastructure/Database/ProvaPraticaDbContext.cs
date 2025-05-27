using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class ProvaPraticaDbContext : DbContext
{
    public ProvaPraticaDbContext(DbContextOptions<ProvaPraticaDbContext> options) : base(options) {  }

    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<Product.Image> ProductImages { get; set; } = default!;
    public DbSet<Product.Categories> ProductCategories { get; set; } = default!;
}
