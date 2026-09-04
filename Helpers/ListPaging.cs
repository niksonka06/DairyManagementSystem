using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Helpers
{
    public static class ListPaging
    {
        public const int DefaultPageSize = 15;

        public static PagedTable<T> Apply<T>(
            IEnumerable<T> source,
            string? sort,
            string? dir,
            int page,
            IReadOnlyDictionary<string, Func<T, object?>> columns,
            string defaultSort,
            bool defaultDesc = false,
            Func<T, bool>? activeFirst = null,
            IDictionary<string, string?>? extraRoute = null,
            int pageSize = DefaultPageSize)
        {
            var columnsIgnoreCase = new Dictionary<string, Func<T, object?>>(columns, StringComparer.OrdinalIgnoreCase);
            var list = source as IList<T> ?? source.ToList();
            var sortKey = columnsIgnoreCase.ContainsKey(sort ?? string.Empty)
                ? columnsIgnoreCase.Keys.First(k => string.Equals(k, sort, StringComparison.OrdinalIgnoreCase))
                : (columnsIgnoreCase.ContainsKey(defaultSort) ? defaultSort : columnsIgnoreCase.Keys.First());
            var descending = !string.IsNullOrEmpty(dir)
                ? string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase)
                : defaultDesc;

            IEnumerable<T> ordered = list;
            if (activeFirst is not null)
            {
                ordered = ordered.OrderByDescending(activeFirst);
                ordered = descending
                    ? ((IOrderedEnumerable<T>)ordered).ThenByDescending(x => columnsIgnoreCase[sortKey](x), Comparer)
                    : ((IOrderedEnumerable<T>)ordered).ThenBy(x => columnsIgnoreCase[sortKey](x), Comparer);
            }
            else
            {
                ordered = descending
                    ? ordered.OrderByDescending(x => columnsIgnoreCase[sortKey](x), Comparer)
                    : ordered.OrderBy(x => columnsIgnoreCase[sortKey](x), Comparer);
            }

            var materialized = ordered.ToList();
            var total = materialized.Count;
            var size = pageSize < 1 ? DefaultPageSize : pageSize;
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)size));
            var current = page < 1 ? 1 : Math.Min(page, totalPages);
            var items = materialized.Skip((current - 1) * size).Take(size).ToList();

            return new PagedTable<T>
            {
                Items = items,
                TotalCount = total,
                Page = current,
                PageSize = size,
                Sort = sortKey,
                Dir = descending ? "desc" : "asc",
                ExtraRoute = extraRoute is null
                    ? new Dictionary<string, string?>()
                    : new Dictionary<string, string?>(extraRoute)
            };
        }

        private static readonly IComparer<object?> Comparer = Comparer<object?>.Create((a, b) =>
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a is null) return -1;
            if (b is null) return 1;
            if (a.GetType() == b.GetType() && a is IComparable comparable)
            {
                return comparable.CompareTo(b);
            }

            return string.Compare(Convert.ToString(a), Convert.ToString(b), StringComparison.CurrentCultureIgnoreCase);
        });
    }
}
