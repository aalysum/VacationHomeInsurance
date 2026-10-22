namespace VacationHome.PolicyService.Application.Exceptions
{
    public sealed class ConcurrencyConflictException : Exception
    {
        public ConcurrencyConflictException()
            : base("The record was modified by another process.")
        {
        }
    }
}
