using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class Document
{
    [Key]
    public int DocumentId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    public int? ProjectId { get; set; }

    [ForeignKey("ProjectId")]
    public virtual Project? Project { get; set; }

    [Required]
    public int UploaderId { get; set; }

    [ForeignKey("UploaderId")]
    public virtual User Uploader { get; set; } = null!;

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string StoredFilePath { get; set; } = string.Empty;

    [Required]
    public long FileSize { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileType { get; set; } = string.Empty;

    public bool IsPreviewable { get; set; }

    public virtual ICollection<DocumentTag> DocumentTags { get; set; } = new List<DocumentTag>();
    public virtual ICollection<DocumentShare> Shares { get; set; } = new List<DocumentShare>();
    public virtual ICollection<DocumentActivity> Activities { get; set; } = new List<DocumentActivity>();
    public virtual ICollection<TaskDocument> TaskDocuments { get; set; } = new List<TaskDocument>();

    [NotMapped]
    public string? DownloadUrl { get; set; }
}
