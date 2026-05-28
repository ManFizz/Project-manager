using FluentAssertions;
using MegaProject.Data;
using MegaProject.Domain.Models;
using MegaProject.Services;
using Microsoft.EntityFrameworkCore;

namespace MegaProject.Tests.Services;

public class ProjectServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Project MakeProject(Employee manager, string name = "Проект",
        string client = "Клиент", string executor = "Исполнитель",
        int priority = 5, DateTime? start = null, DateTime? end = null) =>
        new()
        {
            Name         = name,
            ClientName   = client,
            ExecutorName = executor,
            Manager      = manager,
            Priority     = priority,
            Start        = start ?? DateTime.Today,
            End          = end   ?? DateTime.Today.AddDays(30)
        };


    [Fact]
    public async Task GetProjectByIdAsync_ReturnsProject_WhenExists()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "Менеджер", LastName = "Проектов" };
        var project = MakeProject(manager, "Тестовый проект");
        context.Employees.Add(manager);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var result = await new ProjectService(context).GetProjectByIdAsync(project.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Тестовый проект");
    }

    [Fact]
    public async Task GetProjectByIdAsync_ReturnsNull_WhenNotFound()
    {
        await using var context = CreateContext();

        var result = await new ProjectService(context).GetProjectByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProjectByIdAsync_IncludesManagerAndEmployees()
    {
        await using var context = CreateContext();
        var manager  = new Employee { FirstName = "Менеджер", LastName = "Проектов" };
        var employee = new Employee { FirstName = "Участник", LastName = "Команды" };
        var project  = MakeProject(manager);
        project.Employees = [manager, employee];
        context.Employees.AddRange(manager, employee);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var result = await new ProjectService(context).GetProjectByIdAsync(project.Id);

        result!.Manager.Should().NotBeNull();
        result.Employees.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetProjectsAsync_ReturnsAll_WhenFilterIsEmpty()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        context.Employees.Add(manager);
        context.Projects.AddRange(MakeProject(manager, "А"), MakeProject(manager, "Б"), MakeProject(manager, "В"));
        await context.SaveChangesAsync();

        var result = await new ProjectService(context).GetProjectsAsync(new ProjectFilter());

        result.Should().HaveCount(3);
    }

    [Theory]
    [InlineData("Альфа",    1)] // Name
    [InlineData("Клиент А", 1)] // ClientName
    [InlineData("Исп Б",    1)] // ExecutorName
    [InlineData("проект",   2)] // Project name
    [InlineData("zzz",      0)] // Empty result
    public async Task GetProjectsAsync_FiltersBySearch(string search, int expectedCount)
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        context.Employees.Add(manager);
        context.Projects.AddRange(
            new Project { Name = "Альфа проект",  ClientName = "Клиент А",  ExecutorName = "Исп А",  Manager = manager, Start = DateTime.Today, End = DateTime.Today.AddDays(1) },
            new Project { Name = "Бета проект",   ClientName = "Клиент Б",  ExecutorName = "Исп Б",  Manager = manager, Start = DateTime.Today, End = DateTime.Today.AddDays(1) },
            new Project { Name = "Гамма",         ClientName = "Заказчик",  ExecutorName = "Команда",Manager = manager, Start = DateTime.Today, End = DateTime.Today.AddDays(1) }
        );
        await context.SaveChangesAsync();

        var result = await new ProjectService(context).GetProjectsAsync(new ProjectFilter { Search = search });

        result.Should().HaveCount(expectedCount);
    }

    [Fact]
    public async Task GetProjectsAsync_FiltersByStartFrom()
    {
        await using var context = CreateContext();
        var manager  = new Employee { FirstName = "М", LastName = "П" };
        var baseDate = new DateTime(2026, 1, 1);
        context.Employees.Add(manager);
        context.Projects.AddRange(
            MakeProject(manager, "Старый",  start: baseDate.AddDays(-10), end: baseDate.AddDays(20)),
            MakeProject(manager, "Новый",   start: baseDate,              end: baseDate.AddDays(30)),
            MakeProject(manager, "Будущий", start: baseDate.AddDays(10),  end: baseDate.AddDays(40))
        );
        await context.SaveChangesAsync();

        var result = await new ProjectService(context)
            .GetProjectsAsync(new ProjectFilter { StartFrom = baseDate });

        result.Should().HaveCount(2);
        result.Should().NotContain(p => p.Name == "Старый");
    }

    [Fact]
    public async Task GetProjectsAsync_FiltersByStartTo()
    {
        await using var context = CreateContext();
        var manager  = new Employee { FirstName = "М", LastName = "П" };
        var baseDate = new DateTime(2026, 1, 1);
        context.Employees.Add(manager);
        context.Projects.AddRange(
            MakeProject(manager, "Ранний",  start: baseDate.AddDays(-5), end: baseDate.AddDays(25)),
            MakeProject(manager, "Точный",  start: baseDate,             end: baseDate.AddDays(30)),
            MakeProject(manager, "Поздний", start: baseDate.AddDays(5),  end: baseDate.AddDays(35))
        );
        await context.SaveChangesAsync();

        var result = await new ProjectService(context)
            .GetProjectsAsync(new ProjectFilter { StartTo = baseDate });

        result.Should().HaveCount(2);
        result.Should().NotContain(p => p.Name == "Поздний");
    }

    [Fact]
    public async Task GetProjectsAsync_FiltersByPriorityRange()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        context.Employees.Add(manager);
        context.Projects.AddRange(
            MakeProject(manager, "Низкий",   priority: 1),
            MakeProject(manager, "Средний",  priority: 5),
            MakeProject(manager, "Высокий",  priority: 10)
        );
        await context.SaveChangesAsync();

        var result = await new ProjectService(context)
            .GetProjectsAsync(new ProjectFilter { MinPriority = 3, MaxPriority = 7 });

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Средний");
    }

    [Fact]
    public async Task GetProjectsAsync_CombinesMultipleFilters()
    {
        await using var context = CreateContext();
        var manager  = new Employee { FirstName = "М", LastName = "П" };
        var baseDate = new DateTime(2026, 6, 1);
        context.Employees.Add(manager);
        context.Projects.AddRange(
            // all is ok
            new Project { Name = "Нужный", ClientName = "Клиент", ExecutorName = "Исп", Manager = manager, Priority = 5, Start = baseDate, End = baseDate.AddDays(30) },
            // filter by priority
            new Project { Name = "Лишний приоритет", ClientName = "Клиент", ExecutorName = "Исп", Manager = manager, Priority = 1, Start = baseDate, End = baseDate.AddDays(30) },
            // filter by start from date
            new Project { Name = "Лишняя дата", ClientName = "Клиент", ExecutorName = "Исп", Manager = manager, Priority = 5, Start = baseDate.AddDays(-10), End = baseDate.AddDays(20) }
        );
        await context.SaveChangesAsync();

        var result = await new ProjectService(context).GetProjectsAsync(new ProjectFilter
        {
            StartFrom   = baseDate,
            MinPriority = 3
        });

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Нужный");
    }

    [Theory]
    [InlineData("Name",     "asc",  "Альфа")]
    [InlineData("Name",     "desc", "Гамма")]
    [InlineData("Client",   "asc",  "Альфа")]  // ClientName: "Клиент А" < "Клиент Б" < "Клиент В"
    [InlineData("Priority", "asc",  "Альфа")]  // Priority: 1, 5, 10
    [InlineData("Priority", "desc", "Гамма")]
    [InlineData("Start",    "asc",  "Альфа")]
    [InlineData("Start",    "desc", "Гамма")]
    public async Task GetProjectsAsync_SortsCorrectly(string column, string direction, string expectedFirstName)
    {
        await using var context = CreateContext();
        var manager  = new Employee { FirstName = "М", LastName = "П" };
        var baseDate = new DateTime(2026, 1, 1);
        context.Employees.Add(manager);
        context.Projects.AddRange(
            new Project { Name = "Альфа", ClientName = "Клиент А", ExecutorName = "Исп", Manager = manager, Priority = 1,  Start = baseDate,              End = baseDate.AddDays(30) },
            new Project { Name = "Бета",  ClientName = "Клиент Б", ExecutorName = "Исп", Manager = manager, Priority = 5,  Start = baseDate.AddDays(5),   End = baseDate.AddDays(35) },
            new Project { Name = "Гамма", ClientName = "Клиент В", ExecutorName = "Исп", Manager = manager, Priority = 10, Start = baseDate.AddDays(10),  End = baseDate.AddDays(40) }
        );
        await context.SaveChangesAsync();

        var result = await new ProjectService(context)
            .GetProjectsAsync(new ProjectFilter { SortColumn = column, SortDirection = direction });

        result.First().Name.Should().Be(expectedFirstName);
    }

    [Fact]
    public async Task GetProjectsAsync_DefaultSortsByNameAsc_WhenColumnUnknown()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        context.Employees.Add(manager);
        context.Projects.AddRange(MakeProject(manager, "Ягода"), MakeProject(manager, "Арбуз"));
        await context.SaveChangesAsync();

        var result = await new ProjectService(context)
            .GetProjectsAsync(new ProjectFilter { SortColumn = "НесуществующаяКолонка" });

        result.First().Name.Should().Be("Арбуз");
    }

    [Fact]
    public async Task CreateProjectAsync_SavesAllFields()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        context.Employees.Add(manager);
        await context.SaveChangesAsync();

        var project = new Project
        {
            Name         = "Новый проект",
            ClientName   = "ООО Клиент",
            ExecutorName = "ООО Исполнитель",
            ManagerId    = manager.Id,
            Priority     = 7,
            Start        = new DateTime(2026, 1, 1),
            End          = new DateTime(2026, 12, 31),
            DocumentPaths = ["/uploads/doc.pdf"]
        };

        await new ProjectService(context).CreateProjectAsync(project);

        var saved = await context.Projects.FindAsync(project.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Новый проект");
        saved.Priority.Should().Be(7);
        saved.DocumentPaths.Should().Contain("/uploads/doc.pdf");
    }

    [Fact]
    public async Task CreateProjectAsync_AddsManagerToEmployeesList()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        context.Employees.Add(manager);
        await context.SaveChangesAsync();

        var project = new Project
        {
            Name = "Проект", ClientName = "К", ExecutorName = "И",
            ManagerId = manager.Id, Start = DateTime.Today, End = DateTime.Today.AddDays(1)
        };
        await new ProjectService(context).CreateProjectAsync(project);

        var saved = await context.Projects.Include(p => p.Employees).FirstAsync(p => p.Id == project.Id);
        saved.Employees.Should().Contain(e => e.Id == manager.Id);
    }

    [Fact]
    public async Task CreateProjectAsync_AddsSelectedEmployees()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        var emp1    = new Employee { FirstName = "А", LastName = "Б" };
        var emp2    = new Employee { FirstName = "В", LastName = "Г" };
        context.Employees.AddRange(manager, emp1, emp2);
        await context.SaveChangesAsync();

        var project = new Project
        {
            Name = "Проект", ClientName = "К", ExecutorName = "И",
            ManagerId = manager.Id, Start = DateTime.Today, End = DateTime.Today.AddDays(1)
        };
        await new ProjectService(context).CreateProjectAsync(project, [emp1.Id, emp2.Id]);

        var saved = await context.Projects.Include(p => p.Employees).FirstAsync(p => p.Id == project.Id);
        saved.Employees.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateProjectAsync_ThrowsException_WhenEndBeforeStart()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        context.Employees.Add(manager);
        await context.SaveChangesAsync();

        var project = new Project
        {
            Name = "Проект", ClientName = "К", ExecutorName = "И",
            ManagerId = manager.Id,
            Start = new DateTime(2026, 6, 1),
            End   = new DateTime(2026, 1, 1) // раньше старта
        };

        var act = async () => await new ProjectService(context).CreateProjectAsync(project);

        await act.Should().ThrowAsync<Exception>().WithMessage("*date*");
    }

    [Fact]
    public async Task UpdateProjectAsync_UpdatesAllFields()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        var project = MakeProject(manager, "Старое название", priority: 1);
        context.Employees.Add(manager);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        await new ProjectService(context).UpdateProjectAsync(project.Id, new Project
        {
            Name         = "Новое название",
            ClientName   = "Новый клиент",
            ExecutorName = "Новый исп",
            Priority     = 9,
            Start        = DateTime.Today,
            End          = DateTime.Today.AddDays(60),
            DocumentPaths = ["/uploads/new.pdf"]
        });

        var updated = await context.Projects.FindAsync(project.Id);
        updated!.Name.Should().Be("Новое название");
        updated.Priority.Should().Be(9);
        updated.DocumentPaths.Should().Contain("/uploads/new.pdf");
    }

    [Fact]
    public async Task UpdateProjectAsync_ThrowsException_WhenProjectNotFound()
    {
        await using var context = CreateContext();

        var act = async () => await new ProjectService(context)
            .UpdateProjectAsync(Guid.NewGuid(), new Project
            {
                Name = "X", ClientName = "X", ExecutorName = "X",
                Start = DateTime.Today, End = DateTime.Today.AddDays(1)
            });

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task UpdateProjectAsync_ThrowsException_WhenEndBeforeStart()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        var project = MakeProject(manager);
        context.Employees.Add(manager);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var act = async () => await new ProjectService(context).UpdateProjectAsync(project.Id, new Project
        {
            Name = "X", ClientName = "X", ExecutorName = "X",
            Start = new DateTime(2026, 6, 1),
            End   = new DateTime(2026, 1, 1)
        });

        await act.Should().ThrowAsync<Exception>().WithMessage("*date*");
    }

    [Fact]
    public async Task DeleteProjectAsync_RemovesProject()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        var project = MakeProject(manager);
        context.Employees.Add(manager);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        await new ProjectService(context).DeleteProjectAsync(project.Id);

        context.Projects.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteProjectAsync_ReturnsDocumentPaths()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        var project = MakeProject(manager);
        project.DocumentPaths = ["/uploads/a.pdf", "/uploads/b.pdf"];
        context.Employees.Add(manager);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var paths = await new ProjectService(context).DeleteProjectAsync(project.Id);

        paths.Should().HaveCount(2);
        paths.Should().Contain("/uploads/a.pdf");
    }

    [Fact]
    public async Task DeleteProjectAsync_ReturnsEmptyList_WhenProjectNotFound()
    {
        await using var context = CreateContext();

        var paths = await new ProjectService(context).DeleteProjectAsync(Guid.NewGuid());

        paths.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchEmployeesAsync_ReturnsAll_WhenTermIsEmpty()
    {
        await using var context = CreateContext();
        context.Employees.AddRange(
            new Employee { FirstName = "Иван",  LastName = "Петров",  Mail = "ivan@test.com" },
            new Employee { FirstName = "Мария", LastName = "Иванова", Mail = "maria@test.com" }
        );
        await context.SaveChangesAsync();

        var result = await new ProjectService(context).SearchEmployeesAsync(string.Empty);

        result.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("Ива",   2)] // word part
    [InlineData("petrov", 0)] // case sensitive
    [InlineData("test.com", 2)] // by mail
    [InlineData("Мария", 1)] // by name
    public async Task SearchEmployeesAsync_FiltersByTerm(string term, int expectedCount)
    {
        await using var context = CreateContext();
        context.Employees.AddRange(
            new Employee { FirstName = "Иван",  LastName = "Петров",  Mail = "ivan@test.com" },
            new Employee { FirstName = "Мария", LastName = "Иванова", Mail = "maria@test.com" }
        );
        await context.SaveChangesAsync();

        var result = await new ProjectService(context).SearchEmployeesAsync(term);

        result.Should().HaveCount(expectedCount);
    }

    [Fact]
    public async Task AddEmployeesAsync_AddsNewEmployeesToProject()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        var emp1    = new Employee { FirstName = "А", LastName = "Б" };
        var emp2    = new Employee { FirstName = "В", LastName = "Г" };
        var project = MakeProject(manager);
        context.Employees.AddRange(manager, emp1, emp2);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        await new ProjectService(context).AddEmployeesAsync(project.Id, [emp1.Id, emp2.Id]);

        var updated = await context.Projects.Include(p => p.Employees).FirstAsync(p => p.Id == project.Id);
        updated.Employees.Should().Contain(e => e.Id == emp1.Id);
        updated.Employees.Should().Contain(e => e.Id == emp2.Id);
    }

    [Fact]
    public async Task AddEmployeesAsync_DoesNotDuplicateExistingEmployees()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        var emp     = new Employee { FirstName = "А", LastName = "Б" };
        var project = MakeProject(manager);
        project.Employees = [manager, emp];
        context.Employees.AddRange(manager, emp);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        await new ProjectService(context).AddEmployeesAsync(project.Id, [emp.Id, emp.Id]);

        var updated = await context.Projects.Include(p => p.Employees).FirstAsync(p => p.Id == project.Id);
        updated.Employees.Count(e => e.Id == emp.Id).Should().Be(1);
    }

    [Fact]
    public async Task RemoveEmployeeAsync_RemovesEmployeeFromProject()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        var emp     = new Employee { FirstName = "А", LastName = "Б" };
        var project = MakeProject(manager);
        project.Employees = [manager, emp];
        context.Employees.AddRange(manager, emp);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        await new ProjectService(context).RemoveEmployeeAsync(project.Id, emp.Id);

        var updated = await context.Projects.Include(p => p.Employees).FirstAsync(p => p.Id == project.Id);
        updated.Employees.Should().NotContain(e => e.Id == emp.Id);
    }

    [Fact]
    public async Task RemoveEmployeeAsync_DoesNotThrow_WhenEmployeeNotInProject()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        var project = MakeProject(manager);
        context.Employees.Add(manager);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var act = async () => await new ProjectService(context)
            .RemoveEmployeeAsync(project.Id, Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveDocumentPathAsync_RemovesPathFromProject()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        var project = MakeProject(manager);
        project.DocumentPaths = ["/uploads/keep.pdf", "/uploads/remove.pdf"];
        context.Employees.Add(manager);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        await new ProjectService(context).RemoveDocumentPathAsync(project.Id, "/uploads/remove.pdf");

        var updated = await context.Projects.FindAsync(project.Id);
        updated!.DocumentPaths.Should().NotContain("/uploads/remove.pdf");
        updated.DocumentPaths.Should().Contain("/uploads/keep.pdf");
    }

    [Fact]
    public async Task RemoveDocumentPathAsync_DoesNotThrow_WhenPathNotFound()
    {
        await using var context = CreateContext();
        var manager = new Employee { FirstName = "М", LastName = "П" };
        var project = MakeProject(manager);
        context.Employees.Add(manager);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var act = async () => await new ProjectService(context)
            .RemoveDocumentPathAsync(project.Id, "/uploads/nonexistent.pdf");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SetManagerAsync_ChangesManager()
    {
        await using var context = CreateContext();
        var oldManager = new Employee { FirstName = "Старый", LastName = "Менеджер" };
        var newManager = new Employee { FirstName = "Новый",  LastName = "Менеджер" };
        var project    = MakeProject(oldManager);
        project.Employees = [oldManager, newManager];
        context.Employees.AddRange(oldManager, newManager);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        await new ProjectService(context).SetManagerAsync(project.Id, newManager.Id);

        var updated = await context.Projects.FindAsync(project.Id);
        updated!.ManagerId.Should().Be(newManager.Id);
    }

    [Fact]
    public async Task SetManagerAsync_AddsNewManagerToEmployees_IfNotAlreadyMember()
    {
        await using var context = CreateContext();
        var manager     = new Employee { FirstName = "М",       LastName = "П" };
        var newManager  = new Employee { FirstName = "Новый",   LastName = "Менеджер" };
        var project     = MakeProject(manager);
        project.Employees = [manager];
        context.Employees.AddRange(manager, newManager);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        await new ProjectService(context).SetManagerAsync(project.Id, newManager.Id);

        var updated = await context.Projects.Include(p => p.Employees).FirstAsync(p => p.Id == project.Id);
        updated.Employees.Should().Contain(e => e.Id == newManager.Id);
    }

    [Fact]
    public async Task SetManagerAsync_ThrowsException_WhenProjectNotFound()
    {
        await using var context = CreateContext();

        var act = async () => await new ProjectService(context)
            .SetManagerAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<Exception>();
    }
}
