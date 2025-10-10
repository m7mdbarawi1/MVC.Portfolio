using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Portfolio.Models;

public partial class Service
{
    [Key]
    public int ServiceId { get; set; }

    public int UserId { get; set; }

    public int ServiceCategoryId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string ServiceTitle { get; set; } = null!;

    [StringLength(500)]
    [Unicode(false)]
    public string Description { get; set; } = null!;

    [StringLength(250)]
    [Unicode(false)]
    public string? CoverImageUrl { get; set; }

    [ForeignKey("ServiceCategoryId")]
    [InverseProperty("Services")]
    public virtual ServiceCategory ServiceCategory { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Services")]
    public virtual User User { get; set; } = null!;
}
