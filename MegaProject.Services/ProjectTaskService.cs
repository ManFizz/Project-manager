using MegaProject.Data;
using MegaProject.Domain.Models;
using MegaProject.Domain.Models.Exceptions;
using Microsoft.EntityFrameworkCore;
using TaskStatus = MegaProject.Domain.Models.Enums.TaskStatus;

namespace MegaProject.Services;

public class ProjectTaskService(ApplicationDbContext context) : IProjectTaskService
{
    public Task<List<ProjectTask>> GetByProjectAsync(Guid projectId, TaskStatus? status = null)
    {
        var query = context.ProjectsTasks
            .Include(t => t.Author)
            .Include(t => t.Worker)
            .Where(t => t.ProjectId == projectId);

        if (status != null)
            query = query.Where(t => t.Status == status);

        return query.OrderBy(t => t.Priority).ToListAsync();
    }

    public Task<ProjectTask?> GetByIdAsync(Guid id) =>
        context.ProjectsTasks
            .Include(t => t.Author)
            .Include(t => t.Worker)
            .FirstOrDefaultAsync(t => t.Id == id);

    public Task CreateAsync(ProjectTask task)
    {
        context.ProjectsTasks.Add(task);
        return context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ProjectTask task)
    {
        var found = await context.ProjectsTasks.FindAsync(task.Id);
        if (found == null)
            throw new NotFoundException("Task not found");

        found.Name = task.Name;
        found.Comment = task.Comment;
        found.Status = task.Status;
        found.Priority = task.Priority;
        found.ProjectId = task.ProjectId;
        found.AuthorId = task.AuthorId;
        found.WorkerId = task.WorkerId;

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var task = await context.ProjectsTasks.FindAsync(id);
        if (task == null)
            throw new NotFoundException("Task not found");

        context.ProjectsTasks.Remove(task);
        await context.SaveChangesAsync();
    }
}
