using FluentAssertions;
using MegaProject.Data;
using MegaProject.Domain.Models;
using MegaProject.Domain.Models.Exceptions;
using MegaProject.Services;
using Microsoft.EntityFrameworkCore;

namespace MegaProject.Tests.Services;

public class EmployeeServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoEmployees()
    {
        await using var context = CreateContext();
        var service = new EmployeeService(context);

        var result = await service.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEmployees()
    {
        await using var context = CreateContext();
        context.Employees.AddRange(
            new Employee { FirstName = "Иван",   LastName = "Петров" },
            new Employee { FirstName = "Мария",  LastName = "Иванова" },
            new Employee { FirstName = "Сергей", LastName = "Сидоров" }
        );
        await context.SaveChangesAsync();

        var result = await new EmployeeService(context).GetAllAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_SortsByLastNameThenFirstName()
    {
        await using var context = CreateContext();
        context.Employees.AddRange(
            new Employee { FirstName = "Борис",   LastName = "Сидоров" },
            new Employee { FirstName = "Анна",    LastName = "Петров" },
            new Employee { FirstName = "Алексей", LastName = "Петров" }
        );
        await context.SaveChangesAsync();

        var result = await new EmployeeService(context).GetAllAsync();

        result.Select(e => e.FirstName).Should().ContainInOrder("Алексей", "Анна", "Борис");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEmployee_WhenExists()
    {
        await using var context = CreateContext();
        var employee = new Employee { FirstName = "Иван", LastName = "Петров", Mail = "ivan@test.com" };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var result = await new EmployeeService(context).GetByIdAsync(employee.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(employee.Id);
        result.FirstName.Should().Be("Иван");
        result.Mail.Should().Be("ivan@test.com");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        await using var context = CreateContext();

        var result = await new EmployeeService(context).GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_SavesAllFields()
    {
        await using var context = CreateContext();
        var employee = new Employee
        {
            FirstName  = "Иван",
            LastName   = "Петров",
            MiddleName = "Сергеевич",
            Mail       = "ivan@test.com"
        };

        await new EmployeeService(context).CreateAsync(employee);

        var saved = await context.Employees.FindAsync(employee.Id);
        saved.Should().NotBeNull();
        saved!.FirstName.Should().Be("Иван");
        saved.MiddleName.Should().Be("Сергеевич");
        saved.Mail.Should().Be("ivan@test.com");
    }

    [Fact]
    public async Task CreateAsync_WorksWithNullMiddleName()
    {
        await using var context = CreateContext();
        var employee = new Employee { FirstName = "Иван", LastName = "Петров", MiddleName = null };

        await new EmployeeService(context).CreateAsync(employee);

        var saved = await context.Employees.FindAsync(employee.Id);
        saved!.MiddleName.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_PreservesProvidedId()
    {
        await using var context = CreateContext();
        var id = Guid.NewGuid();
        var employee = new Employee { Id = id, FirstName = "Иван", LastName = "Петров" };

        await new EmployeeService(context).CreateAsync(employee);

        context.Employees.Should().Contain(e => e.Id == id);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAllEditableFields()
    {
        await using var context = CreateContext();
        var employee = new Employee
        {
            FirstName  = "Старое",
            LastName   = "Имя",
            MiddleName = "Отчество",
            Mail       = "old@test.com"
        };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        await new EmployeeService(context).UpdateAsync(new Employee
        {
            Id         = employee.Id,
            FirstName  = "Новое",
            LastName   = "Фамилия",
            MiddleName = "Новоевич",
            Mail       = "new@test.com"
        });

        var updated = await context.Employees.FindAsync(employee.Id);
        updated!.FirstName.Should().Be("Новое");
        updated.LastName.Should().Be("Фамилия");
        updated.MiddleName.Should().Be("Новоевич");
        updated.Mail.Should().Be("new@test.com");
    }

    [Fact]
    public async Task UpdateAsync_CanClearMiddleName()
    {
        await using var context = CreateContext();
        var employee = new Employee { FirstName = "Иван", LastName = "Петров", MiddleName = "Сергеевич" };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        await new EmployeeService(context).UpdateAsync(new Employee
        {
            Id         = employee.Id,
            FirstName  = employee.FirstName,
            LastName   = employee.LastName,
            MiddleName = null
        });

        var updated = await context.Employees.FindAsync(employee.Id);
        updated!.MiddleName.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_DoesNotChangeId()
    {
        await using var context = CreateContext();
        var employee = new Employee { FirstName = "Иван", LastName = "Петров" };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        var originalId = employee.Id;

        await new EmployeeService(context).UpdateAsync(new Employee
        {
            Id        = originalId,
            FirstName = "Новое",
            LastName  = "Имя"
        });

        context.Employees.Should().HaveCount(1);
        context.Employees.First().Id.Should().Be(originalId);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenEmployeeNotFound()
    {
        await using var context = CreateContext();

        var act = async () => await new EmployeeService(context)
            .UpdateAsync(new Employee { Id = Guid.NewGuid(), FirstName = "X", LastName = "Y" });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_RemovesEmployee_WhenNoManagedProjects()
    {
        await using var context = CreateContext();
        var employee = new Employee { FirstName = "Иван", LastName = "Петров" };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        await new EmployeeService(context).DeleteAsync(employee.Id);

        context.Employees.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenEmployeeNotFound()
    {
        await using var context = CreateContext();

        var act = async () => await new EmployeeService(context).DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsBusinessRuleException_WhenEmployeeIsManager()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "Менеджер", LastName = "Проектов" };
        var project = new Project
        {
            Name         = "Проект",
            ClientName   = "Клиент",
            ExecutorName = "Исполнитель",
            Manager      = manager
        };
        context.Employees.Add(manager);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var act = async () => await new EmployeeService(context).DeleteAsync(manager.Id);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task DeleteAsync_Succeeds_WhenEmployeeIsProjectMemberButNotManager()
    {
        await using var context = CreateContext();

        var manager  = new Employee { FirstName = "Менеджер", LastName = "Проектов" };
        var member   = new Employee { FirstName = "Участник", LastName = "Проекта" };
        var project  = new Project
        {
            Name         = "Проект",
            ClientName   = "Клиент",
            ExecutorName = "Исполнитель",
            Manager      = manager,
            Employees    = [manager, member]
        };
        context.Employees.AddRange(manager, member);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        await new EmployeeService(context).DeleteAsync(member.Id);

        context.Employees.Should().NotContain(e => e.Id == member.Id);
    }

    [Fact]
    public async Task GetByIdProject_ReturnsProjectMembers()
    {
        await using var context = CreateContext();
        var emp1 = new Employee { FirstName = "Алла",  LastName = "Воронова" };
        var emp2 = new Employee { FirstName = "Борис", LastName = "Громов" };
        var outsider = new Employee { FirstName = "Чужой", LastName = "Сотрудник" };
        var project = new Project
        {
            Name         = "Проект",
            ClientName   = "Клиент",
            ExecutorName = "Исполнитель",
            Manager      = emp1,
            Employees    = [emp1, emp2]
        };
        context.Employees.AddRange(emp1, emp2, outsider);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var result = await new EmployeeService(context).GetByIdProject(project.Id);

        result.Should().HaveCount(2);
        result.Should().NotContain(e => e.Id == outsider.Id);
    }

    [Fact]
    public async Task GetByIdProject_ReturnsEmpty_WhenProjectHasNoMembers()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "Менеджер", LastName = "Проектов" };
        var project = new Project
        {
            Name         = "Пустой проект",
            ClientName   = "Клиент",
            ExecutorName = "Исполнитель",
            Manager      = manager
        };
        context.Employees.Add(manager);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var result = await new EmployeeService(context).GetByIdProject(project.Id);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdProject_ReturnsOnlyMembersOfSpecificProject()
    {
        await using var context = CreateContext();
        var emp1 = new Employee { FirstName = "Алла",  LastName = "Первова" };
        var emp2 = new Employee { FirstName = "Борис", LastName = "Второв" };
        var manager = new Employee { FirstName = "Главный", LastName = "Менеджер" };

        var project1 = new Project { Name = "Проект 1", ClientName = "К1", ExecutorName = "И1", Manager = manager, Employees = [emp1] };
        var project2 = new Project { Name = "Проект 2", ClientName = "К2", ExecutorName = "И2", Manager = manager, Employees = [emp2] };

        context.Employees.AddRange(emp1, emp2, manager);
        context.Projects.AddRange(project1, project2);
        await context.SaveChangesAsync();

        var result = await new EmployeeService(context).GetByIdProject(project1.Id);

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(emp1.Id);
    }

    [Fact]
    public async Task GetByIdProject_ReturnsSortedByLastNameThenFirstName()
    {
        await using var context = CreateContext();
        var emp1 = new Employee { FirstName = "Жанна",  LastName = "Громова" };
        var emp2 = new Employee { FirstName = "Антон",  LastName = "Антонов" };
        var emp3 = new Employee { FirstName = "Зинаида",LastName = "Антонов" };
        var manager = new Employee { FirstName = "Менеджер", LastName = "Проектов" };
        var project = new Project
        {
            Name = "Проект", ClientName = "К", ExecutorName = "И",
            Manager = manager, Employees = [emp1, emp2, emp3]
        };
        context.Employees.AddRange(emp1, emp2, emp3, manager);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var result = await new EmployeeService(context).GetByIdProject(project.Id);

        result[0].FirstName.Should().Be("Антон");
        result[1].FirstName.Should().Be("Зинаида");
        result[2].LastName.Should().Be("Громова");
    }
}
