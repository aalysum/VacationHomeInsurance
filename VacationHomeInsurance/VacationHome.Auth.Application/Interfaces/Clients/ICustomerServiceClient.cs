namespace VacationHome.Auth.Application.Interfaces.Clients
{
    public interface ICustomerServiceClient
    {
        Task CreateCustomerAsync(
            string authUserId,
            string firstName,
            string lastName,
            string phoneNumber,
            string correlationId,
            CancellationToken cancellationToken);


        Task SynchronizeCustomerStatusAsync(
            string authUserId,
            string status,
            string reason,
            string correlationId,
            CancellationToken cancellationToken);
    }
}
