using MegaProject.Services;
using MegaProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MegaProject.Controllers;

/// <summary>
/// CRUD controller for Projects (5-step wizard)
/// </summary>
public class ProjectController(IProjectService projectService) : Controller
{
    // GET: /Project — list of all projects with filtering and sorting
    [HttpGet]
    public async Task<IActionResult> Index(ProjectListViewModel model)
    {
        var result = await projectService.GetProjectsAsync(model);
        return View(result);
    }

    // GET: /Project/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new ProjectCreateViewModel());
    }

    // POST: /Project/Create — save project from wizard
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await projectService.CreateProjectAsync(model);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    // GET: /Project/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var project = await projectService.GetProjectByIdAsync(id);
        if (project == null) return NotFound();

        var model = new ProjectEditViewModel
        {
            Id = project.Id,
            Name = project.Name,
            ClientName = project.ClientName,
            ExecutorName = project.ExecutorName,
            Start = project.Start,
            End = project.End,
            Priority = project.Priority,
            DocumentPaths = project.DocumentPaths
        };

        return View(model);
    }

    // POST: /Project/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProjectEditViewModel updatedProject)
    {
        if (!ModelState.IsValid)
            return View(updatedProject);
        
        try
        {
            await projectService.UpdateProjectAsync(id, updatedProject);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(updatedProject);
        }
    }
    
    // POST: /Project/Delete/{projectId}:{filePath}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFile(Guid projectId, string filePath)
    {
        await projectService.DeleteFileAsync(projectId, filePath);
        return RedirectToAction("Edit", new { id = projectId });
    }

    // POST: /Project/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await projectService.DeleteProjectAsync(id); //TODO: move project to archive
        // TODO: transfer uploaded file to archive
        return RedirectToAction(nameof(Index));
    }

    // GET: /Project/SearchEmployees/{term} - AJAX search employees (used in wizard)
    [HttpGet]
    public async Task<JsonResult> SearchEmployees(string term)
    {
        return Json(await projectService.SearchEmployeesAsync(term)); // EmployeeDto
    }
    
    // GET: /Project/Details/{id}
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var project = await projectService.GetProjectByIdAsync(id);
        if (project == null) return NotFound();

        return View(project);
    }

    // POST: /Project/AddEmployees - Add employees to existing project
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEmployees(Guid projectId, List<Guid> employeeIds)
    {
        await projectService.AddEmployeesAsync(projectId, employeeIds);
        return RedirectToAction(nameof(Details), new { id = projectId });
    }
    
    // POST: /Project/RemoveEmployee - Add employees on existing project
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveEmployee(Guid projectId, Guid employeeId)
    {
        await projectService.RemoveEmployeeAsync(projectId, employeeId);
        return RedirectToAction(nameof(Details), new { id = projectId });
    }
    
    // POST: /Project/SetManager - Change manager on existing project
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetManager(Guid projectId, Guid employeeId)
    {
        try
        {
            await projectService.SetManagerAsync(projectId, employeeId);
            return RedirectToAction(nameof(Details), new { id = projectId });
        }
        catch(Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }
}