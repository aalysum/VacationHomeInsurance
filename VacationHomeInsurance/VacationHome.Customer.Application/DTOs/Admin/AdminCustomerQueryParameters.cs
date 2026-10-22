using VacationHome.Customer.Application.Common;
using VacationHome.Customer.Domain.Enums;


namespace VacationHome.Customer.Application.DTOs.Admin
{
    public class AdminCustomerQueryParameters
    {
        public int PageNumber { get; set; }
            = PaginationConstants.DefaultPageNumber;


        public int PageSize { get; set; }
            = PaginationConstants.DefaultPageSize;


        public string? Name { get; set; }


        public CustomerStatus? Status { get; set; }


        public string SortBy { get; set; }
            = "createdAtUtc";


        public string SortDirection { get; set; }
            = "desc";
    }
}
