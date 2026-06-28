using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TodoList.Helpers;
using TodoList.Messages;
using TodoList.Models;
using TodoList.Providers;

namespace TodoList.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public LoginViewModel(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Vui lòng nhập đầy đủ Email và Mật khẩu.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null || !PasswordHasher.VerifyPassword(Password, user.PasswordHash))
            {
                ErrorMessage = "Email hoặc Mật khẩu không chính xác.";
                IsBusy = false;
                return;
            }

            // Đăng nhập thành công
            App.CurrentUser = user;
            WeakReferenceMessenger.Default.Send(new UserStatusChangedMessage(user));
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
    private void NavigateToRegister()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Register"));
    }
}
