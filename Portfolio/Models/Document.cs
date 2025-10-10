using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Portfolio.Models;

public partial class Document
{
    [Key]
    public int DocumentId { get; set; }

    public int UserId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string DocumentTitle { get; set; } = null!;

    [StringLength(250)]
    [Unicode(false)]
    public string? CoverImageUrl { get; set; }

    [StringLength(250)]
    [Unicode(false)]
    public string? DocumentUrl { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Documents")]
    public virtual User User { get; set; } = null!;
}
