namespace VacationHome.ClaimService.Domain.Entities
{
    public sealed class ClaimAssignmentHistory
    {
        public Guid Id { get; set; }


        public Guid ClaimId { get; set; }


        /*
         * Null for the first assignment.
         * String because this is an Auth ApplicationUser ID.
         */
        public string? PreviousClaimsManagerId
        {
            get;
            set;
        }


        public string? PreviousClaimsManagerName
        {
            get;
            set;
        }


        public string? PreviousClaimsManagerEmail
        {
            get;
            set;
        }


        public string ClaimsManagerId { get; set; } =
            string.Empty;


        public string ClaimsManagerName { get; set; } =
            string.Empty;


        public string ClaimsManagerEmail { get; set; } =
            string.Empty;


        /*
         * Null is permitted for the first assignment.
         * Reassignment will require a nonempty reason in
         * application business logic.
         */
        public string? Reason { get; set; }


        /*
         * Identity of the Admin who performed the assignment.
         */
        public string AssignedBy { get; set; } =
            string.Empty;


        public DateTime AssignedAtUtc { get; set; }


        public Claim Claim { get; set; } =
            null!;
    }
}
