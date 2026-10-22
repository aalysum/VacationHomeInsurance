using VacationHome.Customer.Domain.Enums;


namespace VacationHome.Customer.Application.DTOs.Admin
{
    public class AdminCustomerDetailsResponse
    {
        public Guid CustomerId { get; set; }


        public string AuthUserId { get; set; }
            = string.Empty;


        public string FirstName { get; set; }
            = string.Empty;


        public string LastName { get; set; }
            = string.Empty;


        public string PhoneNumber { get; set; }
            = string.Empty;


        public CustomerStatus Status { get; set; }


        public string? City { get; set; }


        public string? State { get; set; }


        public string? PostalCode { get; set; }


        public string? Country { get; set; }


        public int PropertyCount { get; set; }


        public DateTime CreatedAtUtc { get; set; }


        public DateTime? UpdatedAtUtc { get; set; }
    }
}
