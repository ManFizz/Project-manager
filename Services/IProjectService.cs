using MegaProject.Models;

namespace MegaProject.Services;

/// <summary>
/// Business Logic Layer - an interface for working with projects
/// </summary>
public interface IProjectService
{
    Task<IEnumerable<Project>> GetAllProjectsAsync();

    Task<Project?> GetProjectByIdAsync(Guid id);
    Task CreateProjectAsync(Project project);
    Task UpdateProjectAsync(Project project);
    Task DeleteProjectAsync(Guid id);

    // Дополнительно для визарда
    Task<IEnumerable<Employee>> SearchEmployeesAsync(string term);
    Task AddEmployeesToProjectAsync(Guid projectId, List<Guid> employeeIds);
    Task SetManagerAsync(Guid projectId, Guid managerId);
    
    Task RemoveEmployeeFromProjectAsync(Guid projectId, Guid employeeId);
}