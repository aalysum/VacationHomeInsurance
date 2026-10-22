namespace VacationHome.ProductQuote.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }


        string? Role { get; }


        bool IsAuthenticated { get; }
    }
}
