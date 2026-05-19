using MegaProject.Domain.Models;
using TaskStatus = MegaProject.Domain.Models.Enums.TaskStatus;

namespace MegaProject.Web.Services;

public interface IProjectTaskService
{
    Task<List<ProjectTask>> GetByProjectAsync(Guid projectId, TaskStatus? status = null);
    Task<ProjectTask?> GetByIdAsync(Guid id);
    Task CreateAsync(ProjectTask task);
    Task UpdateAsync(ProjectTask task);
    Task DeleteAsync(Guid id);
}