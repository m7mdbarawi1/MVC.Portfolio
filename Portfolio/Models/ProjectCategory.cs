using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Portfolio.Models;

public partial class ProjectCategory
{
    [Key]
    public int ProjectCategoryId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string CategoryDesc { get; set; } = null!;

    [InverseProperty("ProjectCategory")]
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
