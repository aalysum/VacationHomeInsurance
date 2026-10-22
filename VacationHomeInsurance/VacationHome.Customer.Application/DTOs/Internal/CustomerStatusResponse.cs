namespace VacationHome.Customer.Application.DTOs.Internal
{
    public sealed class CustomerStatusResponse
    {
        public Guid CustomerId { get; set; }


        public string AuthUserId { get; set; }
            = string.Empty;


        public bool IsActive { get; set; }
    }
}
