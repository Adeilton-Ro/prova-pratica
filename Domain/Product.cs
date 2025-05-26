namespace Domain;

public class Product : Entity
{
    public required string Nome { get; set; }
    public decimal Price { get; set; }
    public required string Category { get; set; }
    public required string ImageUri { get; set; }
}
