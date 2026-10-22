using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Domain.Entities
{
    public class ClaimDocument
    {
        public Guid Id { get; set; }


        public Guid ClaimId { get; set; }


        public ClaimDocumentType DocumentType { get; set; }


        public string FileName { get; set; } = string.Empty;


        public string ContentType { get; set; } = string.Empty;


        public string StoragePath { get; set; } = string.Empty;


        public long FileSizeBytes { get; set; }


        public DateTime UploadedAtUtc { get; set; }


        public string UploadedBy { get; set; } = string.Empty;


        public Claim? Claim { get; set; }
    }
}
