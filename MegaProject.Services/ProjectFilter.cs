namespace MegaProject.Services;

public class ProjectFilter
{
    public string? Search { get; set; }
    public DateTime? StartFrom { get; set; }
    public DateTime? StartTo { get; set; }
    public int? MinPriority { get; set; }
    public int? MaxPriority { get; set; }
    public string SortColumn { get; set; } = "Name";
    public string SortDirection { get; set; } = "asc";
}
