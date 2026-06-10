using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

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
