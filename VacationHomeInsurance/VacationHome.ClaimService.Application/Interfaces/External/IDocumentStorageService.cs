namespace VacationHome.ClaimService.Application.Interfaces.External
{
    public interface IDocumentStorageService
    {
        Task<string> UploadAsync(
            Stream stream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default);


        Task DeleteAsync(
            string path,
            CancellationToken cancellationToken = default);
    }
}
