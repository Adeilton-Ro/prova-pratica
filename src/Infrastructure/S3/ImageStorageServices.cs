using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Domain.Images;

namespace Infrastructure.S3;

public class ImageStorageServices : IImageStorageServices
{
    private readonly IAmazonS3 amazonS3;

    public ImageStorageServices(IAmazonS3 amazonS3)
    {
        this.amazonS3 = amazonS3;
    }

    const string ProductBucketName = "products";
    
    public async Task<string> StoreProductImage(
        Guid productId,
        (Stream content, string contentType) image,
        CancellationToken cancellationToken = default
    )
    {
        await EnsurerBucket(ProductBucketName, cancellationToken);

        var key = $"{productId}/{Guid.NewGuid()}";

        var (content, contentType) = image;

        await amazonS3.PutObjectAsync(
            new()
            {
                BucketName = ProductBucketName,
                Key = key,
                InputStream = content,
                ContentType = contentType
            },
            cancellationToken
        );

        return key;
    }

    private async Task EnsurerBucket(string bucketName, CancellationToken cancellationToken = default)
    {

        var exists = await AmazonS3Util.DoesS3BucketExistV2Async(amazonS3, bucketName);
        if (!exists)
        {
            await amazonS3.PutBucketAsync(new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true
            }, cancellationToken);

            var policyJson = $$"""
            {
              "Version": "2025-01-01",
              "Statement": [
                {
                  "Effect": "Allow",
                  "Principal": "*",
                  "Action": ["s3:GetObject"],
                  "Resource": ["arn:aws:s3:::{{bucketName}}/*"]
                }
              ]
            }
            """;

            var request = new PutBucketPolicyRequest
            {
                BucketName = bucketName,
                Policy = policyJson
            };

            await amazonS3.PutBucketPolicyAsync(request, cancellationToken);
        }
    }

    public class Options
    {
        public required string BaseUrl { get; set; }
        public required string User { get; set; }
        public required string Password { get; set; }
    }
}
