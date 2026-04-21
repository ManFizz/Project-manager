using System.ComponentModel.DataAnnotations;
using MegaProject.Models;

namespace MegaProject.ViewModels;

/// <summary>
/// ViewModel for 5-step wizard
/// </summary>
public class ProjectCreateViewModel
{
    // Шаг 1
    [Required(ErrorMessage = "Project name is required")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime Start { get; set; } = DateTime.Today;

    [Required]
    public DateTime End { get; set; } = DateTime.Today.AddDays(30);

    [Range(1, 15)]
    public int Priority { get; set; } = 5;

    [Required]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    public string ExecutorName { get; set; } = string.Empty;

    [Required]
    public Guid ManagerId { get; set; }

    public List<Guid>? SelectedEmployeeIds { get; set; } = new();

    public List<string> DocumentPaths { get; set; } = new();

    public List<Employee> Employees { get; set; } = new();
    
    public List<IFormFile> Files { get; set; } = new();
}