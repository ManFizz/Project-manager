using MegaProject.Models;

namespace MegaProject.ViewModels;

public class ProjectListViewModel
{
    public List<Project> Projects { get; set; } = new();

    // Filters
    public string? Search { get; set; }
    public DateTime? StartFrom { get; set; }
    public DateTime? StartTo { get; set; }
    public int? MinPriority { get; set; }
    public int? MaxPriority { get; set; }

    // Sorting
    public string SortColumn { get; set; } = "Name";
    public string SortDirection { get; set; } = "asc";
}