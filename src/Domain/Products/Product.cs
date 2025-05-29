namespace Domain.Products;

public partial class Product : Entity
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CategoryId { get; set; }
    public required Category Category { get; set; }
    public ICollection<Image> Images { get; set; } = [];

    public class Image : Entity
    {
        public required string Uri { get; set; }
    }
}
