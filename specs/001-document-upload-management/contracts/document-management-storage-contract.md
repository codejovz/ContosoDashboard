# Contract: Document Storage Abstraction

This contract defines the storage abstraction for document binary files. It enables secure local file storage today and a future migration path to cloud storage.

## Storage Interface

```csharp
public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string destinationPath, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string destinationPath, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadAsync(string destinationPath, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string destinationPath, CancellationToken cancellationToken = default);
    Task<string> GetDownloadUrlAsync(string destinationPath);
}
```

## Implementation Notes

- `destinationPath` is an application-controlled relative path, not a user-supplied path.
- The local implementation stores files in a secure path outside `wwwroot`.
- `UploadAsync` returns the stored path used by `Document.StoredFilePath`.
- `GetDownloadUrlAsync` may return a local app route or signed URL in a future cloud implementation.

## Local Implementation

The local strategy will use a path pattern such as:

```text
/{uploaderId}/{projectId or personal}/{guid}{extension}
```

This avoids filename collisions, prevents path traversal, and keeps the storage structure predictable.
