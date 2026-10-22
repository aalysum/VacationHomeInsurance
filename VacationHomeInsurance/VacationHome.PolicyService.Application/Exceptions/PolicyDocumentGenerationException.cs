namespace VacationHome.PolicyService.Application.Exceptions
{
    public sealed class PolicyDocumentGenerationException : Exception
    {
        public const string Code = "POLICY_DOCUMENT_GENERATION_FAILED";

        public PolicyDocumentGenerationException(Exception innerException)
            : base(
                "Payment and policy issuance completed, but the policy document could not be generated or stored. Retry the request to recover the document; the payment will not be charged again.",
                innerException)
        {
        }
    }
}