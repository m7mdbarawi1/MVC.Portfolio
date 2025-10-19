using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace Portfolio.Models;

public partial class Project
{
    [Key]
    public int ProjectId { get; set; }

    // FK to User - non-nullable, cascade delete
    public int UserId { get; set; }

    // FK to ProjectCategory - nullable to support 'SetNull' on delete
    public int? ProjectCategoryId { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string ProjectTitle { get; set; } = null!;

    [Required]
    [StringLength(500)]
    [Unicode(false)]
    public string Description { get; set; } = null!;

    [StringLength(250)]
    [Unicode(false)]
    public string? CoverImageUrl { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ProjectCategoryId))]
    [InverseProperty(nameof(ProjectCategory.Projects))]
    [ValidateNever]
    public virtual ProjectCategory? ProjectCategory { get; set; }

    [ForeignKey(nameof(UserId))]
    [InverseProperty(nameof(User.Projects))]
    [ValidateNever]
    public virtual User User { get; set; } = null!;
}
