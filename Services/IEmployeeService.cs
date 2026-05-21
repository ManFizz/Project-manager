using MegaProject.Domain.Models;

namespace MegaProject.Web.Services;

public interface IEmployeeService
{
    Task<List<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(Guid id);
    Task CreateAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(Guid id);
    Task<List<Employee>> GetByIdProject(Guid projectId);
}