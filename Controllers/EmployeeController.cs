using MegaProject.Domain.Models;
using MegaProject.Domain.Models.Exceptions;
using MegaProject.Services;
using MegaProject.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MegaProject.Web.Controllers;

/// <summary>
/// CRUD controller for Employees
/// </summary>
public class EmployeeController(IEmployeeService employeeService) : Controller
{
    // GET: /Employee — list of all employees
    public async Task<IActionResult> Index()
    {
        var employees = await employeeService.GetAllAsync();
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

        await employeeService.CreateAsync(employee);

        return RedirectToAction(nameof(Index));
    }

    // GET: /Employee/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        
        var employee = await employeeService.GetByIdAsync(id);
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
        
        var employee = new Employee
        {
            Id = id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            MiddleName = model.MiddleName,
            Mail = model.Mail,
        };
        try
        {
            await employeeService.UpdateAsync(employee);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Employee/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await employeeService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch(BusinessRuleException e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
            var employees = await employeeService.GetAllAsync();
            return View("Index", employees);
        }
    }
}