using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Application.DTOs.Documents
{
    public sealed class ClaimDocumentDto
    {
        public Guid Id { get; set; }


        public ClaimDocumentType DocumentType { get; set; }


        public string FileName { get; set; } = string.Empty;


        public string ContentType { get; set; } = string.Empty;


        public long FileSizeBytes { get; set; }


        public DateTime UploadedAtUtc { get; set; }
    }
}
