using FluentAssertions;
using MegaProject.Data;
using MegaProject.Domain.Models;
using MegaProject.Domain.Models.Exceptions;
using MegaProject.Services;
using Microsoft.EntityFrameworkCore;
using TaskStatus = MegaProject.Domain.Models.Enums.TaskStatus;

namespace MegaProject.Tests.Services;

public class ProjectTaskServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static (Employee manager, Project project) SeedProject(ApplicationDbContext context, string projectName = "Проект")
    {
        var manager = new Employee { FirstName = "Менеджер", LastName = "Проектов" };
        var project = new Project
        {
            Name         = projectName,
            ClientName   = "Клиент",
            ExecutorName = "Исполнитель",
            Manager      = manager
        };
        context.Employees.Add(manager);
        context.Projects.Add(project);
        return (manager, project);
    }

    [Fact]
    public async Task GetByProjectAsync_ReturnsEmptyList_WhenProjectHasNoTasks()
    {
        await using var context = CreateContext();
        var (_, project) = SeedProject(context);
        await context.SaveChangesAsync();

        var result = await new ProjectTaskService(context).GetByProjectAsync(project.Id);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByProjectAsync_ReturnsAllTasks_WhenStatusIsNull()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        context.ProjectsTasks.AddRange(
            new ProjectTask { Name = "Задача 1", ProjectId = project.Id, AuthorId = manager.Id, Status = TaskStatus.ToDo },
            new ProjectTask { Name = "Задача 2", ProjectId = project.Id, AuthorId = manager.Id, Status = TaskStatus.InProgress },
            new ProjectTask { Name = "Задача 3", ProjectId = project.Id, AuthorId = manager.Id, Status = TaskStatus.Done }
        );
        await context.SaveChangesAsync();

        var result = await new ProjectTaskService(context).GetByProjectAsync(project.Id, status: null);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByProjectAsync_FiltersTasksByStatus()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        context.ProjectsTasks.AddRange(
            new ProjectTask { Name = "ToDo 1",      ProjectId = project.Id, AuthorId = manager.Id, Status = TaskStatus.ToDo },
            new ProjectTask { Name = "ToDo 2",      ProjectId = project.Id, AuthorId = manager.Id, Status = TaskStatus.ToDo },
            new ProjectTask { Name = "InProgress 1", ProjectId = project.Id, AuthorId = manager.Id, Status = TaskStatus.InProgress }
        );
        await context.SaveChangesAsync();

        var result = await new ProjectTaskService(context).GetByProjectAsync(project.Id, TaskStatus.ToDo);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.Status == TaskStatus.ToDo);
    }

    [Fact]
    public async Task GetByProjectAsync_ReturnsEmpty_WhenNoTasksMatchStatus()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        context.ProjectsTasks.Add(
            new ProjectTask { Name = "Задача", ProjectId = project.Id, AuthorId = manager.Id, Status = TaskStatus.ToDo }
        );
        await context.SaveChangesAsync();

        var result = await new ProjectTaskService(context).GetByProjectAsync(project.Id, TaskStatus.Done);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByProjectAsync_ReturnsOnlyTasksOfSpecificProject()
    {
        await using var context = CreateContext();
        var (manager, project1) = SeedProject(context, "Проект 1");
        var (_, project2) = SeedProject(context, "Проект 2");
        context.ProjectsTasks.AddRange(
            new ProjectTask { Name = "Задача проекта 1", ProjectId = project1.Id, AuthorId = manager.Id },
            new ProjectTask { Name = "Задача проекта 2", ProjectId = project2.Id, AuthorId = manager.Id }
        );
        await context.SaveChangesAsync();

        var result = await new ProjectTaskService(context).GetByProjectAsync(project1.Id);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Задача проекта 1");
    }

    [Fact]
    public async Task GetByProjectAsync_OrdersByPriorityAscending()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        context.ProjectsTasks.AddRange(
            new ProjectTask { Name = "Низкий",   ProjectId = project.Id, AuthorId = manager.Id, Priority = 10 },
            new ProjectTask { Name = "Высокий",  ProjectId = project.Id, AuthorId = manager.Id, Priority = 1 },
            new ProjectTask { Name = "Средний",  ProjectId = project.Id, AuthorId = manager.Id, Priority = 5 }
        );
        await context.SaveChangesAsync();

        var result = await new ProjectTaskService(context).GetByProjectAsync(project.Id);

        result.Select(t => t.Priority).Should().BeInAscendingOrder();
        result[0].Name.Should().Be("Высокий");
    }

    [Fact]
    public async Task GetByProjectAsync_IncludesAuthorAndWorker()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        var worker = new Employee { FirstName = "Работник", LastName = "Задачи" };
        context.Employees.Add(worker);
        context.ProjectsTasks.Add(new ProjectTask
        {
            Name     = "Задача",
            ProjectId = project.Id,
            AuthorId = manager.Id,
            WorkerId = worker.Id
        });
        await context.SaveChangesAsync();

        var result = await new ProjectTaskService(context).GetByProjectAsync(project.Id);

        result[0].Author.Should().NotBeNull();
        result[0].Author.FirstName.Should().Be("Менеджер");
        result[0].Worker.Should().NotBeNull();
        result[0].Worker!.FirstName.Should().Be("Работник");
    }

    [Fact]
    public async Task GetByProjectAsync_WorkerIsNull_WhenNotAssigned()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        context.ProjectsTasks.Add(new ProjectTask
        {
            Name      = "Задача без исполнителя",
            ProjectId = project.Id,
            AuthorId  = manager.Id,
            WorkerId  = null
        });
        await context.SaveChangesAsync();

        var result = await new ProjectTaskService(context).GetByProjectAsync(project.Id);

        result[0].Worker.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTask_WhenExists()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        var task = new ProjectTask { Name = "Задача", ProjectId = project.Id, AuthorId = manager.Id };
        context.ProjectsTasks.Add(task);
        await context.SaveChangesAsync();

        var result = await new ProjectTaskService(context).GetByIdAsync(task.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(task.Id);
        result.Name.Should().Be("Задача");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        await using var context = CreateContext();

        var result = await new ProjectTaskService(context).GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_IncludesAuthorAndWorker()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        var worker = new Employee { FirstName = "Работник", LastName = "Задачи" };
        context.Employees.Add(worker);
        var task = new ProjectTask
        {
            Name      = "Задача",
            ProjectId = project.Id,
            AuthorId  = manager.Id,
            WorkerId  = worker.Id
        };
        context.ProjectsTasks.Add(task);
        await context.SaveChangesAsync();

        var result = await new ProjectTaskService(context).GetByIdAsync(task.Id);

        result!.Author.Should().NotBeNull();
        result.Worker.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_SavesAllFields()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        var worker = new Employee { FirstName = "Работник", LastName = "Задачи" };
        context.Employees.Add(worker);
        await context.SaveChangesAsync();

        var task = new ProjectTask
        {
            Name      = "Новая задача",
            Comment   = "Комментарий",
            Status    = TaskStatus.InProgress,
            Priority  = 3,
            ProjectId = project.Id,
            AuthorId  = manager.Id,
            WorkerId  = worker.Id
        };
        await new ProjectTaskService(context).CreateAsync(task);

        var saved = await context.ProjectsTasks.FindAsync(task.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Новая задача");
        saved.Comment.Should().Be("Комментарий");
        saved.Status.Should().Be(TaskStatus.InProgress);
        saved.Priority.Should().Be(3);
        saved.WorkerId.Should().Be(worker.Id);
    }

    [Fact]
    public async Task CreateAsync_WorksWithoutWorker()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        await context.SaveChangesAsync();

        var task = new ProjectTask
        {
            Name      = "Задача без исполнителя",
            ProjectId = project.Id,
            AuthorId  = manager.Id,
            WorkerId  = null
        };
        await new ProjectTaskService(context).CreateAsync(task);

        var saved = await context.ProjectsTasks.FindAsync(task.Id);
        saved!.WorkerId.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_DefaultStatusIsToDo()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        await context.SaveChangesAsync();

        var task = new ProjectTask { Name = "Задача", ProjectId = project.Id, AuthorId = manager.Id };
        await new ProjectTaskService(context).CreateAsync(task);

        var saved = await context.ProjectsTasks.FindAsync(task.Id);
        saved!.Status.Should().Be(TaskStatus.ToDo);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAllFields()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        var worker = new Employee { FirstName = "Работник", LastName = "Задачи" };
        context.Employees.Add(worker);
        var task = new ProjectTask
        {
            Name     = "Старое название",
            Comment  = "Старый комментарий",
            Status   = TaskStatus.ToDo,
            Priority = 1,
            ProjectId = project.Id,
            AuthorId  = manager.Id
        };
        context.ProjectsTasks.Add(task);
        await context.SaveChangesAsync();

        await new ProjectTaskService(context).UpdateAsync(new ProjectTask
        {
            Id        = task.Id,
            Name      = "Новое название",
            Comment   = "Новый комментарий",
            Status    = TaskStatus.InProgress,
            Priority  = 5,
            ProjectId = project.Id,
            AuthorId  = manager.Id,
            WorkerId  = worker.Id
        });

        var updated = await context.ProjectsTasks.FindAsync(task.Id);
        updated!.Name.Should().Be("Новое название");
        updated.Comment.Should().Be("Новый комментарий");
        updated.Status.Should().Be(TaskStatus.InProgress);
        updated.Priority.Should().Be(5);
        updated.WorkerId.Should().Be(worker.Id);
    }

    [Fact]
    public async Task UpdateAsync_CanClearWorker()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        var worker = new Employee { FirstName = "Работник", LastName = "Задачи" };
        context.Employees.Add(worker);
        var task = new ProjectTask
        {
            Name = "Задача", ProjectId = project.Id, AuthorId = manager.Id, WorkerId = worker.Id
        };
        context.ProjectsTasks.Add(task);
        await context.SaveChangesAsync();

        await new ProjectTaskService(context).UpdateAsync(new ProjectTask
        {
            Id        = task.Id,
            Name      = task.Name,
            ProjectId = project.Id,
            AuthorId  = manager.Id,
            WorkerId  = null
        });

        var updated = await context.ProjectsTasks.FindAsync(task.Id);
        updated!.WorkerId.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_DoesNotChangeId()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        var task = new ProjectTask { Name = "Задача", ProjectId = project.Id, AuthorId = manager.Id };
        context.ProjectsTasks.Add(task);
        await context.SaveChangesAsync();
        var originalId = task.Id;

        await new ProjectTaskService(context).UpdateAsync(new ProjectTask
        {
            Id = originalId, Name = "Новое", ProjectId = project.Id, AuthorId = manager.Id
        });

        context.ProjectsTasks.Should().HaveCount(1);
        context.ProjectsTasks.First().Id.Should().Be(originalId);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenTaskNotFound()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        await context.SaveChangesAsync();

        var act = async () => await new ProjectTaskService(context).UpdateAsync(new ProjectTask
        {
            Id = Guid.NewGuid(), Name = "X", ProjectId = project.Id, AuthorId = manager.Id
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_RemovesTask()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        var task = new ProjectTask { Name = "Задача", ProjectId = project.Id, AuthorId = manager.Id };
        context.ProjectsTasks.Add(task);
        await context.SaveChangesAsync();

        await new ProjectTaskService(context).DeleteAsync(task.Id);

        context.ProjectsTasks.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_OnlyRemovesTargetTask_LeavesOthersIntact()
    {
        await using var context = CreateContext();
        var (manager, project) = SeedProject(context);
        var task1 = new ProjectTask { Name = "Задача 1", ProjectId = project.Id, AuthorId = manager.Id };
        var task2 = new ProjectTask { Name = "Задача 2", ProjectId = project.Id, AuthorId = manager.Id };
        context.ProjectsTasks.AddRange(task1, task2);
        await context.SaveChangesAsync();

        await new ProjectTaskService(context).DeleteAsync(task1.Id);

        context.ProjectsTasks.Should().HaveCount(1);
        context.ProjectsTasks.First().Id.Should().Be(task2.Id);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenTaskNotFound()
    {
        await using var context = CreateContext();

        var act = async () => await new ProjectTaskService(context).DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
