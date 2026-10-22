using VacationHome.Customer.Domain.Enums;


namespace VacationHome.Customer.Application.DTOs.Customers
{
    public class CustomerProfileResponse
    {
        public Guid CustomerId { get; set; }


        public string FirstName { get; set; }
            = string.Empty;


        public string LastName { get; set; }
            = string.Empty;


        public string PhoneNumber { get; set; }
            = string.Empty;


        public CustomerStatus Status { get; set; }


        public DateTime? DateOfBirth { get; set; }


        public string? AddressLine1 { get; set; }


        public string? AddressLine2 { get; set; }


        public string? City { get; set; }


        public string? State { get; set; }


        public string? PostalCode { get; set; }


        public string? Country { get; set; }
    }
}
