namespace VacationHome.ClaimService.Application.DTOs.Documents
{
    public sealed class PolicyDocumentMetadataDto
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? GeneratedAtUtc { get; set; }
    }
}
