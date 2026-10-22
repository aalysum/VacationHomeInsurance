namespace VacationHome.Customer.Application.DTOs.Customers
{


    public class CreateCustomerRequest
    {
        public string AuthUserId { get; set; } = string.Empty;


        public string FirstName { get; set; } = string.Empty;


        public string LastName { get; set; } = string.Empty;


        public string PhoneNumber { get; set; } = string.Empty;
    }
}
