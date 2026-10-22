namespace VacationHome.PolicyService.Application.DTOs.External
{
    public sealed record CustomerStatusDto
    {
        public Guid CustomerId
        {
            get;
            init;
        }


        public string AuthUserId
        {
            get;
            init;
        } = string.Empty;


        public bool IsActive
        {
            get;
            init;
        }
    }
}
