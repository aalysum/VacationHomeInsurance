namespace VacationHome.ProductQuote.Application.DTOs.Common
{
    public abstract class PageRequest
    {
        public int PageNumber { get; init; } = 1;


        public int PageSize { get; init; } = 10;


        public string? SortBy { get; init; }


        public bool SortDescending { get; init; } = true;
    }
}
