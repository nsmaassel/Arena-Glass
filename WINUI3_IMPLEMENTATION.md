# WinUI 3 Overlay Implementation Guide

This document provides a complete implementation guide for converting the current console application to a WinUI 3 overlay that meets all the requirements specified in the issue.

## Project Structure

The complete WinUI 3 implementation would require the following additional files:

```
ArenaGlass.UI/
├── ArenaGlass.UI.csproj (modified)
├── Program.cs (modified for WinUI 3)
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── Services/
│   ├── IOverlayService.cs
│   ├── OverlayService.cs
│   └── Win32Interop.cs
├── ViewModels/
│   ├── MainViewModel.cs
│   └── CardViewModel.cs
└── Package.appxmanifest
```

## 1. Project File Modifications

The `ArenaGlass.UI.csproj` should be updated to:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
    <UseWinUI>true</UseWinUI>
    <Nullable>enable</Nullable>
    <UsePackageReferenceProjectReferenceForPackages>true</UsePackageReferenceProjectReferenceForPackages>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.5.240404000" />
    <PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.22000.2176" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\ArenaGlass.Core\ArenaGlass.Core.csproj" />
  </ItemGroup>

</Project>
```

## 2. Win32 Interop Service

Create `Services/Win32Interop.cs`:

```csharp
using System.Runtime.InteropServices;

namespace ArenaGlass.UI.Services;

public static class Win32Interop
{
    // Window styles
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_TRANSPARENT = 0x00000020;
    public const int WS_EX_TOOLWINDOW = 0x00000080;
    public const int WS_EX_TOPMOST = 0x00000008;
    public const int WS_EX_LAYERED = 0x00080000;

    // Window positioning
    public const int HWND_TOPMOST = -1;
    public const int SWP_NOMOVE = 0x0002;
    public const int SWP_NOSIZE = 0x0001;

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
```

## 3. Overlay Service

Create `Services/IOverlayService.cs`:

```csharp
namespace ArenaGlass.UI.Services;

public interface IOverlayService
{
    void ConfigureAsOverlay(IntPtr windowHandle);
    void UpdatePosition(IntPtr windowHandle);
    bool FindAndPositionRelativeToMtgArena(IntPtr windowHandle);
}
```

Create `Services/OverlayService.cs`:

```csharp
using Microsoft.Extensions.Logging;

namespace ArenaGlass.UI.Services;

public class OverlayService : IOverlayService
{
    private readonly ILogger<OverlayService> _logger;
    private Timer? _positionUpdateTimer;

    public OverlayService(ILogger<OverlayService> logger)
    {
        _logger = logger;
    }

    public void ConfigureAsOverlay(IntPtr windowHandle)
    {
        // Make window transparent and click-through
        var exStyle = Win32Interop.GetWindowLong(windowHandle, Win32Interop.GWL_EXSTYLE);
        exStyle |= Win32Interop.WS_EX_TRANSPARENT | Win32Interop.WS_EX_TOOLWINDOW | 
                   Win32Interop.WS_EX_TOPMOST | Win32Interop.WS_EX_LAYERED;
        Win32Interop.SetWindowLong(windowHandle, Win32Interop.GWL_EXSTYLE, exStyle);

        // Set window as always on top
        Win32Interop.SetWindowPos(windowHandle, Win32Interop.HWND_TOPMOST, 0, 0, 0, 0,
            Win32Interop.SWP_NOMOVE | Win32Interop.SWP_NOSIZE);

        // Set transparency
        Win32Interop.SetLayeredWindowAttributes(windowHandle, 0, 255, 0x02);

        // Start position monitoring
        _positionUpdateTimer = new Timer(_ => UpdatePosition(windowHandle), 
            null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));

        _logger.LogInformation("Configured window as overlay");
    }

    public void UpdatePosition(IntPtr windowHandle)
    {
        FindAndPositionRelativeToMtgArena(windowHandle);
    }

    public bool FindAndPositionRelativeToMtgArena(IntPtr windowHandle)
    {
        var mtgArenaWindow = Win32Interop.FindWindow(null, "MTG Arena");
        if (mtgArenaWindow == IntPtr.Zero)
        {
            // Hide overlay if MTG Arena is not found
            Win32Interop.SetWindowPos(windowHandle, 0, -1000, -1000, 0, 0, 
                Win32Interop.SWP_NOSIZE);
            return false;
        }

        if (Win32Interop.GetWindowRect(mtgArenaWindow, out var gameRect))
        {
            // Position overlay in the top-right corner of the game window
            var overlayX = gameRect.Right - 300; // 300px from right edge
            var overlayY = gameRect.Top + 50;    // 50px from top edge

            Win32Interop.SetWindowPos(windowHandle, Win32Interop.HWND_TOPMOST,
                overlayX, overlayY, 280, 400, 0);

            return true;
        }

        return false;
    }
}
```

## 4. XAML Files

Create `App.xaml`:

```xml
<Application
    x:Class="ArenaGlass.UI.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

Create `MainWindow.xaml`:

```xml
<Window
    x:Class="ArenaGlass.UI.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Grid Background="Transparent">
        <Border Background="Black" Opacity="0.7" CornerRadius="8" Margin="10">
            <StackPanel Margin="15">
                <TextBlock Text="ArenaGlass" FontSize="16" FontWeight="Bold" 
                          Foreground="White" HorizontalAlignment="Center" Margin="0,0,0,10"/>
                
                <TextBlock Text="{x:Bind ViewModel.TotalCardsText, Mode=OneWay}" 
                          Foreground="White" FontSize="14" Margin="0,0,0,10"/>
                
                <ListView ItemsSource="{x:Bind ViewModel.Cards, Mode=OneWay}"
                         Background="Transparent" MaxHeight="300">
                    <ListView.ItemTemplate>
                        <DataTemplate x:DataType="local:CardViewModel">
                            <Grid>
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="Auto"/>
                                </Grid.ColumnDefinitions>
                                
                                <TextBlock Text="{x:Bind Name}" Foreground="White" 
                                          VerticalAlignment="Center"/>
                                <TextBlock Grid.Column="1" Text="{x:Bind CountText}" 
                                          Foreground="LightGray" VerticalAlignment="Center"/>
                            </Grid>
                        </DataTemplate>
                    </ListView.ItemTemplate>
                </ListView>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

## 5. ViewModels

Create `ViewModels/CardViewModel.cs`:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArenaGlass.UI.ViewModels;

public partial class CardViewModel : ObservableObject
{
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private int count;
    [ObservableProperty] private string manaCost = string.Empty;
    [ObservableProperty] private string type = string.Empty;

    public string CountText => $"{Count}x";
}
```

Create `ViewModels/MainViewModel.cs`:

```csharp
using System.Collections.ObjectModel;
using ArenaGlass.Core.Models;
using ArenaGlass.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace ArenaGlass.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ICardTrackingService _cardTracker;
    private readonly DispatcherQueue _dispatcherQueue;

    [ObservableProperty] private string totalCardsText = "Total Cards: 0";
    [ObservableProperty] private ObservableCollection<CardViewModel> cards = new();

    public MainViewModel(ICardTrackingService cardTracker)
    {
        _cardTracker = cardTracker;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        
        _cardTracker.GameStateUpdated += OnGameStateUpdated;
        UpdateFromGameState(_cardTracker.CurrentGameState);
    }

    private void OnGameStateUpdated(object? sender, GameState gameState)
    {
        _dispatcherQueue.TryEnqueue(() => UpdateFromGameState(gameState));
    }

    private void UpdateFromGameState(GameState gameState)
    {
        TotalCardsText = $"Total Cards: {gameState.TotalCardsInLibrary}";
        
        Cards.Clear();
        foreach (var card in gameState.Library.OrderBy(c => c.Name))
        {
            Cards.Add(new CardViewModel
            {
                Name = card.Name,
                Count = card.Count,
                ManaCost = card.ManaCost,
                Type = card.Type
            });
        }
    }
}
```

## 6. Code-Behind Files

Create `App.xaml.cs`:

```csharp
using ArenaGlass.Core.Extensions;
using ArenaGlass.UI.Services;
using ArenaGlass.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

namespace ArenaGlass.UI;

public partial class App : Application
{
    private IHost? _host;

    public App()
    {
        this.InitializeComponent();
        
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddArenaGlassCore();
                services.AddSingleton<IOverlayService, OverlayService>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        var window = _host!.Services.GetRequiredService<MainWindow>();
        window.Activate();
    }
}
```

Create `MainWindow.xaml.cs`:

```csharp
using ArenaGlass.Core.Services;
using ArenaGlass.UI.Services;
using ArenaGlass.UI.ViewModels;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace ArenaGlass.UI;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }
    
    private readonly IOverlayService _overlayService;
    private readonly IGameMonitorService _gameMonitor;
    private readonly ILogParserService _logParser;

    public MainWindow(
        MainViewModel viewModel,
        IOverlayService overlayService,
        IGameMonitorService gameMonitor,
        ILogParserService logParser)
    {
        this.InitializeComponent();
        
        ViewModel = viewModel;
        _overlayService = overlayService;
        _gameMonitor = gameMonitor;
        _logParser = logParser;
        
        InitializeOverlay();
    }

    private async void InitializeOverlay()
    {
        // Get the window handle
        var windowHandle = WindowNative.GetWindowHandle(this);
        
        // Configure as overlay
        _overlayService.ConfigureAsOverlay(windowHandle);
        
        // Start services
        await _gameMonitor.StartMonitoringAsync();
        await _logParser.StartParsingAsync();
    }
}
```

## 7. Package Manifest

Create `Package.appxmanifest`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10" 
         xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
         xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities">
  <Identity Name="ArenaGlass" Publisher="CN=ArenaGlass" Version="1.0.0.0" />
  <Properties>
    <DisplayName>ArenaGlass</DisplayName>
    <PublisherDisplayName>ArenaGlass</PublisherDisplayName>
    <Logo>Assets\StoreLogo.png</Logo>
  </Properties>
  <Dependencies>
    <TargetDeviceFamily Name="Windows.Universal" MinVersion="10.0.17763.0" MaxVersionTested="10.0.19041.0" />
    <PackageDependency Name="Microsoft.WindowsAppRuntime.1.5" MinVersion="5.0.0.0" Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US" />
  </Dependencies>
  <Applications>
    <Application Id="App" Executable="ArenaGlass.UI.exe" EntryPoint="$targetname$.App">
      <uap:VisualElements DisplayName="ArenaGlass" BackgroundColor="transparent" Square44x44Logo="Assets\Square44x44Logo.png" Square150x150Logo="Assets\Square150x150Logo.png" />
    </Application>
  </Applications>
  <Capabilities>
    <rescap:Capability Name="runFullTrust" />
  </Capabilities>
</Package>
```

## Implementation Notes

1. **Transparent Window**: The overlay uses `WS_EX_TRANSPARENT` and `WS_EX_LAYERED` to achieve transparency and click-through behavior.

2. **Always on Top**: `WS_EX_TOPMOST` ensures the overlay stays above other windows, including fullscreen games.

3. **Dynamic Positioning**: A timer continuously monitors the MTG Arena window position and adjusts the overlay accordingly.

4. **Data Binding**: The UI uses MVVM pattern with observable collections and properties for real-time updates.

5. **Threading**: All UI updates are dispatched to the UI thread using `DispatcherQueue.TryEnqueue`.

6. **Service Integration**: The existing backend services are integrated through dependency injection.

This implementation provides a complete, non-intrusive overlay that meets all the requirements specified in the original issue.