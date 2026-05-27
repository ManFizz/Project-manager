using MegaProject.Domain.Models;

namespace MegaProject.Services;

public interface IProjectService
{
    Task<Project?> GetProjectByIdAsync(Guid id);
    Task<List<Project>> GetProjectsAsync(ProjectFilter filter);
    Task CreateProjectAsync(Project project, List<Guid>? employeeIds = null);
    Task UpdateProjectAsync(Guid id, Project updated);
    Task<List<string>> DeleteProjectAsync(Guid id);  // возвращает пути файлов для удаления контроллером
    Task<List<EmployeeDto>> SearchEmployeesAsync(string term);
    Task RemoveEmployeeAsync(Guid projectId, Guid employeeId);
    Task AddEmployeesAsync(Guid projectId, List<Guid> employeeIds);
    Task RemoveDocumentPathAsync(Guid projectId, string filePath);
    Task SetManagerAsync(Guid projectId, Guid employeeId);
}
