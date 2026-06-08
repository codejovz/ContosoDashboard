# Contract: Document Management Service

This contract defines the service-level interface for document upload and management.

## Service Interface

```csharp
public interface IDocumentService
{
    Task<List<Document>> GetDocumentsForUserAsync(int userId);
    Task<List<Document>> GetSharedDocumentsForUserAsync(int userId);
    Task<List<Document>> GetDocumentsForProjectAsync(int projectId, int requestingUserId);
    Task<Document?> GetDocumentByIdAsync(int documentId, int requestingUserId);

    Task<Document> UploadDocumentAsync(DocumentUploadRequest request, Stream fileStream, int userId);
    Task<bool> UpdateDocumentMetadataAsync(int documentId, DocumentMetadataUpdate update, int requestingUserId);
    Task<bool> ReplaceDocumentFileAsync(int documentId, Stream fileStream, string originalFileName, int requestingUserId);
    Task<bool> DeleteDocumentAsync(int documentId, int requestingUserId);

    Task<bool> ShareDocumentAsync(int documentId, DocumentShareRequest shareRequest, int requestingUserId);
    Task<bool> AttachDocumentToTaskAsync(int documentId, int taskId, int requestingUserId);

    Task<Stream?> DownloadDocumentAsync(int documentId, int requestingUserId);
    Task<DocumentSearchResult> SearchDocumentsAsync(DocumentSearchFilter filter, int requestingUserId);
}
```

## Data Contracts

```csharp
public class DocumentUploadRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class DocumentMetadataUpdate
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

public class DocumentShareRequest
{
    public List<int> UserIds { get; set; } = new();
    public int? ProjectId { get; set; }
    public string? Role { get; set; }
}

public class DocumentSearchFilter
{
    public string? Query { get; set; }
    public string? Category { get; set; }
    public int? ProjectId { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}

public class DocumentSearchResult
{
    public List<Document> Documents { get; set; } = new();
    public int TotalCount { get; set; }
}
```

## Contract Notes

- The service is responsible for authorization checks before returning or mutating documents.
- All file content handling is delegated to a storage abstraction to keep the document service focused on business rules.
- Search results must only include documents the requesting user is authorized to view.
