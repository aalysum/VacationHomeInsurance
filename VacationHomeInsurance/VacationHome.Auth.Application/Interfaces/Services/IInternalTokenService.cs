namespace VacationHome.Auth.Application.Interfaces.Services
{
    public interface IInternalTokenService
    {
        string GenerateAuthServiceToken();


        string GenerateQuoteServiceToken();
    }
}
