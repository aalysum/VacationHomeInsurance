namespace VacationHome.PolicyService.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        Guid GetUserId();


        string GetEmail();


        bool IsAuthenticated();
    }
}
