namespace VacationHome.ClaimService.Domain.Entities
{
    public sealed class ClaimCoverage
    {
        public Guid Id { get; set; }


        public Guid ClaimId { get; set; }


        /*
         * Identifier of the Coverage from the issued
         * Policy snapshot.
         */
        public Guid CoverageId { get; set; }


        /*
         * Snapshot fields preserve the Coverage details
         * that existed when the Claim was submitted.
         */
        public string CoverageName { get; set; } =
            string.Empty;


        public bool IsMandatoryCoverage { get; set; }


        public decimal CoverageCharge { get; set; }


        public decimal CoverageLimitPercentage
        {
            get;
            set;
        }


        public decimal CoverageLimitAmount
        {
            get;
            set;
        }


        /*
         * Amount requested by the Customer against this
         * individual Policy Coverage.
         */
        public decimal RequestedAmount { get; set; }


        /*
         * Amount approved against this individual
         * Coverage. Null until a decision is made.
         */
        public decimal? ApprovedAmount { get; set; }


        public Claim Claim { get; set; } =
            null!;
    }
}
