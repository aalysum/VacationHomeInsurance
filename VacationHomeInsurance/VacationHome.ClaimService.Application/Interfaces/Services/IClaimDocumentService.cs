using VacationHome.ClaimService.Application.DTOs.Documents;
using VacationHome.ClaimService.Domain.Enums;
using Microsoft.AspNetCore.Http;


namespace VacationHome.ClaimService.Application.Interfaces.Services
{
    public interface IClaimDocumentService
    {


        Task<IReadOnlyCollection<ClaimDocumentDto>> GetDocumentsAsync(
            Guid claimId,
            CancellationToken cancellationToken = default);


        Task<ClaimDocumentDto> GetDocumentAsync(
            Guid claimId,
            Guid documentId,
            CancellationToken cancellationToken = default);

        Task<(Stream Content, string ContentType, string FileName)> GetDocumentContentAsync(
            Guid claimId,
            Guid documentId,
            CancellationToken cancellationToken = default);


        Task DeleteDocumentAsync(
            Guid claimId,
            Guid documentId,
            CancellationToken cancellationToken = default);


        Task<ClaimDocumentDto> AddDocumentAsync(
        Guid claimId,
        ClaimDocumentType documentType,
        IFormFile file,
        CancellationToken cancellationToken = default);
    }
}
