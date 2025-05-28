namespace Domain.Products;

public partial class Product
{
    public interface IRepository
    {
        Task Create(Product product, CancellationToken cancellationToken = default);
    }
}
