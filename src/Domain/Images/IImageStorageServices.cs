namespace Domain.Images;

public interface IImageStorageServices
{
    Task<string> StoreProductImage(
        Guid productId,
        (Stream content, string contentType) images,
        CancellationToken cancellationToken = default
    );
}
