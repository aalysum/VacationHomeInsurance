namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed class RequestInformationRequest
    {
        public string Message { get; set; } = string.Empty;


        public string RowVersion { get; set; } = string.Empty;
    }
}
