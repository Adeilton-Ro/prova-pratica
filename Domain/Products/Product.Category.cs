namespace Domain.Products;

public partial class Product
{
	public class Categories : Entity
	{
        public required string Nome { get; set; }
        public string? Description { get; set; }

        public interface IRepository
        {
            Task Create(Categories category, CancellationToken cancellationToken = default);
            Task<IEnumerable<Categories>> Get(CancellationToken cancellationToken = default);
            Task<Categories?> Get(int id, CancellationToken cancellationToken = default);
        }
    }
}
