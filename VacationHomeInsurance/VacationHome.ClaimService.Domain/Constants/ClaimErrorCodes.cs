namespace VacationHome.ClaimService.Domain.Constants
{
    public static class ClaimErrorCodes
    {
        public const string ClaimNotFound = "CLAIM_NOT_FOUND";
        public const string PolicyNotFound = "POLICY_NOT_FOUND";
        public const string ClaimAlreadyClosed = "CLAIM_ALREADY_CLOSED";
        public const string ClaimAlreadyProcessed = "CLAIM_ALREADY_PROCESSED";
        public const string InvalidStatusTransition = "INVALID_STATUS_TRANSITION";
        public const string DuplicateClaim = "DUPLICATE_CLAIM";
    }
}
