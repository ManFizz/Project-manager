using MegaProject.Data;
using MegaProject.Domain.Models;
using MegaProject.Domain.Models.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MegaProject.Web.Services;

public class EmployeeService(ApplicationDbContext context) : IEmployeeService
{
    public Task<List<Employee>> GetAllAsync()
    {
        var employees = context.Employees
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
        
        return employees;
    }

    public Task<Employee?> GetByIdAsync(Guid id)
    {
        var employee = context.Employees.FindAsync(id).AsTask();
        return employee;
    }

    public Task CreateAsync(Employee employee)
    {
        context.Employees.Add(employee);
        
        return context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Employee employee)
    {
        var foundEmployee = await GetByIdAsync(employee.Id);
        if (foundEmployee == null)
            throw new NotFoundException("Employee not found");
        
        foundEmployee.FirstName = employee.FirstName;
        foundEmployee.LastName = employee.LastName;
        foundEmployee.MiddleName = employee.MiddleName;
        foundEmployee.Mail = employee.Mail;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var employee = await context.Employees
            .Include(e => e.ManagedProjects)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            throw new NotFoundException("Employee not found");

        // Block deletion if the employee is the manager of at least one project.
        if (employee.ManagedProjects.Count != 0)
            throw new BusinessRuleException("Cannot delete employee who is a manager of one or more projects.");

        context.Employees.Remove(employee);
        await context.SaveChangesAsync();
    }
}