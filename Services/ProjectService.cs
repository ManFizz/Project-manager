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
    private readonly IFileService _fileService;

    public ProjectService(ApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public IQueryable<Project> GetProjectsQuery()
    {
        return _context.Projects
            .Include(p => p.Manager)
            .Include(p => p.Employees);
    }

    public async Task<Project?> GetProjectByIdAsync(Guid id)
    {
        return await _context.Projects
            .Include(p => p.Manager)
            .Include(p => p.Employees)
            .FirstOrDefaultAsync(p => p.Id == id);
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
    
    public async Task CreateProjectAsync(Project project, List<Guid> employeeIds, List<IFormFile> files)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var paths = await _fileService.SaveFilesAsync(files);
            project.DocumentPaths.AddRange(paths);

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var employees = await _context.Employees
                .Where(e => employeeIds.Contains(e.Id))
                .ToListAsync();

            project.Employees = employees;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            
            _fileService.DeleteFiles(project.DocumentPaths);

            throw;
        }
    }
}