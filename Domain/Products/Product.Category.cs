namespace Domain.Products;

public partial class Product
{
	public class Categories : Entity
	{
        public required string Name { get; set; }
        public string? Description { get; set; }

        public interface IRepository
        {
            Task Ensurer(Categories category, CancellationToken cancellationToken = default);
            Task<IEnumerable<Categories>> Get(CancellationToken cancellationToken = default);
            Task<Categories?> Get(Guid id, CancellationToken cancellationToken = default);
        }
    }
}
