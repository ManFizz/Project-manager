using MegaProject.Domain.Models;
using MegaProject.Services;
using MegaProject.Web.Services;
using MegaProject.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MegaProject.Web.Controllers;

/// <summary>
/// CRUD controller for Projects
/// </summary>
public class ProjectController(IProjectService projectService, IFileService fileService) : Controller
{
    // GET: /Project
    [HttpGet]
    public async Task<IActionResult> Index(ProjectListViewModel model)
    {
        var filter = new ProjectFilter
        {
            Search = model.Search,
            StartFrom = model.StartFrom,
            StartTo = model.StartTo,
            MinPriority = model.MinPriority,
            MaxPriority = model.MaxPriority,
            SortColumn = model.SortColumn,
            SortDirection = model.SortDirection
        };

        model.Projects = await projectService.GetProjectsAsync(filter);
        return View(model);
    }

    // GET: /Project/Create
    [HttpGet]
    public IActionResult Create() => View(new ProjectCreateViewModel());

    // POST: /Project/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var paths = await fileService.SaveFilesAsync(model.Files);

            var project = new Project
            {
                Name = model.Name,
                ClientName = model.ClientName,
                ExecutorName = model.ExecutorName,
                ManagerId = model.ManagerId,
                Start = model.Start,
                End = model.End,
                Priority = model.Priority,
                DocumentPaths = paths
            };

            await projectService.CreateProjectAsync(project, model.SelectedEmployeeIds);
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
    public async Task<IActionResult> Edit(Guid id, ProjectEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var oldProject = await projectService.GetProjectByIdAsync(id);
            if (oldProject == null) return NotFound();

            // Сохраняем новые файлы
            var newPaths = model.Files.Count > 0
                ? await fileService.SaveFilesAsync(model.Files)
                : new List<string>();

            // Финальный список = те что оставили + новые
            var finalPaths = model.DocumentPaths.Concat(newPaths).ToList();

            // Удаляем файлы которые убрали из проекта
            var deletedPaths = oldProject.DocumentPaths.Except(model.DocumentPaths).ToList();
            fileService.DeleteFiles(deletedPaths);

            var updated = new Project
            {
                Name = model.Name,
                ClientName = model.ClientName,
                ExecutorName = model.ExecutorName,
                Start = model.Start,
                End = model.End,
                Priority = model.Priority,
                DocumentPaths = finalPaths
            };

            await projectService.UpdateProjectAsync(id, updated);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    // POST: /Project/DeleteFile
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFile(Guid projectId, string filePath)
    {
        await projectService.RemoveDocumentPathAsync(projectId, filePath);
        fileService.DeleteFile(filePath);
        return RedirectToAction("Edit", new { id = projectId });
    }

    // POST: /Project/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var paths = await projectService.DeleteProjectAsync(id);
        fileService.DeleteFiles(paths);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Project/SearchEmployees — AJAX
    [HttpGet]
    public async Task<JsonResult> SearchEmployees(string term) =>
        Json(await projectService.SearchEmployeesAsync(term));

    // GET: /Project/Details/{id}
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var project = await projectService.GetProjectByIdAsync(id);
        if (project == null) return NotFound();
        return View(project);
    }

    // POST: /Project/AddEmployees
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEmployees(Guid projectId, List<Guid> employeeIds)
    {
        await projectService.AddEmployeesAsync(projectId, employeeIds);
        return RedirectToAction(nameof(Details), new { id = projectId });
    }

    // POST: /Project/RemoveEmployee
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveEmployee(Guid projectId, Guid employeeId)
    {
        await projectService.RemoveEmployeeAsync(projectId, employeeId);
        return RedirectToAction(nameof(Details), new { id = projectId });
    }

    // POST: /Project/SetManager
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetManager(Guid projectId, Guid employeeId)
    {
        try
        {
            await projectService.SetManagerAsync(projectId, employeeId);
            return RedirectToAction(nameof(Details), new { id = projectId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }
}
