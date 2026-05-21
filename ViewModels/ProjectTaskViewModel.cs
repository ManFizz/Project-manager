using System.ComponentModel.DataAnnotations;
using MegaProject.Domain.Models;
using TaskStatus = MegaProject.Domain.Models.Enums.TaskStatus;

namespace MegaProject.Web.ViewModels;

public class ProjectTaskViewModel
{
    public Guid? Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Comment { get; set; }
    
    public int Priority { get; set; } = 0;
    
    public TaskStatus Status { get; set; } = TaskStatus.ToDo;
    
    [Required]
    public Guid ProjectId { get; set; }
    
    [Required]
    public Guid AuthorId { get; set; }
    
    public Guid? WorkerId { get; set; }
    
    public List<Employee> Employees { get; set; } = [];
}