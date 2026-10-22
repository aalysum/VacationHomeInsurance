namespace VacationHome.Customer.Application.DTOs.Internal
{
    public sealed class CustomerSummaryDto
    {
        public Guid CustomerId { get; set; }

        public string AuthUserId { get; set; } = string.Empty;


        public string FirstName { get; set; } =
            string.Empty;


        public string LastName { get; set; } =
            string.Empty;


        public string PhoneNumber { get; set; } =
            string.Empty;


        public bool IsActive { get; set; }
    }
}
