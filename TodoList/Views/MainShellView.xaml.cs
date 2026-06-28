using System.Windows;
using TodoList.ViewModels;

namespace TodoList.Views;

public partial class MainShellView : Window
{
    public MainShellView(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
