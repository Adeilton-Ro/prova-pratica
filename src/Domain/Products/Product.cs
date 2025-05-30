namespace Domain.Products;

public partial class Product : Entity
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public ICollection<Image> Images { get; set; } = [];

    public class Image : Entity
    {
        public required string Uri { get; set; }
    }
}
