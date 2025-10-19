using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Models;

public partial class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [StringLength(20)]
    [Unicode(false)]
    public string UserName { get; set; } = null!;

    [Required]
    [StringLength(20)]
    [Unicode(false)]
    public string Password { get; set; } = null!;

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string FirstName { get; set; } = null!;

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string LastName { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? ContactNumber { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(500)]
    [Unicode(false)]
    public string Description { get; set; } = null!;

    [StringLength(250)]
    [Unicode(false)]
    public string? CoverImageUrl { get; set; }

    // Navigation properties - cascade delete
    public virtual ICollection<Document> Documents { get; init; } = new List<Document>();
    public virtual ICollection<Project> Projects { get; init; } = new List<Project>();
    public virtual ICollection<Service> Services { get; init; } = new List<Service>();
    public virtual ICollection<News> News { get; init; } = new List<News>();
}
