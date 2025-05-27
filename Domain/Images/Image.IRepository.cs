namespace Domain.Images;

public partial class Image
{
    public interface IRepository
    {
        Task<IDictionary<string, string>> Create(IEnumerable<Image> images, CancellationToken cancellationToken = default);
    }
}
