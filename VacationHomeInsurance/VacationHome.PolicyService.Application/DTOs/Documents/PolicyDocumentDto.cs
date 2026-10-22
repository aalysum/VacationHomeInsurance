namespace VacationHome.PolicyService.Application.DTOs.Documents
{
    public sealed record PolicyDocumentDto(
        Guid Id,
        Guid PolicyId,
        int Version,
        string Status,
        string ContentType,
        long FileSizeBytes,
        string? Sha256Checksum,
        string? FailureCode,
        DateTime CreatedAtUtc,
        DateTime? GeneratedAtUtc);
}