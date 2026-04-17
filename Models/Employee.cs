using System.ComponentModel.DataAnnotations;

namespace MegaProject.Models;

/// <summary>
/// Employee (used both as a regular contractor and as a project manager)
/// </summary>
public class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "First name is required")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Middle Name")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Mail { get; set; } = string.Empty;

    public List<Project> Projects { get; set; } = new();           // projects in which the employee participates
    public List<Project> ManagedProjects { get; set; } = new();    // projects where the employee is the supervisor
}