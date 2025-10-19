using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace Portfolio.Models;

public partial class News
{
    [Key]
    public int NewsId { get; set; }

    // FK to User - non-nullable, cascade delete
    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string NewsTitle { get; set; } = null!;

    [Required]
    [StringLength(500)]
    [Unicode(false)]
    public string NewsDescription { get; set; } = null!;

    public DateOnly NewsDate { get; set; }

    [StringLength(250)]
    [Unicode(false)]
    public string? CoverImageUrl { get; set; }

    // Navigation property
    [ForeignKey(nameof(UserId))]
    [InverseProperty(nameof(User.News))]
    [ValidateNever]
    public virtual User User { get; set; } = null!;
}
