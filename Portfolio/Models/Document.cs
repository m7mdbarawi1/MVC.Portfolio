using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace Portfolio.Models;

public partial class Document
{
    [Key]
    public int DocumentId { get; set; }

    // FK to User - non-nullable, cascade delete
    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string DocumentTitle { get; set; } = null!;

    [StringLength(250)]
    [Unicode(false)]
    public string? CoverImageUrl { get; set; }

    [StringLength(250)]
    [Unicode(false)]
    public string? DocumentUrl { get; set; }

    // Navigation property
    [ForeignKey(nameof(UserId))]
    [InverseProperty(nameof(User.Documents))]
    [ValidateNever]
    public virtual User User { get; set; } = null!;
}
