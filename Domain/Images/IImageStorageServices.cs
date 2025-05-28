namespace Domain.Images;

public interface IImageStorageServices
{
    Task<IEnumerable<string>> StoreProductImage(
        Guid productId, 
        IEnumerable<(Stream content, string contentType)> images, 
        CancellationToken cancellationToken = default
    );
}
