namespace Domain;

public record PaginatedEnumerable<T>(int CurrentPage = 1, int PageSize = 10)
{
    public IEnumerable<T> Items { get; private set; } = [];
    public int TotalItems { get; private set; }
    private int QuantityItemsToSkip => (CurrentPage - 1) * PageSize;

    private PaginatedEnumerable(
        int currentPage,
        int pageSize,
        int totalItems,
        IEnumerable<T> paginatedValues
    )
        : this(currentPage, pageSize)
    {
        TotalItems = totalItems;
        Items = paginatedValues;
    }

    public PaginatedEnumerable<T> Paginate(IEnumerable<T> allItems)
    {
        TotalItems = allItems.Count();
        Items = allItems.Skip(QuantityItemsToSkip).Take(PageSize);

        return this;
    }

    public PaginatedEnumerable<TNew> Select<TNew>(Func<T, TNew> selector)
    {
        return new PaginatedEnumerable<TNew>(
            CurrentPage,
            PageSize,
            TotalItems,
            Items.Select(selector)
        );
    }
}
