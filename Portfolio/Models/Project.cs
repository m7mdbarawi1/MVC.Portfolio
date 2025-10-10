using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Portfolio.Models;

public partial class Project
{
    [Key]
    public int ProjectId { get; set; }

    public int UserId { get; set; }

    public int ProjectCategoryId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string ProjectTitle { get; set; } = null!;

    [StringLength(500)]
    [Unicode(false)]
    public string Description { get; set; } = null!;

    [StringLength(250)]
    [Unicode(false)]
    public string? CoverImageUrl { get; set; }

    [ForeignKey("ProjectCategoryId")]
    [InverseProperty("Projects")]
    public virtual ProjectCategory ProjectCategory { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Projects")]
    public virtual User User { get; set; } = null!;
}
