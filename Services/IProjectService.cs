using MegaProject.Models;

namespace MegaProject.Services;

/// <summary>
/// Business Logic Layer - an interface for working with projects
/// </summary>
public interface IProjectService
{
    IQueryable<Project> GetProjectsQuery();

    Task<Project?> GetProjectByIdAsync(Guid id);
    Task CreateProjectAsync(Project project, List<Guid> employeeIds, List<IFormFile> files);
    Task UpdateProjectAsync(Project project);
    Task DeleteProjectAsync(Guid id);
    Task<IEnumerable<Employee>> SearchEmployeesAsync(string term);
    Task AddEmployeesToProjectAsync(Guid projectId, List<Guid> employeeIds);
}