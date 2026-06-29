using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using CommunityToolkit.Mvvm.Messaging;
using TodoList.Helpers;
using TodoList.Messages;
using TodoList.Models;
using TodoList.Providers;
using TodoList.Services;
using TodoList.ViewModels;
using Xunit;

namespace TodoList.Tests
{
    public class AuthViewModelTests : IDisposable
    {
        public AuthViewModelTests()
        {
            // Reset state
            App.CurrentUser = null;
        }

        public void Dispose()
        {
            App.CurrentUser = null;
            WeakReferenceMessenger.Default.UnregisterAll(this);
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
        //  LOGIN VIEWMODEL TESTS
        // =========================================================================

        [Fact]
        public async Task Login_NormalFlow_ShouldSucceedAndSendMessage()
        {
            // Arrange
            var factory = CreateInMemoryDbContextFactory();
            var email = "loginuser@test.com";
            var password = "Password123";
            var hash = PasswordHasher.HashPassword(password);
            
            using (var db = factory.CreateDbContext())
            {
                db.Users.Add(new User { Email = email, PasswordHash = hash });
                await db.SaveChangesAsync();
            }

            var viewModel = new LoginViewModel(factory)
            {
                Email = email,
                Password = password
            };

            User? receivedUser = null;
            WeakReferenceMessenger.Default.Register<UserStatusChangedMessage>(this, (r, m) =>
            {
                receivedUser = m.Value;
            });

            // Act
            await viewModel.LoginCommand.ExecuteAsync(null);

            // Assert
            Assert.Empty(viewModel.ErrorMessage);
            Assert.NotNull(receivedUser);
            Assert.Equal(email, receivedUser.Email);
            Assert.Equal(App.CurrentUser, receivedUser);
        }

        [Fact]
        public async Task Login_AlternativeFlow_IncorrectPassword_ShouldSetErrorMessage()
        {
            // Arrange
            var factory = CreateInMemoryDbContextFactory();
            var email = "loginuser2@test.com";
            var password = "Password123";
            var hash = PasswordHasher.HashPassword(password);
            
            using (var db = factory.CreateDbContext())
            {
                db.Users.Add(new User { Email = email, PasswordHash = hash });
                await db.SaveChangesAsync();
            }

            var viewModel = new LoginViewModel(factory)
            {
                Email = email,
                Password = "WrongPassword"
            };

            // Act
            await viewModel.LoginCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Email hoặc Mật khẩu không chính xác.", viewModel.ErrorMessage);
            Assert.Null(App.CurrentUser);
        }

        [Fact]
        public async Task Login_ExceptionFlow_EmptyInputs_ShouldSetValidationError()
        {
            // Arrange
            var factory = CreateInMemoryDbContextFactory();
            var viewModel = new LoginViewModel(factory)
            {
                Email = "",
                Password = ""
            };

            // Act
            await viewModel.LoginCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Vui lòng nhập đầy đủ Email và Mật khẩu.", viewModel.ErrorMessage);
        }

        // =========================================================================
        //  REGISTER VIEWMODEL TESTS
        // =========================================================================

        [Fact]
        public async Task Register_NormalFlow_ShouldSucceedCreateUserAndSendMessage()
        {
            // Arrange
            var factory = CreateInMemoryDbContextFactory();
            var email = "newuser@test.com";
            var password = "NewPassword123";

            var viewModel = new RegisterViewModel(factory)
            {
                Email = email,
                Password = password,
                ConfirmPassword = password
            };

            User? receivedUser = null;
            WeakReferenceMessenger.Default.Register<UserStatusChangedMessage>(this, (r, m) =>
            {
                receivedUser = m.Value;
            });

            // Act
            await viewModel.RegisterCommand.ExecuteAsync(null);

            // Assert
            Assert.Empty(viewModel.ErrorMessage);
            Assert.NotNull(receivedUser);
            Assert.Equal(email, receivedUser.Email);
            
            // Verify in DB
            using (var db = factory.CreateDbContext())
            {
                var userInDb = db.Users.FirstOrDefault(u => u.Email == email);
                Assert.NotNull(userInDb);
                Assert.True(PasswordHasher.VerifyPassword(password, userInDb.PasswordHash));
            }
        }

        [Fact]
        public async Task Register_AlternativeFlow_DuplicateEmail_ShouldSetErrorMessage()
        {
            // Arrange
            var factory = CreateInMemoryDbContextFactory();
            var email = "existing@test.com";
            using (var db = factory.CreateDbContext())
            {
                db.Users.Add(new User { Email = email, PasswordHash = "somehash" });
                await db.SaveChangesAsync();
            }

            var viewModel = new RegisterViewModel(factory)
            {
                Email = email,
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            // Act
            await viewModel.RegisterCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Email này đã được sử dụng.", viewModel.ErrorMessage);
        }

        [Fact]
        public async Task Register_AlternativeFlow_PasswordMismatch_ShouldSetErrorMessage()
        {
            // Arrange
            var factory = CreateInMemoryDbContextFactory();
            var viewModel = new RegisterViewModel(factory)
            {
                Email = "mismatch@test.com",
                Password = "Password123",
                ConfirmPassword = "DifferentPassword"
            };

            // Act
            await viewModel.RegisterCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Xác nhận mật khẩu không trùng khớp.", viewModel.ErrorMessage);
        }

        [Fact]
        public async Task Register_ExceptionFlow_InvalidEmailFormat_ShouldSetErrorMessage()
        {
            // Arrange
            var factory = CreateInMemoryDbContextFactory();
            var viewModel = new RegisterViewModel(factory)
            {
                Email = "invalid-email-format",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            // Act
            await viewModel.RegisterCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Định dạng Email không hợp lệ.", viewModel.ErrorMessage);
        }
    }
}
