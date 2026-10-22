using VacationHome.PolicyService.Domain.Enums;


namespace VacationHome.PolicyService.Domain.Entities
{
    public sealed class PolicyDocument
    {
        public Guid Id { get; set; }


        public Guid PolicyId { get; set; }


        public int Version { get; set; }


        public string? StorageKey { get; set; }


        public string ContentType { get; set; } = "application/pdf";


        public long FileSizeBytes { get; set; }


        public string? Sha256Checksum { get; set; }


        public PolicyDocumentGenerationStatus Status { get; set; }


        public string? FailureCode { get; set; }


        public DateTime CreatedAtUtc { get; set; }


        public DateTime? GeneratedAtUtc { get; set; }


        public Policy Policy { get; set; } = null!;
    }
}