using System.ComponentModel.DataAnnotations;
using MegaProject.Models.Enums;

namespace MegaProject.Models;

/// <summary>
/// Project (main entity)
/// </summary>
public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Project name is required")]
    [Display(Name = "Project Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Customer company is required")]
    [Display(Name = "Customer Company")]
    public string ClientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Executor company is required")]
    [Display(Name = "Executor Company")]
    public string ExecutorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Manager is required")]
    public Guid ManagerId { get; set; } 

    public Employee? Manager { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    [Display(Name = "Start Date")]
    public DateTime Start { get; set; }

    [Required(ErrorMessage = "End date is required")]
    [Display(Name = "End Date")]
    public DateTime End { get; set; }

    [Range(1, 15, ErrorMessage = "Priority must be between 1 and 15")]
    public int Priority { get; set; }
    
    public List<Employee> Employees { get; set; } = new();          // Many-to-Many: employees on the project
    
    public List<string> DocumentPaths { get; set; } = new();        // Project documents (paths to uploaded files)
}