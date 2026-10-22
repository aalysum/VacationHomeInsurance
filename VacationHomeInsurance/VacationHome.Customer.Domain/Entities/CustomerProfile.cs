namespace VacationHome.Customer.Domain.Entities
{
    public class CustomerProfile
    {
        public Guid Id { get; set; }


        public Guid CustomerId { get; set; }


        public DateTime? DateOfBirth { get; set; }


        public string? AddressLine1 { get; set; }


        public string? AddressLine2 { get; set; }


        public string? City { get; set; }


        public string? State { get; set; }


        public string? PostalCode { get; set; }


        public string? Country { get; set; }


        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;


        public DateTime? UpdatedAtUtc { get; set; }


        public Customers Customer { get; set; } = null!;
    }
}
