using MegaProject.Models;
using MegaProject.Services;
using MegaProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MegaProject.Controllers;

/// <summary>
/// CRUD controller for Projects (5-step wizard)
/// </summary>
public class ProjectController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IFileService _fileService;

    public ProjectController(IProjectService projectService, IFileService fileService)
    {
        _projectService = projectService;
        _fileService = fileService;
    }

    // GET: /Project — list of all projects with filtering and sorting
    public async Task<IActionResult> Index(
        string? search,
        DateTime? startFrom,
        DateTime? startTo,
        int? minPriority,
        int? maxPriority)
    {
        var projects = await _projectService.GetAllProjectsAsync(search, startFrom, startTo, minPriority, maxPriority);
        return View(projects);
    }

    // GET: /Project/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ProjectCreateViewModel
        {
            Employees = (await _projectService.SearchEmployeesAsync("")).ToList()
        };
        return View(model);
    }

    // POST: /Project/Create — save project from wizard
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        
        if (model.End < model.Start)
        {
            ModelState.AddModelError("End", "End date must be after start date");
            return View(model);
        }

        var project = new Project
        {
            Name = model.Name,
            ClientName = model.ClientName,
            ExecutorName = model.ExecutorName,
            ManagerId = model.ManagerId,
            Start = model.Start,
            End = model.End,
            Priority = model.Priority,
            DocumentPaths = model.DocumentPaths
        };
        
        var paths = await _fileService.SaveFilesAsync(model.Files);
        project.DocumentPaths.AddRange(paths);

        await _projectService.CreateProjectAsync(project);

        var employeeIds = new List<Guid> { model.ManagerId };
        if (model.SelectedEmployeeIds.Count != 0)
            employeeIds.AddRange(model.SelectedEmployeeIds);

        await _projectService.AddEmployeesToProjectAsync(project.Id, employeeIds.Distinct().ToList());

        return RedirectToAction(nameof(Index));
    }

    // GET: /Project/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null) return NotFound();

        return View(project);
    }

    // POST: /Project/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Project project, List<IFormFile> files)
    {
        if (id != project.Id) return NotFound();

        var existingProject = await _projectService.GetProjectByIdAsync(id);
        if (existingProject == null) return NotFound();

        if (!ModelState.IsValid)
            return View(project);

        if (project.End < project.Start)
        {
            ModelState.AddModelError("End", "End date must be after start date");
            return View(project);
        }

        existingProject.Name = project.Name;
        existingProject.ClientName = project.ClientName;
        existingProject.ExecutorName = project.ExecutorName;
        existingProject.Start = project.Start;
        existingProject.End = project.End;
        existingProject.Priority = project.Priority;
        // ManagerId & list Employees dont change
        
        try
        {
            if (files?.Count > 0)
            {
                var paths = await _fileService.SaveFilesAsync(files);
                existingProject.DocumentPaths.AddRange(paths);
            }

            await _projectService.UpdateProjectAsync(existingProject);
            if (!ModelState.IsValid)
                return View(existingProject);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("DocumentPaths", "File upload failed");
            return View(project);
        }
    }
    
    // POST: /Project/Delete/{projectId}:{filePath}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFile(Guid projectId, string filePath)
    {
        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project == null) return RedirectToAction("Index");

        if (!project.DocumentPaths.Contains(filePath))
            return RedirectToAction("Edit", new { id = projectId });
        
        project.DocumentPaths.Remove(filePath);
        await _projectService.UpdateProjectAsync(project);

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));

        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);

        return RedirectToAction("Edit", new { id = projectId });
    }

    // POST: /Project/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _projectService.DeleteProjectAsync(id); //TODO: move project to archive
        // TODO: transfer uploaded file to archive
        return RedirectToAction(nameof(Index));
    }

    // GET: /Project/SearchEmployees/{term} - AJAX search employees (used in wizard)
    [HttpGet]
    public async Task<JsonResult> SearchEmployees(string term)
    {
        var employees = await _projectService.SearchEmployeesAsync(term ?? "");
        var result = employees.Select(e => new
        {
            id = e.Id,
            firstName = e.FirstName,
            lastName = e.LastName,
            mail = e.Mail
        });

        return Json(result);
    }
    
    // GET: /Project/Details/{id}
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null) return NotFound();

        return View(project);
    }

    // POST: /Project/AddEmployees - Add employees to existing project
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEmployees(Guid projectId, List<Guid> employeeIds)
    {
        employeeIds = employeeIds.Distinct().ToList();
        if (employeeIds.Count != 0)
            await _projectService.AddEmployeesToProjectAsync(projectId, employeeIds);
        
        return RedirectToAction(nameof(Details), new { id = projectId });
    }
    
    // POST: /Project/RemoveEmployee - Add employees on existing project
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveEmployee(Guid projectId, Guid employeeId)
    {
        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project == null)
            return RedirectToAction(nameof(Details), new { id = projectId });
        
        var employee = project.Employees.FirstOrDefault(e => e.Id == employeeId);
        if (employee == null)
            return RedirectToAction(nameof(Details), new { id = projectId });
        
        project.Employees.Remove(employee);
        await _projectService.UpdateProjectAsync(project);
        
        return RedirectToAction(nameof(Details), new { id = projectId });
    }
    
    // POST: /Project/SetManager - Change manager on existing project
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetManager(Guid projectId, Guid employeeId)
    {
        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project == null)
            return RedirectToAction(nameof(Index));

        if (project.Employees.All(e => e.Id != employeeId))
        {
            await _projectService.AddEmployeesToProjectAsync(projectId, [employeeId]);
        }

        project.ManagerId = employeeId;

        await _projectService.UpdateProjectAsync(project);

        return RedirectToAction(nameof(Details), new { id = projectId });
    }
}