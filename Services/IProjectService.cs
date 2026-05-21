using MegaProject.Domain.Models;
using MegaProject.Web.ViewModels;

namespace MegaProject.Web.Services;

/// <summary>
/// Business Logic Layer - an interface for working with projects
/// </summary>
public interface IProjectService
{
    IQueryable<Project> GetProjectsQuery();
    Task<Project?> GetProjectByIdAsync(Guid id);
    Task CreateProjectAsync(ProjectCreateViewModel project);
    Task UpdateProjectAsync(Project updated, List<IFormFile> files, Guid? id = null);
    Task DeleteProjectAsync(Guid id);
    Task<List<EmployeeDto>> SearchEmployeesAsync(string term);
    Task RemoveEmployeeAsync(Guid projectId, Guid employeeId);
    Task AddEmployeesAsync(Guid projectId, List<Guid> employeeIds);
    Task DeleteFileAsync(Guid projectId, string filePath);
    Task SetManagerAsync(Guid projectId, Guid employeeId);
    Task<ProjectListViewModel> GetProjectsAsync(ProjectListViewModel model);
    Task UpdateProjectAsync(Guid id, ProjectEditViewModel model);
}