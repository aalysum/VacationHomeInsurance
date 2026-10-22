namespace VacationHome.ClaimService.Application.Common
{
    public sealed class PagedResult<T>
    {
        public IReadOnlyCollection<T> Items { get; set; }
            = Array.Empty<T>();


        public int PageNumber { get; set; }


        public int PageSize { get; set; }


        public long TotalCount { get; set; }


        public int TotalPages =>
            (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
