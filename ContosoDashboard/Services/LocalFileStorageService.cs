using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace ContosoDashboard.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageRoot;

    public LocalFileStorageService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var rootPath = configuration.GetValue<string>("DocumentStorage:RootPath") ?? "DocumentStorage";
        _storageRoot = Path.IsPathRooted(rootPath)
            ? rootPath
            : Path.GetFullPath(Path.Combine(environment.ContentRootPath, rootPath));

        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<string> UploadAsync(Stream fileStream, string destinationPath, CancellationToken cancellationToken = default)
    {
        if (Path.IsPathRooted(destinationPath) || destinationPath.Contains(".."))
        {
            throw new InvalidOperationException("Invalid destination path.");
        }

        var fullPath = Path.Combine(_storageRoot, destinationPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var output = File.Create(fullPath);
        await fileStream.CopyToAsync(output, cancellationToken);
        return destinationPath.Replace('\\', '/');
    }

    public Task<bool> DeleteAsync(string destinationPath, CancellationToken cancellationToken = default)
    {
        if (Path.IsPathRooted(destinationPath) || destinationPath.Contains(".."))
        {
            return Task.FromResult(false);
        }

        var fullPath = Path.Combine(_storageRoot, destinationPath);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult(false);
        }

        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    public Task<Stream?> DownloadAsync(string destinationPath, CancellationToken cancellationToken = default)
    {
        if (Path.IsPathRooted(destinationPath) || destinationPath.Contains(".."))
        {
            return Task.FromResult<Stream?>(null);
        }

        var fullPath = Path.Combine(_storageRoot, destinationPath);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        var stream = File.OpenRead(fullPath);
        return Task.FromResult<Stream?>(stream);
    }

    public Task<bool> ExistsAsync(string destinationPath, CancellationToken cancellationToken = default)
    {
        if (Path.IsPathRooted(destinationPath) || destinationPath.Contains(".."))
        {
            return Task.FromResult(false);
        }

        var fullPath = Path.Combine(_storageRoot, destinationPath);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task<string> GetDownloadUrlAsync(string destinationPath)
    {
        var cleanedPath = destinationPath.Replace('\\', '/');
        return Task.FromResult($"/documents/download/{cleanedPath}");
    }
}
