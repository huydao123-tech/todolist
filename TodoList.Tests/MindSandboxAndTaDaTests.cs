using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using TodoList.Models;
using TodoList.Services;
using TodoList.ViewModels;

namespace TodoList.Tests;

public class MindSandboxAndTaDaTests
{
    private Mock<ITaskService> _taskServiceMock;

    public MindSandboxAndTaDaTests()
    {
        _taskServiceMock = new Mock<ITaskService>();
        App.CurrentUser = new User { Id = Guid.NewGuid(), Email = "test@test.com" };
    }

    [Fact]
    public async Task MindSandbox_LoadIdeasAsync_ShouldOnlyLoadInboxTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Title = "Idea 1", Quadrant = EisenhowerQuadrant.Inbox, Status = "TODO" },
            new TaskItem { Id = Guid.NewGuid(), Title = "Task P1", Quadrant = EisenhowerQuadrant.P1_Do, Status = "TODO" },
            new TaskItem { Id = Guid.NewGuid(), Title = "Idea 2", Quadrant = EisenhowerQuadrant.Inbox, Status = "TODO" },
            new TaskItem { Id = Guid.NewGuid(), Title = "Deleted Idea", Quadrant = EisenhowerQuadrant.Inbox, DeletedAt = DateTime.UtcNow, Status = "TODO" }
        };

        _taskServiceMock.Setup(s => s.GetAllTasksAsync(It.IsAny<Guid>())).ReturnsAsync(tasks);
        var viewModel = new MindSandboxViewModel(_taskServiceMock.Object);

        // Act
        await viewModel.LoadIdeasAsync();

        // Assert
        Assert.Equal(2, viewModel.Ideas.Count);
        Assert.Contains(viewModel.Ideas, i => i.Title == "Idea 1");
        Assert.Contains(viewModel.Ideas, i => i.Title == "Idea 2");
    }

    [Fact]
    public async Task TaDaList_LoadDataAsync_ShouldOnlyLoadCompletedTasksToday()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Title = "Done Today 1", Status = "DONE", UpdatedAt = today.AddHours(2) },
            new TaskItem { Id = Guid.NewGuid(), Title = "Done Today 2", Status = "DONE", UpdatedAt = today.AddHours(5) },
            new TaskItem { Id = Guid.NewGuid(), Title = "Done Yesterday", Status = "DONE", UpdatedAt = today.AddDays(-1) },
            new TaskItem { Id = Guid.NewGuid(), Title = "Todo Today", Status = "TODO", UpdatedAt = today }
        };

        _taskServiceMock.Setup(s => s.GetAllTasksAsync(It.IsAny<Guid>())).ReturnsAsync(tasks);
        var viewModel = new TaDaListViewModel(_taskServiceMock.Object);

        // Act
        await viewModel.LoadDataAsync();

        // Assert
        Assert.Equal(2, viewModel.CompletedTasks.Count);
        Assert.Equal(2, viewModel.CompletedCount);
        Assert.Contains(viewModel.CompletedTasks, t => t.Title == "Done Today 1");
        Assert.Contains(viewModel.CompletedTasks, t => t.Title == "Done Today 2");
        Assert.False(string.IsNullOrEmpty(viewModel.EncouragingMessage));
    }
}
