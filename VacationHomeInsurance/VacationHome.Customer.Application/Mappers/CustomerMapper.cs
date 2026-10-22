using VacationHome.Customer.Application.DTOs.Customers;
using VacationHome.Customer.Domain.Entities;


namespace VacationHome.Customer.Application.Mappers
{
    public static class CustomerMapper
    {
        public static CustomerProfileResponse ToResponse(
            Customers customer,
            CustomerProfile? profile)
        {
            return new CustomerProfileResponse
            {
                CustomerId = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                PhoneNumber = customer.PhoneNumber,
                Status = customer.Status,
                DateOfBirth = profile?.DateOfBirth,
                AddressLine1 = profile?.AddressLine1,
                AddressLine2 = profile?.AddressLine2,
                City = profile?.City,
                State = profile?.State,
                PostalCode = profile?.PostalCode,
                Country = profile?.Country
            };
        }
    }
}
