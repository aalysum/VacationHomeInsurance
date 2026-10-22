namespace VacationHome.ClaimService.Domain.Exceptions
{
    public sealed class DomainRuleException : Exception
    {
        public DomainRuleException(string message)
            : base(message)
        {
        }
    }
}
