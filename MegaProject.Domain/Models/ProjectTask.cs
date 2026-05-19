using TaskStatus = MegaProject.Domain.Models.Enums.TaskStatus;

namespace MegaProject.Domain.Models;

public class ProjectTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.ToDo;
    public int Priority { get; set; } = 0;
    
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public Guid AuthorId { get; set; }
    public Employee Author { get; set; } = null!;
    
    public Guid? WorkerId { get; set; }
    public Employee? Worker { get; set; }
}