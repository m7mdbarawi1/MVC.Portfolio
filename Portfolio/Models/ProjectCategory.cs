using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Models;

public partial class ProjectCategory
{
    [Key]
    public int ProjectCategoryId { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string CategoryDesc { get; set; } = null!;

    // Navigation property - collection of Projects
    [InverseProperty(nameof(Project.ProjectCategory))]
    public virtual ICollection<Project> Projects { get; init; } = new List<Project>();
}
