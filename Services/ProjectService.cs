using MegaProject.Data;
using MegaProject.Domain.Models;
using MegaProject.ViewModels;
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

    public async Task DeleteProjectAsync(Guid id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            
            _fileService.DeleteFiles(project.DocumentPaths);
        }
    }

    public async Task<List<EmployeeDto>> SearchEmployeesAsync(string term)
    {
        var query = _context.Employees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(e =>
                e.FirstName.Contains(term) ||
                e.LastName.Contains(term) ||
                e.Mail.Contains(term));
        }

        return await query
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Mail = e.Mail
            })
            .ToListAsync();
    }

    public async Task AddEmployeesAsync(Guid projectId, List<Guid> employeeIds)
    {
        employeeIds = employeeIds.Distinct().ToList();
        if (employeeIds.Count == 0)
            return;
            
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
    
    public async Task CreateProjectAsync(ProjectCreateViewModel model)
    {
        if (model.End < model.Start)
            throw new Exception("End date must be after start date");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var project = new Project
            {
                Name = model.Name,
                ClientName = model.ClientName,
                ExecutorName = model.ExecutorName,
                ManagerId = model.ManagerId,
                Start = model.Start,
                End = model.End,
                Priority = model.Priority
            };

            var paths = await _fileService.SaveFilesAsync(model.Files);
            project.DocumentPaths.AddRange(paths);

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var employeeIds = new List<Guid> { model.ManagerId };

            if (model.SelectedEmployeeIds != null && model.SelectedEmployeeIds.Count != 0)
                employeeIds.AddRange(model.SelectedEmployeeIds);

            employeeIds = employeeIds.Distinct().ToList();

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

            _fileService.DeleteFiles(model.Files.Select(f => f.FileName).ToList()); // или paths если вынесешь выше

            throw;
        }
    }
    
    public async Task UpdateProjectAsync(Project updated, List<IFormFile> files, Guid? id = null)
    {
        if (id != null && id != updated.Id)
            throw new Exception("Ids don't match");
        
        var project = await _context.Projects
            .Include(p => p.Employees)
            .FirstOrDefaultAsync(p => p.Id == updated.Id);
        
        if (project == null)
            throw new Exception("Project not found");

        if (updated.End < updated.Start)
            throw new Exception("Invalid dates");

        project.Name = updated.Name;
        project.ClientName = updated.ClientName;
        project.ExecutorName = updated.ExecutorName;
        project.Start = updated.Start;
        project.End = updated.End;
        project.Priority = updated.Priority;
        
        if (files.Count != 0)
        {
            var paths = await _fileService.SaveFilesAsync(files);
            project.DocumentPaths.AddRange(paths);
        }

        var oldFiles = project.DocumentPaths.ToList();
        var newFiles = updated.DocumentPaths;

        var filesToDelete = oldFiles.Except(newFiles).ToList();
        foreach (var file in filesToDelete)
            project.DocumentPaths.Remove(file);

        _fileService.DeleteFiles(filesToDelete);

        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateProjectAsync(Guid id, ProjectEditViewModel model)
    {
        var project = await _context.Projects
            .Include(p => p.Employees)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            throw new Exception("Project not found");

        if (model.End < model.Start)
            throw new Exception("End date must be after start date");

        project.Name = model.Name;
        project.ClientName = model.ClientName;
        project.ExecutorName = model.ExecutorName;
        project.Start = model.Start;
        project.End = model.End;
        project.Priority = model.Priority;

        if (model.Files.Count > 0)
        {
            var paths = await _fileService.SaveFilesAsync(model.Files);
            project.DocumentPaths.AddRange(paths);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteFileAsync(Guid projectId, string filePath)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return;

        if (!project.DocumentPaths.Contains(filePath))
            return;

        project.DocumentPaths.Remove(filePath);

        await _context.SaveChangesAsync();

        _fileService.DeleteFile(filePath);
    }
    
    public async Task RemoveEmployeeAsync(Guid projectId, Guid employeeId)
    {
        
        var project = await _context.Projects
            .Include(p => p.Employees)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        var employee = project?.Employees.FirstOrDefault(e => e.Id == employeeId);
        if (employee == null)
            return;
        
        project!.Employees.Remove(employee);
        await _context.SaveChangesAsync();
    }
    
    public async Task SetManagerAsync(Guid projectId, Guid employeeId)
    {
        var project = await _context.Projects
            .Include(p => p.Employees)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            throw new Exception("Project not found");

        if (project.Employees.All(e => e.Id != employeeId))
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                throw new Exception("Employee not found");

            project.Employees.Add(employee);
        }

        project.ManagerId = employeeId;

        await _context.SaveChangesAsync();
    }
    
    public async Task<ProjectListViewModel> GetProjectsAsync(ProjectListViewModel model)
    {
        var query = _context.Projects
            .Include(p => p.Manager)
            .Include(p => p.Employees)
            .AsQueryable();

        // FILTERS
        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            var search = model.Search;
            query = query.Where(p =>
                p.Name.Contains(search) ||
                p.ClientName.Contains(search) ||
                p.ExecutorName.Contains(search));
        }

        if (model.StartFrom.HasValue)
            query = query.Where(p => p.Start >= model.StartFrom.Value);

        if (model.StartTo.HasValue)
            query = query.Where(p => p.Start <= model.StartTo.Value);

        if (model.MinPriority.HasValue)
            query = query.Where(p => p.Priority >= model.MinPriority.Value);

        if (model.MaxPriority.HasValue)
            query = query.Where(p => p.Priority <= model.MaxPriority.Value);

        // SORTING
        query = model.SortColumn switch
        {
            "Name" => model.SortDirection == "asc"
                ? query.OrderBy(x => x.Name)
                : query.OrderByDescending(x => x.Name),

            "Client" => model.SortDirection == "asc"
                ? query.OrderBy(x => x.ClientName)
                : query.OrderByDescending(x => x.ClientName),

            "Executor" => model.SortDirection == "asc"
                ? query.OrderBy(x => x.ExecutorName)
                : query.OrderByDescending(x => x.ExecutorName),

            "Start" => model.SortDirection == "asc"
                ? query.OrderBy(x => x.Start)
                : query.OrderByDescending(x => x.Start),

            "End" => model.SortDirection == "asc"
                ? query.OrderBy(x => x.End)
                : query.OrderByDescending(x => x.End),

            "Priority" => model.SortDirection == "asc"
                ? query.OrderBy(x => x.Priority)
                : query.OrderByDescending(x => x.Priority),

            _ => query.OrderBy(x => x.Name)
        };

        model.Projects = await query.ToListAsync();

        return model;
    }
}