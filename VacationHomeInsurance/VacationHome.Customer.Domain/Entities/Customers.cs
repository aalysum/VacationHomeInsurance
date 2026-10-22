using VacationHome.Customer.Domain.Enums;


namespace VacationHome.Customer.Domain.Entities
{
    public class Customers
    {
        public Guid Id { get; set; }


        public string AuthUserId { get; set; } = string.Empty;


        public string FirstName { get; set; } = string.Empty;


        public string LastName { get; set; } = string.Empty;


        public string PhoneNumber { get; set; } = string.Empty;


        public CustomerStatus Status { get; set; }
            = CustomerStatus.Active;


        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;


        public DateTime? UpdatedAtUtc { get; set; }


        public CustomerProfile? Profile { get; set; }


        public ICollection<VacationProperty> VacationProperties { get; set; }
            = new List<VacationProperty>();
    }
}
