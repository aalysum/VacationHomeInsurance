namespace VacationHome.PolicyService.Application.DTOs.Documents
{
    public sealed record PolicyDocumentDownload(
        Stream Content,
        string FileName,
        string ContentType);
}