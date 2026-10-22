using VacationHome.Customer.Application.Common;


namespace VacationHome.Customer.Application.Exceptions
{
    public class ConcurrencyConflictException : ConflictException
    {
        public ConcurrencyConflictException()
            : base(
                ErrorCodes.ConcurrencyConflict,
                "The record was modified by another request. Refresh and retry.")
        {
        }
    }
}
