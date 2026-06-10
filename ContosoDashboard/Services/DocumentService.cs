using Microsoft.EntityFrameworkCore;
using ContosoDashboard.Data;
using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _storage;
    private readonly INotificationService _notificationService;

    private static readonly Dictionary<string, string> _previewableTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "application/pdf", "pdf" },
        { "image/png", "png" },
        { "image/jpeg", "jpg" },
        { "image/jpg", "jpg" },
        { "text/plain", "txt" }
    };

    private static readonly HashSet<string> _supportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv", ".png", ".jpg", ".jpeg"
    };

    private const long MaxUploadSize = 25 * 1024 * 1024;

    public DocumentService(ApplicationDbContext context, IFileStorageService storage, INotificationService notificationService)
    {
        _context = context;
        _storage = storage;
        _notificationService = notificationService;
    }

    public async Task<List<Document>> GetDocumentsForUserAsync(int userId)
    {
        var documents = await _context.Documents
            .Include(d => d.Project)
            .Include(d => d.DocumentTags)
            .Where(d => d.UploaderId == userId)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();

        documents.ForEach(SetDownloadUrl);
        return documents;
    }

    public async Task<List<Document>> GetSharedDocumentsForUserAsync(int userId)
    {
        var authorizedProjectIds = await _context.ProjectMembers
            .Where(pm => pm.UserId == userId)
            .Select(pm => pm.ProjectId)
            .ToListAsync();

        var documents = await _context.Documents
            .Include(d => d.Project)
            .Include(d => d.DocumentTags)
            .Include(d => d.Shares)
            .Where(d => d.UploaderId == userId
                || (d.ProjectId.HasValue && authorizedProjectIds.Contains(d.ProjectId.Value))
                || d.Shares.Any(s => s.SharedWithUserId == userId || (s.SharedWithProjectId.HasValue && authorizedProjectIds.Contains(s.SharedWithProjectId.Value))))
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();

        documents.ForEach(SetDownloadUrl);
        return documents;
    }

    public async Task<List<Document>> GetDocumentsForProjectAsync(int projectId, int requestingUserId)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId);

        if (project == null)
        {
            return new List<Document>();
        }

        var isProjectMember = project.ProjectManagerId == requestingUserId
            || project.ProjectMembers.Any(pm => pm.UserId == requestingUserId);

        if (!isProjectMember)
        {
            return new List<Document>();
        }

        var documents = await _context.Documents
            .Include(d => d.Project)
            .Include(d => d.DocumentTags)
            .Where(d => d.ProjectId == projectId)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();

        documents.ForEach(SetDownloadUrl);
        return documents;
    }

    public async Task<Document?> GetDocumentByIdAsync(int documentId, int requestingUserId)
    {
        var document = await _context.Documents
            .Include(d => d.Project)
            .Include(d => d.DocumentTags)
            .Include(d => d.Shares)
            .Include(d => d.TaskDocuments)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            return null;
        }

        if (!await IsAuthorizedToViewDocumentAsync(document, requestingUserId))
        {
            return null;
        }

        SetDownloadUrl(document);
        return document;
    }

    public async Task<Document> UploadDocumentAsync(DocumentUploadRequest request, Stream fileStream, int userId)
    {
        ValidateUpload(request.OriginalFileName, request.Category, request.Title, fileStream.Length);

        var extension = Path.GetExtension(request.OriginalFileName);
        var destinationPath = BuildDestinationPath(userId, request.ProjectId, extension);
        var storedPath = await _storage.UploadAsync(fileStream, destinationPath);

        var document = new Document
        {
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            ProjectId = request.ProjectId,
            UploaderId = userId,
            UploadDate = DateTime.UtcNow,
            OriginalFileName = request.OriginalFileName,
            StoredFilePath = storedPath,
            FileSize = fileStream.CanSeek ? fileStream.Length : 0,
            FileType = DetermineFileType(request.OriginalFileName),
            IsPreviewable = IsPreviewableType(DetermineFileType(request.OriginalFileName))
        };

        foreach (var tag in request.Tags.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            document.DocumentTags.Add(new DocumentTag { Tag = tag });
        }

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        await AddDocumentActivityAsync(document.DocumentId, userId, DocumentActivityType.Upload, "Document uploaded.");

        SetDownloadUrl(document);
        return document;
    }

    public async Task<bool> UpdateDocumentMetadataAsync(int documentId, DocumentMetadataUpdate update, int requestingUserId)
    {
        var document = await _context.Documents
            .Include(d => d.DocumentTags)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null || document.UploaderId != requestingUserId)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(update.Title) || string.IsNullOrWhiteSpace(update.Category))
        {
            return false;
        }

        document.Title = update.Title;
        document.Description = update.Description;
        document.Category = update.Category;
        document.DocumentTags.Clear();

        foreach (var tag in update.Tags.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            document.DocumentTags.Add(new DocumentTag { Tag = tag });
        }

        await _context.SaveChangesAsync();
        await AddDocumentActivityAsync(documentId, requestingUserId, DocumentActivityType.MetadataUpdate, "Document metadata updated.");

        return true;
    }

    public async Task<bool> ReplaceDocumentFileAsync(int documentId, Stream fileStream, string originalFileName, int requestingUserId)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId);
        if (document == null || document.UploaderId != requestingUserId)
        {
            return false;
        }

        ValidateUpload(originalFileName, document.Category, document.Title, fileStream.Length);

        var extension = Path.GetExtension(originalFileName);
        var destinationPath = BuildDestinationPath(requestingUserId, document.ProjectId, extension);
        var storedPath = await _storage.UploadAsync(fileStream, destinationPath);

        if (!string.IsNullOrEmpty(document.StoredFilePath))
        {
            await _storage.DeleteAsync(document.StoredFilePath);
        }

        document.StoredFilePath = storedPath;
        document.OriginalFileName = originalFileName;
        document.FileSize = fileStream.CanSeek ? fileStream.Length : 0;
        document.FileType = DetermineFileType(originalFileName);
        document.IsPreviewable = IsPreviewableType(document.FileType);
        document.UploadDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await AddDocumentActivityAsync(documentId, requestingUserId, DocumentActivityType.Replace, "Document file replaced.");

        return true;
    }

    public async Task<bool> DeleteDocumentAsync(int documentId, int requestingUserId)
    {
        var document = await _context.Documents
            .Include(d => d.Shares)
            .Include(d => d.TaskDocuments)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null || document.UploaderId != requestingUserId)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(document.StoredFilePath))
        {
            await _storage.DeleteAsync(document.StoredFilePath);
        }

        _context.TaskDocuments.RemoveRange(document.TaskDocuments);
        _context.DocumentShares.RemoveRange(document.Shares);
        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();

        await AddDocumentActivityAsync(documentId, requestingUserId, DocumentActivityType.Delete, "Document deleted.");
        return true;
    }

    public async Task<bool> ShareDocumentAsync(int documentId, DocumentShareRequest shareRequest, int requestingUserId)
    {
        var document = await _context.Documents.Include(d => d.Project).FirstOrDefaultAsync(d => d.DocumentId == documentId);
        if (document == null)
        {
            return false;
        }

        if (document.UploaderId != requestingUserId && !await IsProjectManagerOfDocumentAsync(documentId, requestingUserId))
        {
            return false;
        }

        var shared = false;

        if (shareRequest.ProjectId.HasValue)
        {
            var project = await _context.Projects.FindAsync(shareRequest.ProjectId.Value);
            if (project != null)
            {
                _context.DocumentShares.Add(new DocumentShare
                {
                    DocumentId = documentId,
                    SharedWithProjectId = shareRequest.ProjectId,
                    SharedByUserId = requestingUserId,
                    SharedDate = DateTime.UtcNow,
                    Role = shareRequest.Role
                });
                shared = true;
            }
        }

        foreach (var userId in shareRequest.UserIds.Where(id => id != requestingUserId).Distinct())
        {
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                continue;
            }

            _context.DocumentShares.Add(new DocumentShare
            {
                DocumentId = documentId,
                SharedWithUserId = userId,
                SharedByUserId = requestingUserId,
                SharedDate = DateTime.UtcNow,
                Role = shareRequest.Role
            });

            await _notificationService.CreateNotificationAsync(new Notification
            {
                UserId = userId,
                Title = "Document Shared",
                Message = $"A document was shared with you: {document.Title}",
                Type = NotificationType.Informational,
                Priority = NotificationPriority.Important
            });
            shared = true;
        }

        if (!shared)
        {
            return false;
        }

        await _context.SaveChangesAsync();
        await AddDocumentActivityAsync(documentId, requestingUserId, DocumentActivityType.Share, "Document shared with user or project.");
        return true;
    }

    public async Task<bool> AttachDocumentToTaskAsync(int documentId, int taskId, int requestingUserId)
    {
        var document = await _context.Documents
            .Include(d => d.Project)
            .Include(d => d.Shares)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        var task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Project!.ProjectMembers)
            .FirstOrDefaultAsync(t => t.TaskId == taskId);

        if (document == null || task == null)
        {
            return false;
        }

        if (!await IsAuthorizedToViewDocumentAsync(document, requestingUserId))
        {
            return false;
        }

        var isTaskParticipant = task.AssignedUserId == requestingUserId
            || task.CreatedByUserId == requestingUserId
            || task.ProjectId.HasValue && (task.Project!.ProjectManagerId == requestingUserId || task.Project.ProjectMembers.Any(pm => pm.UserId == requestingUserId));

        if (!isTaskParticipant)
        {
            return false;
        }

        var existing = await _context.TaskDocuments.AnyAsync(td => td.TaskId == taskId && td.DocumentId == documentId);
        if (existing)
        {
            return true;
        }

        _context.TaskDocuments.Add(new TaskDocument
        {
            TaskId = taskId,
            DocumentId = documentId,
            AttachedDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        await AddDocumentActivityAsync(documentId, requestingUserId, DocumentActivityType.Share, $"Document attached to task {taskId}.");
        return true;
    }

    public async Task<Stream?> DownloadDocumentAsync(int documentId, int requestingUserId)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId);
        if (document == null)
        {
            return null;
        }

        if (!await IsAuthorizedToViewDocumentAsync(document, requestingUserId))
        {
            return null;
        }

        var stream = await _storage.DownloadAsync(document.StoredFilePath);
        if (stream != null)
        {
            await AddDocumentActivityAsync(documentId, requestingUserId, DocumentActivityType.Download, "Document downloaded.");
        }

        return stream;
    }

    public async Task<DocumentSearchResult> SearchDocumentsAsync(DocumentSearchFilter filter, int requestingUserId)
    {
        var authorizedProjectIds = await _context.ProjectMembers
            .Where(pm => pm.UserId == requestingUserId)
            .Select(pm => pm.ProjectId)
            .ToListAsync();

        var query = _context.Documents
            .Include(d => d.Project)
            .Include(d => d.DocumentTags)
            .Include(d => d.Shares)
            .Where(d => d.UploaderId == requestingUserId
                || (d.ProjectId.HasValue && authorizedProjectIds.Contains(d.ProjectId.Value))
                || d.Shares.Any(s => s.SharedWithUserId == requestingUserId || (s.SharedWithProjectId.HasValue && authorizedProjectIds.Contains(s.SharedWithProjectId.Value))));

        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var queryText = filter.Query.Trim().ToLower();
            query = query.Where(d => d.Title.ToLower().Contains(queryText)
                || (d.Description ?? string.Empty).ToLower().Contains(queryText)
                || d.OriginalFileName.ToLower().Contains(queryText)
                || d.DocumentTags.Any(t => t.Tag.ToLower().Contains(queryText)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            var categoryText = filter.Category.Trim().ToLower();
            query = query.Where(d => d.Category.ToLower().Contains(categoryText));
        }

        if (filter.ProjectId.HasValue)
        {
            query = query.Where(d => d.ProjectId == filter.ProjectId);
        }

        if (filter.CreatedAfter.HasValue)
        {
            query = query.Where(d => d.UploadDate >= filter.CreatedAfter.Value);
        }

        if (filter.CreatedBefore.HasValue)
        {
            query = query.Where(d => d.UploadDate <= filter.CreatedBefore.Value);
        }

        var documents = await query.OrderByDescending(d => d.UploadDate).ToListAsync();
        documents.ForEach(SetDownloadUrl);

        return new DocumentSearchResult
        {
            Documents = documents,
            TotalCount = documents.Count
        };
    }

    private static void ValidateUpload(string originalFileName, string category, string title, long length)
    {
        if (string.IsNullOrWhiteSpace(originalFileName) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(category))
        {
            throw new InvalidOperationException("File upload requires title, category, and filename.");
        }

        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension) || !_supportedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Unsupported file type.");
        }

        if (length <= 0 || length > MaxUploadSize)
        {
            throw new InvalidOperationException("File size exceeds the maximum allowed upload size.");
        }
    }

    private static string DetermineFileType(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };
    }

    private static bool IsPreviewableType(string fileType)
    {
        return _previewableTypes.ContainsKey(fileType);
    }

    private static string BuildDestinationPath(int userId, int? projectId, string extension)
    {
        var folder = projectId.HasValue ? Path.Combine("project", projectId.Value.ToString()) : Path.Combine("personal", userId.ToString());
        var fileName = $"{Guid.NewGuid():N}{extension}";
        return Path.Combine(userId.ToString(), folder, fileName).Replace('\\', '/');
    }

    private async Task<bool> IsAuthorizedToViewDocumentAsync(Document document, int userId)
    {
        if (document.UploaderId == userId)
        {
            return true;
        }

        if (document.Shares.Any(s => s.SharedWithUserId == userId))
        {
            return true;
        }

        if (document.ProjectId.HasValue)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.ProjectId == document.ProjectId.Value);

            if (project != null && (project.ProjectManagerId == userId || project.ProjectMembers.Any(pm => pm.UserId == userId)))
            {
                return true;
            }

            if (document.Shares.Any(s => s.SharedWithProjectId == document.ProjectId.Value))
            {
                return true;
            }
        }

        if (document.Shares.Any(s => s.SharedWithProjectId.HasValue))
        {
            var sharedProjectIds = document.Shares
                .Where(s => s.SharedWithProjectId.HasValue)
                .Select(s => s.SharedWithProjectId!.Value)
                .Distinct()
                .ToList();

            var userProjectMembership = await _context.ProjectMembers
                .AnyAsync(pm => sharedProjectIds.Contains(pm.ProjectId) && pm.UserId == userId);

            if (userProjectMembership)
            {
                return true;
            }
        }

        return false;
    }

    private async Task<bool> IsProjectManagerOfDocumentAsync(int documentId, int userId)
    {
        var document = await _context.Documents.Include(d => d.Project).FirstOrDefaultAsync(d => d.DocumentId == documentId);
        return document?.Project != null && document.Project.ProjectManagerId == userId;
    }

    private async Task AddDocumentActivityAsync(int documentId, int userId, DocumentActivityType activityType, string details)
    {
        if (await _context.Documents.AnyAsync(d => d.DocumentId == documentId))
        {
            _context.DocumentActivities.Add(new DocumentActivity
            {
                DocumentId = documentId,
                UserId = userId,
                ActivityType = activityType,
                ActivityDate = DateTime.UtcNow,
                Details = details
            });

            await _context.SaveChangesAsync();
        }
    }

    private static void SetDownloadUrl(Document document)
    {
        document.DownloadUrl = $"/documents/download/{document.DocumentId}";
    }
}
