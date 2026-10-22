namespace VacationHome.Customer.Application.DTOs.Customers
{
    public class CreateCustomerResponse
    {
        public Guid CustomerId { get; init; }


        public bool AlreadyExisted { get; init; }
    }
}
