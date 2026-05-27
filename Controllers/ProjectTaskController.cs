using MegaProject.Domain.Models;
using MegaProject.Domain.Models.Exceptions;
using MegaProject.Web.Services;
using MegaProject.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using TaskStatus = MegaProject.Domain.Models.Enums.TaskStatus;

namespace MegaProject.Web.Controllers;

public class ProjectTaskController(IProjectTaskService projectTaskService, IEmployeeService employeeService) : Controller
{
    
    // GET: /ProjectTask/Index?projectId={projectId}&status={status}
    public async Task<IActionResult> Index(Guid projectId, TaskStatus? status = null)
    {
        var tasks = await projectTaskService.GetByProjectAsync(projectId, status);
        ViewBag.ProjectId = projectId;
        ViewBag.CurrentStatus = status;
        return View(tasks);
    }
    
    // GET: /Task/Create/{projectId}
    [HttpGet]
    public async Task<IActionResult> Create(Guid projectId)
    {
        var viewModel = new ProjectTaskViewModel
        {
            ProjectId = projectId,
            Employees = await employeeService.GetByIdProject(projectId),
        };
        return View(viewModel);
    }
    
    // POST: /Task/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectTaskViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var task = new ProjectTask
        {
            Name = model.Name,
            Comment = model.Comment,
            Priority = model.Priority,
            Status = model.Status,
            ProjectId = model.ProjectId,
            AuthorId = model.AuthorId,
            WorkerId = model.WorkerId,
        };

        await projectTaskService.CreateAsync(task);

        return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
    }
    
    // GET: /Task/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var task = await projectTaskService.GetByIdAsync(id);
        if (task == null) return NotFound();

        var model = new ProjectTaskViewModel
        {
            Name = task.Name,
            Comment = task.Comment,
            Priority = task.Priority,
            Status = task.Status,
            WorkerId = task.WorkerId,
            AuthorId = task.AuthorId,
            ProjectId = task.ProjectId,
            Employees = await employeeService.GetByIdProject(task.ProjectId),
        };

        ViewBag.ProjectTaskId = id;
        return View(model);
    }
    
    
    // POST: /Task/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProjectTaskViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        
        var task = new ProjectTask
        {
            Id = id,
            Name = model.Name,
            Comment = model.Comment,
            Priority = model.Priority,
            Status = model.Status,
            WorkerId = model.WorkerId,
            AuthorId = model.AuthorId,
            ProjectId = model.ProjectId,
        };
        try
        {
            await projectTaskService.UpdateAsync(task);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
    }
    
    // POST: /Task/Delete/{id}:{projectId}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid projectId)
    {
        try
        {
            await projectTaskService.DeleteAsync(id);
            return RedirectToAction(nameof(Index), new { projectId = projectId });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch(BusinessRuleException e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
            return RedirectToAction(nameof(Index), new { projectId });
        }
    }
}