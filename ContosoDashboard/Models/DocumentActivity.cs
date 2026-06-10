using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class DocumentActivity
{
    [Key]
    public int DocumentActivityId { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [ForeignKey("DocumentId")]
    public virtual Document Document { get; set; } = null!;

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [Required]
    public DocumentActivityType ActivityType { get; set; }

    [Required]
    public DateTime ActivityDate { get; set; } = DateTime.UtcNow;

    [MaxLength(1000)]
    public string? Details { get; set; }
}

public enum DocumentActivityType
{
    Upload,
    Download,
    Delete,
    Share,
    Replace,
    MetadataUpdate
}
