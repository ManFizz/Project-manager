using System.ComponentModel.DataAnnotations;

namespace MegaProject.ViewModels;

public class CreateEmployeeViewModel
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    [Required]
    [EmailAddress]
    public string Mail { get; set; } = string.Empty;
}