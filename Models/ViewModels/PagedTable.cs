namespace DairyManagementSystem.Models.ViewModels
{
    public class PagedTableState
    {
        public int TotalCount { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 15;
        public string Sort { get; init; } = string.Empty;
        public string Dir { get; init; } = "asc";
        public IReadOnlyDictionary<string, string?> ExtraRoute { get; init; } =
            new Dictionary<string, string?>();

        public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
        public bool IsSortedBy(string column) =>
            string.Equals(Sort, column, StringComparison.OrdinalIgnoreCase);
    }

    public class PagedTable<T> : PagedTableState
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    }
}
