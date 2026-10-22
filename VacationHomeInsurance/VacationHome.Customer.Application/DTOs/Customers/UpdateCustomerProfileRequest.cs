namespace VacationHome.Customer.Application.DTOs.Customers
{
    public class UpdateCustomerProfileRequest
    {
        public DateTime? DateOfBirth { get; set; }


        public string? AddressLine1 { get; set; }


        public string? AddressLine2 { get; set; }


        public string? City { get; set; }


        public string? State { get; set; }


        public string? PostalCode { get; set; }


        public string? Country { get; set; }
    }
}
