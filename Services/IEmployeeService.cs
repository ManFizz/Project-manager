using MegaProject.Domain.Models;

namespace MegaProject.Web.Services;

public interface IEmployeeService
{
    Task<List<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(Guid id);
    Task CreateAsync(Employee employee);
    Task UpdateAsync(Employee employee); //bool - employee was found
    Task DeleteAsync(Guid id); //bool - employee was found
}