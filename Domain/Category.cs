namespace Domain;

public class Category : Entity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public interface IRepository
    {
        Task Ensurer(Category category, CancellationToken cancellationToken = default);
        Task<IEnumerable<Category>> Get(CancellationToken cancellationToken = default);
        Task<Category?> Get(Guid id, CancellationToken cancellationToken = default);
        Task<Category?> Get(string name, CancellationToken cancellationToken = default);
        Task Update(Category category, CancellationToken cancellationToken = default);
    }
}