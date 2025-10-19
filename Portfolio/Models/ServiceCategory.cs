using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Models;

public partial class ServiceCategory
{
    [Key]
    public int ServiceCategoryId { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string CategoryDesc { get; set; } = null!;

    // Navigation property - collection of Services
    [InverseProperty(nameof(Service.ServiceCategory))]
    public virtual ICollection<Service> Services { get; init; } = new List<Service>();
}
