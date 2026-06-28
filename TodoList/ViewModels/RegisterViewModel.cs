using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TodoList.Helpers;
using TodoList.Messages;
using TodoList.Models;
using TodoList.Providers;

namespace TodoList.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public RegisterViewModel(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ErrorMessage = "Vui lòng điền đầy đủ các thông tin.";
            return;
        }

        if (!IsValidEmail(Email))
        {
            ErrorMessage = "Định dạng Email không hợp lệ.";
            return;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "Mật khẩu phải chứa ít nhất 6 ký tự.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Xác nhận mật khẩu không trùng khớp.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var exists = await context.Users.AnyAsync(u => u.Email == Email);

            if (exists)
            {
                ErrorMessage = "Email này đã được sử dụng.";
                IsBusy = false;
                return;
            }

            var newUser = new User
            {
                Email = Email,
                PasswordHash = PasswordHasher.HashPassword(Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Users.Add(newUser);
            await context.SaveChangesAsync();

            // Đăng ký và đăng nhập luôn
            App.CurrentUser = newUser;
            WeakReferenceMessenger.Default.Send(new UserStatusChangedMessage(newUser));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi kết nối cơ sở dữ liệu: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void NavigateToLogin()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Login"));
    }

    private bool IsValidEmail(string email)
    {
        var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
}
