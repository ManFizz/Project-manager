using MegaProject.Data;
using MegaProject.Domain.Models;
using MegaProject.Domain.Models.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MegaProject.Services;

public class EmployeeService(ApplicationDbContext context) : IEmployeeService
{
    public Task<List<Employee>> GetAllAsync() =>
        context.Employees
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();

    public Task<Employee?> GetByIdAsync(Guid id) =>
        context.Employees.FindAsync(id).AsTask();

    public Task CreateAsync(Employee employee)
    {
        context.Employees.Add(employee);
        return context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Employee employee)
    {
        var found = await GetByIdAsync(employee.Id);
        if (found == null)
            throw new NotFoundException("Employee not found");

        found.FirstName = employee.FirstName;
        found.LastName = employee.LastName;
        found.MiddleName = employee.MiddleName;
        found.Mail = employee.Mail;

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var employee = await context.Employees
            .Include(e => e.ManagedProjects)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            throw new NotFoundException("Employee not found");

        if (employee.ManagedProjects.Count != 0)
            throw new BusinessRuleException("Cannot delete employee who is a manager of one or more projects.");

        context.Employees.Remove(employee);
        await context.SaveChangesAsync();
    }

    public Task<List<Employee>> GetByIdProject(Guid projectId) =>
        context.Employees
            .Where(e => e.Projects.Any(p => p.Id == projectId))
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
}
