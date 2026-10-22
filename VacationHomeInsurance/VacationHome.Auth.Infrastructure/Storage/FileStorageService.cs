using Microsoft.Extensions.Options;
using VacationHome.Auth.Application.Common;
using VacationHome.Auth.Application.DTOs;
using VacationHome.Auth.Application.Exceptions;
using VacationHome.Auth.Application.Interfaces.Services;


namespace VacationHome.Auth.Infrastructure.Storage
{
    public sealed class FileStorageService
        : IFileStorageService
    {
        private const long MaxFileSize =
            10 * 1024 * 1024;


        private readonly FileStorageSettings
            _settings;


        public FileStorageService(
            IOptions<FileStorageSettings> settings)
        {
            _settings = settings.Value;
        }


        public async Task<string>
            SaveClaimsManagerDocumentAsync(
                ClaimsManagerDocumentDto document,
                CancellationToken cancellationToken)
        {
            cancellationToken
                .ThrowIfCancellationRequested();


            if (document.Content is null ||
                document.Content.Length == 0)
            {
                throw new BadRequestException(
                    "VERIFICATION_DOCUMENT_REQUIRED",
                    "Verification document is required.");
            }


            if (document.Content.LongLength >
                MaxFileSize)
            {
                throw new BadRequestException(
                    "VERIFICATION_DOCUMENT_TOO_LARGE",
                    "File size cannot exceed 10 MB.");
            }


            var extension =
                Path.GetExtension(
                        document.FileName)
                    .ToLowerInvariant();


            var allowedExtensions =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    ".pdf",
                    ".jpg",
                    ".jpeg",
                    ".png"
                };


            if (!allowedExtensions.Contains(
                    extension))
            {
                throw new BadRequestException(
                    "UNSUPPORTED_DOCUMENT_EXTENSION",
                    "Only PDF, JPG, JPEG, and PNG files are allowed.");
            }


            var normalizedContentType =
                document.ContentType
                    .Trim()
                    .ToLowerInvariant();


            var expectedContentType =
                extension switch
                {
                    ".pdf" =>
                        "application/pdf",


                    ".jpg" or ".jpeg" =>
                        "image/jpeg",


                    ".png" =>
                        "image/png",


                    _ =>
                        string.Empty
                };


            if (!string.Equals(
                    normalizedContentType,
                    expectedContentType,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException(
                    "INVALID_DOCUMENT_MIME_TYPE",
                    "The document MIME type does not match its file extension.");
            }


            if (!HasValidFileSignature(
                    extension,
                    document.Content))
            {
                throw new BadRequestException(
                    "INVALID_DOCUMENT_SIGNATURE",
                    "The verification document content does not match its file type.");
            }


            var storageRoot =
                GetStorageRoot();


            Directory.CreateDirectory(
                storageRoot);


            var storedFileName =
                $"{Guid.NewGuid():N}{extension}";


            var fullPath =
                Path.Combine(
                    storageRoot,
                    storedFileName);


            await File.WriteAllBytesAsync(
                fullPath,
                document.Content,
                cancellationToken);


            return fullPath;
        }


        public Task DeleteClaimsManagerDocumentAsync(
            string path,
            CancellationToken cancellationToken)
        {
            cancellationToken
                .ThrowIfCancellationRequested();


            if (string.IsNullOrWhiteSpace(path))
            {
                return Task.CompletedTask;
            }


            var fullPath =
                ValidateAndResolveStoredPath(path);


            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }


            return Task.CompletedTask;
        }


        public Task<StoredFileContent>
            OpenClaimsManagerDocumentAsync(
                string path,
                Guid applicationId,
                CancellationToken cancellationToken)
        {
            cancellationToken
                .ThrowIfCancellationRequested();


            if (string.IsNullOrWhiteSpace(path))
            {
                throw new NotFoundException(
                    "VERIFICATION_DOCUMENT_NOT_FOUND",
                    "Verification document was not found.");
            }


            var fullPath =
                ValidateAndResolveStoredPath(path);


            if (!File.Exists(fullPath))
            {
                throw new NotFoundException(
                    "VERIFICATION_DOCUMENT_NOT_FOUND",
                    "Verification document was not found.");
            }


            var extension =
                Path.GetExtension(fullPath)
                    .ToLowerInvariant();


            var contentType =
                extension switch
                {
                    ".pdf" =>
                        "application/pdf",


                    ".jpg" or ".jpeg" =>
                        "image/jpeg",


                    ".png" =>
                        "image/png",


                    _ =>
                        throw new BadRequestException(
                            "UNSUPPORTED_DOCUMENT_TYPE",
                            "The stored verification document type is unsupported.")
                };


            Stream stream =
                new FileStream(
                    fullPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 64 * 1024,
                    useAsync: true);


            var response =
                new StoredFileContent
                {
                    Content =
                        stream,


                    ContentType =
                        contentType,


                    DownloadFileName =
                        $"claims-manager-verification-" +
                        $"{applicationId}{extension}"
                };


            return Task.FromResult(response);
        }


        private string GetStorageRoot()
        {
            if (string.IsNullOrWhiteSpace(
                    _settings
                        .ClaimsManagerDocumentsPath))
            {
                throw new InvalidOperationException(
                    "Claims Manager document storage path is not configured.");
            }


            return Path.GetFullPath(
                _settings
                    .ClaimsManagerDocumentsPath);
        }


        private string ValidateAndResolveStoredPath(
            string path)
        {
            var storageRoot =
                GetStorageRoot();


            var fullPath =
                Path.GetFullPath(path);


            var rootWithSeparator =
                storageRoot.EndsWith(
                    Path.DirectorySeparatorChar)
                    ? storageRoot
                    : storageRoot +
                      Path.DirectorySeparatorChar;


            if (!fullPath.StartsWith(
                    rootWithSeparator,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ForbiddenException(
                    "INVALID_DOCUMENT_PATH",
                    "The verification document path is invalid.");
            }


            return fullPath;
        }


        private static bool HasValidFileSignature(
            string extension,
            byte[] content)
        {
            return extension switch
            {
                ".pdf" =>
                    content.Length >= 4 &&
                    content[0] == 0x25 &&
                    content[1] == 0x50 &&
                    content[2] == 0x44 &&
                    content[3] == 0x46,


                ".png" =>
                    content.Length >= 8 &&
                    content[0] == 0x89 &&
                    content[1] == 0x50 &&
                    content[2] == 0x4E &&
                    content[3] == 0x47 &&
                    content[4] == 0x0D &&
                    content[5] == 0x0A &&
                    content[6] == 0x1A &&
                    content[7] == 0x0A,


                ".jpg" or ".jpeg" =>
                    content.Length >= 3 &&
                    content[0] == 0xFF &&
                    content[1] == 0xD8 &&
                    content[2] == 0xFF,


                _ =>
                    false
            };
        }
    }
}
