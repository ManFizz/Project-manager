using System.ComponentModel.DataAnnotations;

namespace MegaProject.ViewModels;

public class ProjectEditViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Project name is required")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Customer company is required")]
    public string ClientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Executor company is required")]
    public string ExecutorName { get; set; } = string.Empty;

    [Required]
    public DateTime Start { get; set; }

    [Required]
    public DateTime End { get; set; }

    [Range(1, 15)]
    public int Priority { get; set; }

    public List<string> DocumentPaths { get; set; } = new();

    public List<IFormFile> Files { get; set; } = new();
}