namespace ContosoDashboard.Services;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string destinationPath, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string destinationPath, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadAsync(string destinationPath, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string destinationPath, CancellationToken cancellationToken = default);
    Task<string> GetDownloadUrlAsync(string destinationPath);
}
