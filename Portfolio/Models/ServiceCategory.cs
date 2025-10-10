using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Portfolio.Models;

public partial class ServiceCategory
{
    [Key]
    public int ServiceCategoryId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string CategoryDesc { get; set; } = null!;

    [InverseProperty("ServiceCategory")]
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
