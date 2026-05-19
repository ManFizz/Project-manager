namespace MegaProject.Domain.Models;

/// <summary>
/// Employee (used both as a regular contractor and as a project manager)
/// </summary>
public class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }

    public string Mail { get; set; } = string.Empty;

    public List<Project> Projects { get; set; } = [];           // projects in which the employee participates
    public List<Project> ManagedProjects { get; set; } = [];    // projects where the employee is the supervisor
    public List<ProjectTask> WorkerTasks { get; set; } = [];
    public List<ProjectTask> AuthorTasks { get; set; } = [];
}