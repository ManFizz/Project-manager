using MegaProject.Data;
using MegaProject.Models;
using MegaProject.Services;
using MegaProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MegaProject.Controllers;

/// <summary>
/// CRUD controller for Employees
/// </summary>
public class EmployeeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IProjectService _projectService;

    public EmployeeController(ApplicationDbContext context, IProjectService projectService)
    {
        _context = context;
        _projectService = projectService;
    }

    // GET: /Employee — list of all employees
    public async Task<IActionResult> Index()
    {
        var employees = await _context.Employees
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();

        return View(employees);
    }

    // GET: /Employee/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateEmployeeViewModel());
    }

    // POST: /Employee/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var employee = new Employee
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            MiddleName = model.MiddleName,
            Mail = model.Mail
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: /Employee/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        var model = new CreateEmployeeViewModel
        {
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            MiddleName = employee.MiddleName,
            Mail = employee.Mail
        };

        ViewBag.EmployeeId = id;
        return View(model);
    }

    // POST: /Employee/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CreateEmployeeViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        employee.FirstName = model.FirstName;
        employee.LastName = model.LastName;
        employee.MiddleName = model.MiddleName;
        employee.Mail = model.Mail;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST: /Employee/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee != null)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}