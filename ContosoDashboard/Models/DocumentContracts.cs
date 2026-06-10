namespace ContosoDashboard.Models;

public class DocumentUploadRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public List<string> Tags { get; set; } = new();
    public string OriginalFileName { get; set; } = string.Empty;
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
