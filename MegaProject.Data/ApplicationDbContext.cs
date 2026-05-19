using MegaProject.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MegaProject.Data;

/// <summary>
/// Data Access Layer - DbContext for Entity Framework Core Code First
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    
    public DbSet<ProjectTask> ProjectsTasks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Many-to-Many: Project <-> Employee (employees on the project)
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Employees)
            .WithMany(e => e.Projects)
            .UsingEntity<Dictionary<string, object>>(
                "ProjectEmployee",
                j => j
                    .HasOne<Employee>()
                    .WithMany()
                    .HasForeignKey("EmployeeId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Project>()
                    .WithMany()
                    .HasForeignKey("ProjectId")
                    .OnDelete(DeleteBehavior.Cascade));

        // One-to-Many: Project -> Manager
        modelBuilder.Entity<Project>()
            .HasOne(p => p.Manager)
            .WithMany(e => e.ManagedProjects)
            .HasForeignKey(p => p.ManagerId)
            .OnDelete(DeleteBehavior.Restrict); // don't delete an employee if he is a project manager

        modelBuilder.Entity<ProjectTask>()
            .HasOne(pt => pt.Worker)
            .WithMany(e => e.WorkerTasks)
            .HasForeignKey(pt => pt.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<ProjectTask>()
            .HasOne(pt => pt.Author)
            .WithMany(e => e.AuthorTasks)
            .HasForeignKey(pt => pt.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<ProjectTask>()
            .HasOne(pt => pt.Project)
            .WithMany(e => e.ProjectTasks)
            .HasForeignKey(pt => pt.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}