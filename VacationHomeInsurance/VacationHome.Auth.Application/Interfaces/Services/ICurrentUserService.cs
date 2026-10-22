namespace VacationHome.Auth.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        string UserId { get; }


        string Email { get; }


        string Role { get; }


        string CorrelationId { get; }


        bool IsAuthenticated { get; }
    }
}
