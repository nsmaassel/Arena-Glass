#if WINDOWS
using ArenaGlass.ViewModels;
using System.Windows;
using System.Windows.Media;

namespace ArenaGlass;

/// <summary>
/// Main window for ArenaGlass WPF application
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        
        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        // Subscribe to property changes to update UI elements that need manual binding
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        
        // Handle window closing
        Closing += OnWindowClosing;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsMonitoring))
        {
            UpdateMonitoringStatus();
        }
    }

    private void UpdateMonitoringStatus()
    {
        MonitoringStatus.Text = _viewModel.IsMonitoring ? "Active" : "Stopped";
        MonitoringStatus.Foreground = _viewModel.IsMonitoring 
            ? new SolidColorBrush(Color.FromRgb(76, 175, 80))   // Green
            : new SolidColorBrush(Color.FromRgb(244, 67, 54));  // Red
    }

    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _viewModel.Cleanup();
    }
}
#endif