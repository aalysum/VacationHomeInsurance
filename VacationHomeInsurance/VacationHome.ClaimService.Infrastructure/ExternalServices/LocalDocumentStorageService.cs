using VacationHome.ClaimService.Application.Interfaces.External;


namespace VacationHome.ClaimService.Infrastructure.ExternalServices
{
    public sealed class LocalDocumentStorageService
        : IDocumentStorageService
    {
        public Task<string> UploadAsync(
            Stream stream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                $"uploads/{Guid.NewGuid()}_{fileName}");
        }


        public Task DeleteAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
