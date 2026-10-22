namespace VacationHome.ClaimService.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }


        string AuthUserId { get; }


        string Email { get; }


        string Role { get; }


        bool IsAuthenticated { get; }
    }
}
