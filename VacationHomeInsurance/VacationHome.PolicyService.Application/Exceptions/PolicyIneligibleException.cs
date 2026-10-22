namespace VacationHome.PolicyService.Application.Exceptions
{
    public sealed class PolicyIneligibleException : Exception
    {
        public PolicyIneligibleException(string message)
            : base(message)
        {
        }
    }
}
