using MegaProject.Data;
using MegaProject.Models;
using MegaProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MegaProject.Controllers;

/// <summary>
/// CRUD for employees
/// </summary>
public class EmployeeController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmployeeController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateEmployeeViewModel());
    }

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

        return RedirectToAction("Index", "Home");
    }
}