using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class DocumentShare
{
    [Key]
    public int DocumentShareId { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [ForeignKey("DocumentId")]
    public virtual Document Document { get; set; } = null!;

    public int? SharedWithUserId { get; set; }

    [ForeignKey("SharedWithUserId")]
    public virtual User? SharedWithUser { get; set; }

    public int? SharedWithProjectId { get; set; }

    [ForeignKey("SharedWithProjectId")]
    public virtual Project? SharedWithProject { get; set; }

    [Required]
    public int SharedByUserId { get; set; }

    [ForeignKey("SharedByUserId")]
    public virtual User SharedByUser { get; set; } = null!;

    [Required]
    public DateTime SharedDate { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? Role { get; set; }
}
