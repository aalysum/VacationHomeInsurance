namespace VacationHome.Auth.Application.DTOs.Internal
{
    public class CreateCustomerInternalRequest
    {
        public string AuthUserId { get; set; }
            = string.Empty;


        public string FirstName { get; set; }
            = string.Empty;


        public string LastName { get; set; }
            = string.Empty;


        public string PhoneNumber { get; set; }
            = string.Empty;
    }
}
