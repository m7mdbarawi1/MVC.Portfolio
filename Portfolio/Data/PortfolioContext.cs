using System;
using Microsoft.EntityFrameworkCore;
using Portfolio.Models;

namespace Portfolio.Data;

public partial class PortfolioContext : DbContext
{
    public PortfolioContext() { }

    public PortfolioContext(DbContextOptions<PortfolioContext> options)
        : base(options) { }

    public virtual DbSet<Document> Documents { get; set; }
    public virtual DbSet<Project> Projects { get; set; }
    public virtual DbSet<ProjectCategory> ProjectCategories { get; set; }
    public virtual DbSet<Service> Services { get; set; }
    public virtual DbSet<ServiceCategory> ServiceCategories { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<News> News { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Document
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK_Documents");
            entity.HasOne(d => d.User)
                  .WithMany(p => p.Documents)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade) // delete cascade
                  .HasConstraintName("FK_Documents_Users");
        });

        // Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK_Projects");

            entity.HasOne(d => d.ProjectCategory)
                  .WithMany(p => p.Projects)
                  .HasForeignKey(d => d.ProjectCategoryId)
                  .OnDelete(DeleteBehavior.SetNull) // delete set null
                  .HasConstraintName("FK_Projects_ProjectCategories");

            entity.HasOne(d => d.User)
                  .WithMany(p => p.Projects)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade) // delete cascade
                  .HasConstraintName("FK_Projects_Users");
        });

        // ProjectCategory
        modelBuilder.Entity<ProjectCategory>(entity =>
        {
            entity.HasKey(e => e.ProjectCategoryId).HasName("PK_ProjectCategories");
        });

        // Service
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK_Services");

            entity.HasOne(d => d.ServiceCategory)
                  .WithMany(p => p.Services)
                  .HasForeignKey(d => d.ServiceCategoryId)
                  .OnDelete(DeleteBehavior.SetNull) // delete set null
                  .HasConstraintName("FK_Services_ServiceCategories");

            entity.HasOne(d => d.User)
                  .WithMany(p => p.Services)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade) // delete cascade
                  .HasConstraintName("FK_Services_Users");
        });

        // ServiceCategory
        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasKey(e => e.ServiceCategoryId).HasName("PK_ServiceCategories");
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_Users");
        });

        // News
        modelBuilder.Entity<News>(entity =>
        {
            entity.HasKey(e => e.NewsId).HasName("PK_News");

            entity.HasOne(d => d.User)
                  .WithMany(p => p.News)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade) // delete cascade
                  .HasConstraintName("FK_News_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
