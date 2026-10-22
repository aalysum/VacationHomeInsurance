namespace VacationHome.ProductQuote.Application.Common
{
    public static class PaginationConstants
    {
        public const int DefaultPageNumber = 1;


        public const int DefaultPageSize = 10;


        public static readonly IReadOnlySet<int>
            AllowedPageSizes =
                new HashSet<int>
                {
                    10,
                    25,
                    50
                };
    }
}
