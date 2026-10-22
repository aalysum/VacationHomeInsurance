namespace VacationHome.ClaimService.Application.DTOs.Documents
{
    public sealed record PolicyDocumentContentDto(
        byte[] Content,
        string FileName,
        string ContentType);
}
