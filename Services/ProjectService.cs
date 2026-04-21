using MegaProject.Data;
using MegaProject.Models;
using Microsoft.EntityFrameworkCore;

namespace MegaProject.Services;

/// <summary>
/// Business Logic Layer - project service implementation
/// </summary>
public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;

    public ProjectService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
{
    var query = _context.Projects
        .Include(p => p.Manager)
        .Include(p => p.Employees)
        .AsQueryable();

    return await query.ToListAsync();
}

    public async Task<Project?> GetProjectByIdAsync(Guid id)
    {
        return await _context.Projects
            .Include(p => p.Manager)
            .Include(p => p.Employees)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task CreateProjectAsync(Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProjectAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProjectAsync(Guid id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Employee>> SearchEmployeesAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return await _context.Employees.ToListAsync();

        return await _context.Employees
            .Where(e => e.FirstName.Contains(term) ||
                        e.LastName.Contains(term) ||
                        e.Mail.Contains(term))
            .ToListAsync();
    }

    public async Task AddEmployeesToProjectAsync(Guid projectId, List<Guid> employeeIds)
    {
        var project = await _context.Projects
            .Include(p => p.Employees)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null) return;

        var employees = await _context.Employees
            .Where(e => employeeIds.Contains(e.Id))
            .ToListAsync();

        foreach (var emp in employees)
        {
            if (!project.Employees.Contains(emp))
                project.Employees.Add(emp);
        }

        await _context.SaveChangesAsync();
    }

    public async Task SetManagerAsync(Guid projectId, Guid managerId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project != null)
        {
            project.ManagerId = managerId;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task RemoveEmployeeFromProjectAsync(Guid projectId, Guid employeeId)
    {
        var project = await _context.Projects
            .Include(p => p.Employees)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null) return;

        var employee = project.Employees.FirstOrDefault(e => e.Id == employeeId);
        if (employee != null)
        {
            project.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }
    }
}