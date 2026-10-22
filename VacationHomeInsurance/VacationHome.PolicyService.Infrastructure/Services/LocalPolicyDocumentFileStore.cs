using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using VacationHome.PolicyService.Application.Configuration;
using VacationHome.PolicyService.Application.Interfaces.Services;


namespace VacationHome.PolicyService.Infrastructure.Services
{
    public sealed class LocalPolicyDocumentFileStore
        : IPolicyDocumentFileStore
    {
        private readonly string _storageRoot;


        public LocalPolicyDocumentFileStore(
            IOptions<PolicyDocumentOptions> options,
            IWebHostEnvironment environment)
        {
            string configuredPath =
                options.Value.StoragePath;

            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                throw new InvalidOperationException(
                    "PolicyDocuments:StoragePath must be configured.");
            }

            _storageRoot = Path.GetFullPath(
                Path.IsPathRooted(configuredPath)
                    ? configuredPath
                    : Path.Combine(
                        environment.ContentRootPath,
                        configuredPath));

            string? webRoot = environment.WebRootPath;
            if (!string.IsNullOrWhiteSpace(webRoot))
            {
                string normalizedWebRoot =
                    Path.GetFullPath(webRoot)
                        .TrimEnd(
                            Path.DirectorySeparatorChar,
                            Path.AltDirectorySeparatorChar);

                if (_storageRoot.Equals(
                        normalizedWebRoot,
                        GetPathComparison()) ||
                    _storageRoot.StartsWith(
                        normalizedWebRoot +
                        Path.DirectorySeparatorChar,
                        GetPathComparison()))
                {
                    throw new InvalidOperationException(
                        "Policy document storage must not be inside the web root.");
                }
            }
        }


        public string CreateStorageKey(
            Guid policyId,
            int version)
        {
            if (policyId == Guid.Empty || version < 1)
            {
                throw new ArgumentException(
                    "Policy identifier and positive document version are required.");
            }

            return Path.Combine(
                "policy",
                policyId.ToString("N"),
                $"v{version:D4}.pdf");
        }


        public async Task<IAsyncDisposable> AcquirePolicyLockAsync(
            Guid policyId,
            CancellationToken cancellationToken)
        {
            if (policyId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Policy identifier is required.",
                    nameof(policyId));
            }

            string lockDirectory =
                Path.Combine(_storageRoot, ".locks");
            Directory.CreateDirectory(lockDirectory);
            string lockPath = Path.Combine(
                lockDirectory,
                $"{policyId:N}.lock");

            DateTime expiresAtUtc = DateTime.UtcNow.AddSeconds(30);
            while (DateTime.UtcNow < expiresAtUtc)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var stream = new FileStream(
                        lockPath,
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.None,
                        bufferSize: 1,
                        FileOptions.Asynchronous |
                        FileOptions.DeleteOnClose);
                    return new FileLockLease(stream);
                }
                catch (IOException)
                {
                    await Task.Delay(
                        TimeSpan.FromMilliseconds(100),
                        cancellationToken);
                }
            }

            throw new TimeoutException(
                "A policy document operation is already in progress.");
        }


        public async Task WriteAtomicallyAsync(
            string storageKey,
            ReadOnlyMemory<byte> content,
            CancellationToken cancellationToken)
        {
            string finalPath = ResolvePath(storageKey);
            string directory = Path.GetDirectoryName(finalPath)!;
            Directory.CreateDirectory(directory);

            if (File.Exists(finalPath))
            {
                throw new IOException(
                    "The policy document destination already exists.");
            }

            string temporaryPath = Path.Combine(
                directory,
                $".{Path.GetFileName(finalPath)}.{Guid.NewGuid():N}.tmp");

            try
            {
                await using (var stream = new FileStream(
                    temporaryPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    FileOptions.Asynchronous |
                    FileOptions.WriteThrough))
                {
                    await stream.WriteAsync(
                        content,
                        cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }

                File.Move(temporaryPath, finalPath, overwrite: false);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }


        public async Task<byte[]?> ReadAllBytesAsync(
            string storageKey,
            CancellationToken cancellationToken)
        {
            string path = ResolvePath(storageKey);
            if (!File.Exists(path))
            {
                return null;
            }

            return await File.ReadAllBytesAsync(
                path,
                cancellationToken);
        }


        public Task<Stream?> OpenReadAsync(
            string storageKey,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string path = ResolvePath(storageKey);
            if (!File.Exists(path))
            {
                return Task.FromResult<Stream?>(null);
            }

            Stream stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                FileOptions.Asynchronous |
                FileOptions.SequentialScan);
            return Task.FromResult<Stream?>(stream);
        }


        public Task QuarantineAsync(
            string storageKey,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string path = ResolvePath(storageKey);
            if (File.Exists(path))
            {
                string quarantinePath =
                    $"{path}.quarantine-{Guid.NewGuid():N}";
                File.Move(path, quarantinePath, overwrite: false);
            }

            return Task.CompletedTask;
        }


        private string ResolvePath(string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey) ||
                Path.IsPathRooted(storageKey))
            {
                throw new InvalidOperationException(
                    "Invalid policy document storage key.");
            }

            string fullPath = Path.GetFullPath(
                Path.Combine(_storageRoot, storageKey));
            string normalizedRoot =
                _storageRoot.TrimEnd(Path.DirectorySeparatorChar) +
                Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(
                    normalizedRoot,
                    GetPathComparison()))
            {
                throw new InvalidOperationException(
                    "Policy document storage key escaped the configured storage root.");
            }

            return fullPath;
        }


        private static StringComparison GetPathComparison()
        {
            return OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
        }


        private sealed class FileLockLease : IAsyncDisposable
        {
            private readonly FileStream _stream;

            public FileLockLease(FileStream stream)
            {
                _stream = stream;
            }

            public ValueTask DisposeAsync()
            {
                return _stream.DisposeAsync();
            }
        }
    }
}