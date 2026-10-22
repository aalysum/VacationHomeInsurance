using VacationHome.Auth.Application.DTOs;


namespace VacationHome.Auth.Application.Interfaces.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveClaimsManagerDocumentAsync(
            ClaimsManagerDocumentDto document,
            CancellationToken cancellationToken);


        Task DeleteClaimsManagerDocumentAsync(
            string path,
            CancellationToken cancellationToken);


        Task<StoredFileContent>
            OpenClaimsManagerDocumentAsync(
                string path,
                Guid applicationId,
                CancellationToken cancellationToken);
    }
}
