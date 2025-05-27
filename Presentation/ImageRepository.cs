using Domain.Images;

namespace Presentation;

public class ImageRepository : Image.IRepository
{
    private readonly IWebHostEnvironment _environment;
    private const string ImageFolder = "Products";

    public ImageRepository(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<IDictionary<string, string>> Create(IEnumerable<Image> images, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, string>();

        var uploadPath = Path.Combine(_environment.WebRootPath, ImageFolder);

        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        foreach (var image in images)
        {
            var fileName = $"{Guid.NewGuid()}_{image.Name}.{image.Extension.TrimStart('.')}";
            var filePath = Path.Combine(uploadPath, fileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await image.Stream.CopyToAsync(fileStream, cancellationToken);

            var publicPath = Path.Combine("/", ImageFolder, fileName).Replace("\\", "/");
            result[image.Name] = publicPath;
        }

        return result;
    }
}

