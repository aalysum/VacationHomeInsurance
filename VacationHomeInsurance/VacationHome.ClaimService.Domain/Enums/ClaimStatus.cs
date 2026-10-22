namespace VacationHome.ClaimService.Domain.Enums
{
    public enum ClaimStatus
    {
        Submitted = 1,
        UnderReview = 2,
        AdditionalInformationRequested = 3,
        Approved = 4,
        Rejected = 5,
        Paid = 6,
        Closed = 7,
        Withdrawn = 8
    }
}
