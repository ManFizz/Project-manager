using MegaProject.Data;
using MegaProject.Domain.Models;
using MegaProject.Domain.Models.Exceptions;
using Microsoft.EntityFrameworkCore;
using TaskStatus = MegaProject.Domain.Models.Enums.TaskStatus;

namespace MegaProject.Web.Services;

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

    public Task<ProjectTask?> GetByIdAsync(Guid id)
    {
        return context.ProjectsTasks
            .Include(t => t.Author)
            .Include(t => t.Worker)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public Task CreateAsync(ProjectTask task)
    {
        context.ProjectsTasks.Add(task);

        return context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ProjectTask task)
    {
        var foundTask = await GetByIdAsync(task.Id);
        if (foundTask == null)
            throw new NotFoundException("Task not found");

        foundTask.Name = task.Name;
        foundTask.Comment = task.Comment;
        foundTask.Status = task.Status;
        foundTask.Priority = task.Priority;
        
        foundTask.ProjectId = task.ProjectId;
        foundTask.AuthorId = task.AuthorId;
        foundTask.WorkerId = task.WorkerId;
        
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var foundTask = await GetByIdAsync(id);
        if (foundTask == null)
            throw new NotFoundException("Task not found");

        context.ProjectsTasks.Remove(foundTask);
        await context.SaveChangesAsync();
    }
}