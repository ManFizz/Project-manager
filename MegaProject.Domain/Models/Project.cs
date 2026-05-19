namespace MegaProject.Domain.Models;

/// <summary>
/// Project (main entity)
/// </summary>
public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ExecutorName { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int Priority { get; set; }
    
    public Guid ManagerId { get; set; } 
    public Employee? Manager { get; set; }
    
    public List<Employee> Employees { get; set; } = [];          // Many-to-Many: employees on the project
    public List<string> DocumentPaths { get; set; } = [];        // Project documents (paths to uploaded files)

    public List<ProjectTask> ProjectTasks { get; set; } = [];
}