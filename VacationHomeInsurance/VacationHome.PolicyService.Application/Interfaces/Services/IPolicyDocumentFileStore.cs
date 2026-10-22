namespace VacationHome.PolicyService.Application.Interfaces.Services
{
    public interface IPolicyDocumentFileStore
    {
        Task<IAsyncDisposable> AcquirePolicyLockAsync(
            Guid policyId,
            CancellationToken cancellationToken);

        string CreateStorageKey(
            Guid policyId,
            int version);

        Task WriteAtomicallyAsync(
            string storageKey,
            ReadOnlyMemory<byte> content,
            CancellationToken cancellationToken);

        Task<byte[]?> ReadAllBytesAsync(
            string storageKey,
            CancellationToken cancellationToken);

        Task<Stream?> OpenReadAsync(
            string storageKey,
            CancellationToken cancellationToken);

        Task QuarantineAsync(
            string storageKey,
            CancellationToken cancellationToken);
    }
}