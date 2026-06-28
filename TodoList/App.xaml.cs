using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TodoList.Models;
using TodoList.Providers;
using TodoList.Services;
using TodoList.ViewModels;
using TodoList.Views;

namespace TodoList;

public partial class App : Application
{
    private readonly IHost _host;
    public static User? CurrentUser { get; set; }

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, builder) =>
            {
                builder.SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                string connectionString = context.Configuration.GetConnectionString("DefaultConnection") 
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
                
                // Sử dụng AddDbContextFactory cho ứng dụng WPF để tránh xung đột đa luồng khi các ViewModels tải dữ liệu đồng thời
                services.AddDbContextFactory<AppDbContext>(options =>
                    options.UseSqlServer(connectionString));

                // Đăng ký TaskService dưới dạng Transient
                services.AddSingleton<IGoogleCalendarSyncService, GoogleCalendarSyncService>();
                services.AddTransient<ITaskService, TaskService>();
                
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<CalendarViewModel>();
                services.AddTransient<CreateTaskViewModel>();
                services.AddTransient<EisenhowerViewModel>();
                services.AddTransient<LoginViewModel>();
                services.AddTransient<RegisterViewModel>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<MainShellView>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Tự động seed dữ liệu người dùng mặc định nếu DB chưa có user nào.
        await SeedDefaultUserAsync();

        var mainWindow = _host.Services.GetRequiredService<MainShellView>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    /// <summary>
    /// Tạo người dùng mặc định trong cơ sở dữ liệu nếu chưa tồn tại.
    /// Dùng tạm thời trong giai đoạn phát triển trước khi có màn hình đăng nhập.
    /// </summary>
    private async Task SeedDefaultUserAsync()
    {
        var factory = _host.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var db = factory.CreateDbContext();

        // Chỉ thêm user mặc định nếu bảng users đang trống
        var defaultUser = db.Users.FirstOrDefault(u => u.Email == "default@todolist.app");
        if (defaultUser == null)
        {
            db.Users.Add(new User
            {
                Id = App.DefaultUserId,
                Email = "default@todolist.app",
                PasswordHash = "nKX9ELDY6i2697do/rKMm9fbdhpRnJ52l/C4VRP9uK5L44tNlZzWTMp76SzEXGEP" // Mật khẩu: 123456
            });
            await db.SaveChangesAsync();
        }
        else if (defaultUser.PasswordHash == "dev_placeholder")
        {
            defaultUser.PasswordHash = "nKX9ELDY6i2697do/rKMm9fbdhpRnJ52l/C4VRP9uK5L44tNlZzWTMp76SzEXGEP"; // Cập nhật sang: 123456
            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// ID người dùng mặc định, dùng chung toàn ứng dụng trước khi có Authentication.
    /// </summary>
    public static readonly Guid DefaultUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }
}
