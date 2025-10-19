using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace Portfolio.Models;

public partial class Service
{
    [Key]
    public int ServiceId { get; set; }

    // FK to User - non-nullable, cascade delete
    public int UserId { get; set; }

    // FK to ServiceCategory - nullable to allow 'SetNull' on delete
    public int? ServiceCategoryId { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string ServiceTitle { get; set; } = null!;

    [Required]
    [StringLength(500)]
    [Unicode(false)]
    public string Description { get; set; } = null!;

    [StringLength(250)]
    [Unicode(false)]
    public string? CoverImageUrl { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ServiceCategoryId))]
    [InverseProperty(nameof(ServiceCategory.Services))]
    [ValidateNever]
    public virtual ServiceCategory? ServiceCategory { get; set; }

    [ForeignKey(nameof(UserId))]
    [InverseProperty(nameof(User.Services))]
    [ValidateNever]
    public virtual User User { get; set; } = null!;
}
