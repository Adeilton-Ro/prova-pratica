namespace Domain.Images;

public partial class Image
{
    public required Stream Stream { get; set; }
    public required string Name { get; set; }
    public required string Extension { get; set; }
}
