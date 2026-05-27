using MegaProject.Data;
using MegaProject.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MegaProject.Services;

public class ProjectService(ApplicationDbContext context) : IProjectService
{
    public Task<Project?> GetProjectByIdAsync(Guid id) =>
        context.Projects
            .Include(p => p.Manager)
            .Include(p => p.Employees)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<Project>> GetProjectsAsync(ProjectFilter filter)
    {
        var query = context.Projects
            .Include(p => p.Manager)
            .Include(p => p.Employees)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p =>
                p.Name.Contains(filter.Search) ||
                p.ClientName.Contains(filter.Search) ||
                p.ExecutorName.Contains(filter.Search));

        if (filter.StartFrom.HasValue)
            query = query.Where(p => p.Start >= filter.StartFrom.Value);

        if (filter.StartTo.HasValue)
            query = query.Where(p => p.Start <= filter.StartTo.Value);

        if (filter.MinPriority.HasValue)
            query = query.Where(p => p.Priority >= filter.MinPriority.Value);

        if (filter.MaxPriority.HasValue)
            query = query.Where(p => p.Priority <= filter.MaxPriority.Value);

        query = filter.SortColumn switch
        {
            "Client"   => filter.SortDirection == "asc" ? query.OrderBy(x => x.ClientName)   : query.OrderByDescending(x => x.ClientName),
            "Executor" => filter.SortDirection == "asc" ? query.OrderBy(x => x.ExecutorName) : query.OrderByDescending(x => x.ExecutorName),
            "Start"    => filter.SortDirection == "asc" ? query.OrderBy(x => x.Start)        : query.OrderByDescending(x => x.Start),
            "End"      => filter.SortDirection == "asc" ? query.OrderBy(x => x.End)          : query.OrderByDescending(x => x.End),
            "Priority" => filter.SortDirection == "asc" ? query.OrderBy(x => x.Priority)     : query.OrderByDescending(x => x.Priority),
            _          => filter.SortDirection == "asc" ? query.OrderBy(x => x.Name)         : query.OrderByDescending(x => x.Name),
        };

        return await query.ToListAsync();
    }

    public async Task CreateProjectAsync(Project project, List<Guid>? employeeIds = null)
    {
        if (project.End < project.Start)
            throw new Exception("End date must be after start date");

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var ids = (employeeIds ?? new List<Guid>())
            .Append(project.ManagerId)
            .Distinct()
            .ToList();

        var employees = await context.Employees
            .Where(e => ids.Contains(e.Id))
            .ToListAsync();

        project.Employees = employees;
        await context.SaveChangesAsync();
    }

    public async Task UpdateProjectAsync(Guid id, Project updated)
    {
        var project = await context.Projects.FirstOrDefaultAsync(p => p.Id == id);
        if (project == null)
            throw new Exception("Project not found");

        if (updated.End < updated.Start)
            throw new Exception("End date must be after start date");

        project.Name = updated.Name;
        project.ClientName = updated.ClientName;
        project.ExecutorName = updated.ExecutorName;
        project.Start = updated.Start;
        project.End = updated.End;
        project.Priority = updated.Priority;
        project.DocumentPaths = updated.DocumentPaths;

        await context.SaveChangesAsync();
    }

    public async Task<List<string>> DeleteProjectAsync(Guid id)
    {
        var project = await context.Projects.FindAsync(id);
        if (project == null) return [];

        var paths = project.DocumentPaths.ToList();
        context.Projects.Remove(project);
        await context.SaveChangesAsync();
        return paths;
    }

    public async Task<List<EmployeeDto>> SearchEmployeesAsync(string term)
    {
        var query = context.Employees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(e =>
                e.FirstName.Contains(term) ||
                e.LastName.Contains(term) ||
                e.Mail.Contains(term));

        return await query
            .Select(e => new EmployeeDto { Id = e.Id, FirstName = e.FirstName, LastName = e.LastName, Mail = e.Mail })
            .ToListAsync();
    }

    public async Task AddEmployeesAsync(Guid projectId, List<Guid> employeeIds)
    {
        employeeIds = employeeIds.Distinct().ToList();
        if (employeeIds.Count == 0) return;

        var project = await context.Projects.Include(p => p.Employees).FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null) return;

        var employees = await context.Employees.Where(e => employeeIds.Contains(e.Id)).ToListAsync();
        foreach (var emp in employees)
            if (!project.Employees.Contains(emp))
                project.Employees.Add(emp);

        await context.SaveChangesAsync();
    }

    public async Task RemoveEmployeeAsync(Guid projectId, Guid employeeId)
    {
        var project = await context.Projects.Include(p => p.Employees).FirstOrDefaultAsync(p => p.Id == projectId);
        var employee = project?.Employees.FirstOrDefault(e => e.Id == employeeId);
        if (employee == null) return;

        project!.Employees.Remove(employee);
        await context.SaveChangesAsync();
    }

    public async Task RemoveDocumentPathAsync(Guid projectId, string filePath)
    {
        var project = await context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null || !project.DocumentPaths.Contains(filePath)) return;

        project.DocumentPaths.Remove(filePath);
        await context.SaveChangesAsync();
    }

    public async Task SetManagerAsync(Guid projectId, Guid employeeId)
    {
        var project = await context.Projects.Include(p => p.Employees).FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null) throw new Exception("Project not found");

        if (project.Employees.All(e => e.Id != employeeId))
        {
            var employee = await context.Employees.FindAsync(employeeId);
            if (employee == null) throw new Exception("Employee not found");
            project.Employees.Add(employee);
        }

        project.ManagerId = employeeId;
        await context.SaveChangesAsync();
    }
}
