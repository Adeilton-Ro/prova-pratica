namespace Domain;

public class Category : Entity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public interface IRepository
    {
        Task Ensure(Category category, CancellationToken cancellationToken = default);
        public record QueryFilters(
            string? Name,
            string? Description,
            bool? Active
        );
        Task<IEnumerable<Category>> Get(QueryFilters queryFilters, CancellationToken cancellationToken = default);
        Task<Category?> Get(Guid id, CancellationToken cancellationToken = default);
        Task<Category?> Get(string name, CancellationToken cancellationToken = default);
        Task Update(Category category, CancellationToken cancellationToken = default);
    }
}