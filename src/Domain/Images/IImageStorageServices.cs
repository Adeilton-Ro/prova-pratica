namespace Domain.Images;

public interface IImageStorageServices
{
    Task<string> StoreProductImage(
        Guid productId,
        (Stream content, string contentType) images,
        CancellationToken cancellationToken = default
    );

    Task DeleteProductImage(
        string key, 
        CancellationToken cancellationToken = default
    );

    Task DeleteProductImages(
        IEnumerable<string> uris,
        CancellationToken cancellationToken = default
    );
}
