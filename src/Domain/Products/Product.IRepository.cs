namespace Domain.Products;

public partial class Product
{
    public interface IRepository
    {
        Task Create(Product product, CancellationToken cancellationToken = default);
        public record QueryFilters(
            int CurrentPage,
            int PageSize,
            decimal? MinPrice,
            decimal? MaxPrice,
            Guid? CategoryId,
            bool? Active
        ) : PaginatedEnumerable<Product>(CurrentPage, PageSize);
        Task<PaginatedEnumerable<Product>> Get(QueryFilters filters, CancellationToken cancellationToken = default);
        Task<Product?> Get(Guid Id, CancellationToken cancellationToken = default);
        Task Update(Product product, CancellationToken cancellationToken = default);
    }
}
