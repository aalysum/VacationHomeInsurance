namespace VacationHome.PolicyService.Application.Exceptions
{
    public sealed class PaymentAttemptsExceededException : Exception
    {
        public PaymentAttemptsExceededException()
            : base("Maximum payment attempts exceeded.")
        {
        }
    }
}
