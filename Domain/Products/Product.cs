using Domain.Images;

namespace Domain.Products;

public partial class Product : Entity
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public required Categories Category { get; set; }
    public IEnumerable<Image> Images { get; set; } = [];

    public class Image : Entity
    {
        public required string Uri { get; set; }
    }
}
