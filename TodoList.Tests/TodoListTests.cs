using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using TodoList.Helpers;
using TodoList.Models;
using TodoList.Providers;
using TodoList.Services;
using TodoList.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace TodoList.Tests
{
    public class TodoListTests : IDisposable
    {
        public TodoListTests()
        {
            App.CurrentUser = null;
        }

        public void Dispose()
        {
            App.CurrentUser = null;
        }

        private IDbContextFactory<AppDbContext> CreateInMemoryDbContextFactory()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var mockFactory = new Mock<IDbContextFactory<AppDbContext>>();
            mockFactory.Setup(f => f.CreateDbContextAsync(default))
                .ReturnsAsync(() => new AppDbContext(options));
            mockFactory.Setup(f => f.CreateDbContext())
                .Returns(() => new AppDbContext(options));

            return mockFactory.Object;
        }

        // =========================================================================
        //  TEST GROUP 1: PASSWORD HASHER (AUTH)
        // =========================================================================

        [Fact]
        public void HashPassword_NormalFlow_ShouldGenerateValidHash()
        {
            // Arrange
            string password = "SecretPassword123";

            // Act
            string hash = PasswordHasher.HashPassword(password);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
            Assert.True(PasswordHasher.VerifyPassword(password, hash));
        }

        [Fact]
        public void VerifyPassword_AlternativeFlow_IncorrectPassword_ShouldReturnFalse()
        {
            // Arrange
            string password = "SecretPassword123";
            string wrongPassword = "WrongPassword123";
            string hash = PasswordHasher.HashPassword(password);

            // Act
            bool result = PasswordHasher.VerifyPassword(wrongPassword, hash);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_ExceptionFlow_EmptyOrNullInputs_ShouldHandleSafelyAndReturnFalse()
        {
            // Arrange
            string password = "SecretPassword123";
            string hash = PasswordHasher.HashPassword(password);

            // Act
            bool resultNullPassword = PasswordHasher.VerifyPassword(null!, hash);
            bool resultNullHash = PasswordHasher.VerifyPassword(password, null!);

            // Assert
            Assert.False(resultNullPassword);
            Assert.False(resultNullHash);
        }

        // =========================================================================
        //  TEST GROUP 2: TASK SERVICE (CRUD)
        // =========================================================================

        [Fact]
        public async Task TaskCRUD_NormalFlow_ShouldCreateReadUpdateDelete()
        {
            // Arrange
            var factory = CreateInMemoryDbContextFactory();
            var mockSync = new Mock<IGoogleCalendarSyncService>();
            var service = new TaskService(factory, mockSync.Object);
            var userId = Guid.NewGuid();

            var task = new TaskItem
            {
                UserId = userId,
                Title = "Test Task",
                Status = "TODO"
            };

            // 1. Create (Add)
            var addedTask = await service.AddTaskAsync(task);
            Assert.NotEqual(Guid.Empty, addedTask.Id);

            // 2. Read (Get All)
            var tasks = await service.GetAllTasksAsync(userId);
            Assert.Single(tasks);
            Assert.Equal("Test Task", tasks[0].Title);

            // 3. Update
            addedTask.Title = "Updated Title";
            addedTask.Status = "DONE";
            await service.UpdateTaskAsync(addedTask);

            var updatedTasks = await service.GetAllTasksAsync(userId);
            Assert.Equal("Updated Title", updatedTasks[0].Title);
            Assert.Equal("DONE", updatedTasks[0].Status);

            // 4. Delete
            await service.DeleteTaskAsync(addedTask.Id);
            var emptyTasks = await service.GetAllTasksAsync(userId);
            Assert.Empty(emptyTasks);
        }

        // =========================================================================
        //  TEST GROUP 3: EISENHOWER VIEWMODEL
        // =========================================================================

        [Fact]
        public async Task Eisenhower_NormalFlow_ShouldGroupTasksCorrectly()
        {
            // Arrange
            var factory = CreateInMemoryDbContextFactory();
            var mockSync = new Mock<IGoogleCalendarSyncService>();
            var service = new TaskService(factory, mockSync.Object);
            var userId = Guid.NewGuid();

            // Seed tasks with different quadrants
            using (var db = factory.CreateDbContext())
            {
                db.Tasks.Add(new TaskItem { UserId = userId, Title = "Inbox Task", Quadrant = EisenhowerQuadrant.Inbox, Status = "TODO" });
                db.Tasks.Add(new TaskItem { UserId = userId, Title = "P1 Task", Quadrant = EisenhowerQuadrant.P1_Do, Status = "TODO" });
                db.Tasks.Add(new TaskItem { UserId = userId, Title = "P2 Task", Quadrant = EisenhowerQuadrant.P2_Schedule, Status = "TODO", DueDate = DateTime.Today });
                db.Tasks.Add(new TaskItem { UserId = userId, Title = "P3 Task", Quadrant = EisenhowerQuadrant.P3_Delegate, Status = "TODO" });
                db.Tasks.Add(new TaskItem { UserId = userId, Title = "P4 Task", Quadrant = EisenhowerQuadrant.P4_Eliminate, Status = "TODO" });
                await db.SaveChangesAsync();
            }

            // Set current user
            App.CurrentUser = new User { Id = userId, Email = "test@test.com" };

            var viewModel = new EisenhowerViewModel(service);

            // Act
            await viewModel.LoadDataAsync();

            // Assert
            Assert.Single(viewModel.P1Tasks);
            Assert.Single(viewModel.P2Tasks);
            Assert.Single(viewModel.P3Tasks);
            Assert.Single(viewModel.P4Tasks);

            Assert.Equal("P1 Task", viewModel.P1Tasks[0].Title);
        }

        [Fact]
        public async Task Eisenhower_Analytics_BalancedVsUnbalancedMatrix_AdviceLogic()
        {
            // Arrange
            var factory = CreateInMemoryDbContextFactory();
            var mockSync = new Mock<IGoogleCalendarSyncService>();
            var service = new TaskService(factory, mockSync.Object);
            var userId = Guid.NewGuid();

            // Case A: Unbalanced P1 Overload (Crisis)
            using (var db = factory.CreateDbContext())
            {
                // 3 P1 tasks, 1 P2 task
                db.Tasks.Add(new TaskItem { UserId = userId, Title = "P1 Task 1", Quadrant = EisenhowerQuadrant.P1_Do, Status = "TODO" });
                db.Tasks.Add(new TaskItem { UserId = userId, Title = "P1 Task 2", Quadrant = EisenhowerQuadrant.P1_Do, Status = "TODO" });
                db.Tasks.Add(new TaskItem { UserId = userId, Title = "P1 Task 3", Quadrant = EisenhowerQuadrant.P1_Do, Status = "TODO" });
                db.Tasks.Add(new TaskItem { UserId = userId, Title = "P2 Task", Quadrant = EisenhowerQuadrant.P2_Schedule, Status = "TODO", DueDate = DateTime.Today });
                await db.SaveChangesAsync();
            }

            App.CurrentUser = new User { Id = userId, Email = "test@test.com" };
            var viewModel = new EisenhowerViewModel(service);

            // Act
            await viewModel.LoadDataAsync();

            // Assert
            Assert.Contains("quá nhiều việc khủng hoảng ở ô P1", viewModel.AnalyticsAdvice);
            Assert.Equal("⚠️", viewModel.AnalyticsAdviceIcon);

            // Case B: Balanced P2 focus (> 50% P2)
            using (var db = factory.CreateDbContext())
            {
                // Clear and seed 4 P2 tasks, 1 P1 task
                var oldTasks = db.Tasks.ToList();
                db.Tasks.RemoveRange(oldTasks);
                await db.SaveChangesAsync();

                db.Tasks.Add(new TaskItem { UserId = userId, Title = "P2 Task 1", Quadrant = EisenhowerQuadrant.P2_Schedule, Status = "TODO", DueDate = DateTime.Today });
                db.Tasks.Add(new TaskItem { UserId = userId, Title = "P2 Task 2", Quadrant = EisenhowerQuadrant.P2_Schedule, Status = "TODO", DueDate = DateTime.Today });
                db.Tasks.Add(new TaskItem { UserId = userId, Title = "P2 Task 3", Quadrant = EisenhowerQuadrant.P2_Schedule, Status = "TODO", DueDate = DateTime.Today });
                db.Tasks.Add(new TaskItem { UserId = userId, Title = "P2 Task 4", Quadrant = EisenhowerQuadrant.P2_Schedule, Status = "TODO", DueDate = DateTime.Today });
                db.Tasks.Add(new TaskItem { UserId = userId, Title = "P1 Task", Quadrant = EisenhowerQuadrant.P1_Do, Status = "TODO" });
                await db.SaveChangesAsync();
            }

            // Act
            await viewModel.LoadDataAsync();

            // Assert
            Assert.Contains("phần lớn thời gian cho ô P2", viewModel.AnalyticsAdvice);
            Assert.Equal("🏆", viewModel.AnalyticsAdviceIcon);
        }

        [Fact]
        public async Task Eisenhower_P2Suggestions_TriggerSuggestionsCorrectly()
        {
            // Arrange
            var factory = CreateInMemoryDbContextFactory();
            var mockSync = new Mock<IGoogleCalendarSyncService>();
            var service = new TaskService(factory, mockSync.Object);
            var userId = Guid.NewGuid();

            // Seed P2 task with null DueDate
            using (var db = factory.CreateDbContext())
            {
                db.Tasks.Add(new TaskItem { UserId = userId, Title = "Unscheduled P2 Task", Quadrant = EisenhowerQuadrant.P2_Schedule, Status = "TODO", DueDate = null });
                await db.SaveChangesAsync();
            }

            App.CurrentUser = new User { Id = userId, Email = "test@test.com" };
            var viewModel = new EisenhowerViewModel(service);

            // Act
            await viewModel.LoadDataAsync();

            // Assert
            Assert.True(viewModel.HasP2Suggestion);
            Assert.NotNull(viewModel.P2SuggestedTask);
            Assert.Equal("Unscheduled P2 Task", viewModel.P2SuggestedTask.Title);

            // Act: Accept Suggestion
            await viewModel.AcceptP2SuggestionCommand.ExecuteAsync(null);

            // Assert: Should schedule for tomorrow
            Assert.False(viewModel.HasP2Suggestion);
            var updatedTasks = await service.GetAllTasksAsync(userId);
            var suggestTask = updatedTasks.FirstOrDefault(t => t.Title == "Unscheduled P2 Task");
            Assert.NotNull(suggestTask);
            Assert.NotNull(suggestTask.DueDate);
            Assert.Equal(DateTime.Today.AddDays(1).Date, suggestTask.DueDate.Value.Date);
        }

        [Fact]
        public async Task TabSwitching_NormalFlow_ShouldUseCacheAndInvalidateOnDataChanged()
        {
            // Arrange
            var factory = CreateInMemoryDbContextFactory();
            var mockSync = new Mock<IGoogleCalendarSyncService>();
            var mockTaskService = new Mock<ITaskService>();
            var userId = Guid.NewGuid();
            App.CurrentUser = new User { Id = userId, Email = "test@test.com" };

            mockTaskService.Setup(s => s.GetAllTasksAsync(userId))
                .ReturnsAsync(new System.Collections.Generic.List<TaskItem>
                {
                    new TaskItem { UserId = userId, Title = "Task 1", Quadrant = EisenhowerQuadrant.P1_Do, Status = "TODO" }
                });

            var dashboardVM = new DashboardViewModel(mockTaskService.Object, mockSync.Object, null!);
            var calendarVM = new CalendarViewModel(mockTaskService.Object);
            var eisenhowerVM = new EisenhowerViewModel(mockTaskService.Object);
            var createTaskVM = new CreateTaskViewModel(mockTaskService.Object);
            
            var mockServiceProvider = new Mock<IServiceProvider>();
            var loginVM = new LoginViewModel(factory);
            var registerVM = new RegisterViewModel(factory);
            mockServiceProvider.Setup(s => s.GetService(typeof(LoginViewModel))).Returns(loginVM);
            mockServiceProvider.Setup(s => s.GetService(typeof(RegisterViewModel))).Returns(registerVM);

            var mindSandboxVM = new MindSandboxViewModel(mockTaskService.Object);
            var taDaListVM = new TaDaListViewModel(mockTaskService.Object);

            var mainVM = new MainViewModel(
                dashboardVM,
                calendarVM,
                createTaskVM,
                eisenhowerVM,
                mindSandboxVM,
                taDaListVM,
                mockServiceProvider.Object,
                mockSync.Object
            );

            // Act 1: Initial navigation calls LoadTasksAsync (first load)
            mainVM.NavigateToTodayCommand.Execute(null);
            
            // Act 2: Navigate again - should NOT call LoadTasksAsync because forceReload is false and data is loaded
            mainVM.NavigateToTodayCommand.Execute(null);
            
            // Assert 1: Service should have been queried exactly once
            mockTaskService.Verify(s => s.GetAllTasksAsync(userId), Times.Once);

            // Act 3: Broadcast TaskDataChangedMessage to simulate mutation
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new TodoList.Messages.TaskDataChangedMessage());

            // Act 4: Navigate to Today again - since cache was invalidated, it should reload!
            mainVM.NavigateToTodayCommand.Execute(null);

            // Assert 2: Service should have been queried exactly 4 times (first time + 1 for dashboard + 1 for mindsandbox + 1 for tadalist)
            mockTaskService.Verify(s => s.GetAllTasksAsync(userId), Times.Exactly(4));
        }
    }
}
