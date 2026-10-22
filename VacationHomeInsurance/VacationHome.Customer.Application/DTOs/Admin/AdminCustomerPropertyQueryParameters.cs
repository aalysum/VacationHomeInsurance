using VacationHome.Customer.Application.Common;


namespace VacationHome.Customer.Application.DTOs.Admin
{
    public class AdminCustomerPropertyQueryParameters
    {
        public int PageNumber { get; set; }
            = PaginationConstants.DefaultPageNumber;


        public int PageSize { get; set; }
            = PaginationConstants.DefaultPageSize;


        public bool IncludeArchived { get; set; }
            = true;


        public string SortBy { get; set; }
            = "createdAtUtc";


        public string SortDirection { get; set; }
            = "desc";
    }
}
