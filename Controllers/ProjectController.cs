using MegaProject.Data;
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
    private readonly ApplicationDbContext _context;
    private readonly IProjectService _projectService;

    // GET: /Project — list of all projects
    public async Task<IActionResult> Index()
    {
        var projects = await _context.Projects
            .OrderBy(e => e.Priority)
            .ThenBy(e => e.Name)
            .Include(e => e.Manager)
            .ToListAsync();
        return View(projects);
    }
    
    public ProjectController(ApplicationDbContext context, IProjectService projectService)
    {
        _context = context;
        _projectService = projectService;
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

    
    // POST: /Project/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var project = new Project
        {
            Name = model.Name,
            ClientName = model.ClientName,
            ExecutorName = model.ExecutorName,
            ManagerId = model.ManagerId,
            Start = model.Start,
            End = model.End,
            Priority = model.Priority,
            DocumentPaths = model.DocumentPaths ?? new List<string>()
        };

        await _projectService.CreateProjectAsync(project);

        if (model.SelectedEmployeeIds?.Any() == true)
        {
            await _projectService.AddEmployeesToProjectAsync(project.Id, model.SelectedEmployeeIds);
        }

        return RedirectToAction("Index", "Home");
    }
    
    // GET: /Employee/SearchEmployees/{term}
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

        return new JsonResult(result);
    }
}