using VacationHome.Customer.Domain.Enums;


namespace VacationHome.Customer.Application.DTOs.Customers
{
    public class UpdateCustomerStatusRequest
    {
        public CustomerStatus Status { get; set; }


        public string Reason { get; set; }
            = string.Empty;
    }
}
