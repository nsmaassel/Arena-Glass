# ArenaGlass WPF UI Components

This directory contains the complete WPF UI implementation for ArenaGlass. These components provide a modern Windows graphical interface for the MTG Arena deck tracker.

## 🎯 UI Architecture

### Core Files
- **`App.xaml`** - WPF Application definition with dark theme styling
- **`App.xaml.cs`** - Application startup logic and console mode fallback
- **`MainWindow.xaml`** - Main UI layout with real-time deck tracking display
- **`MainWindow.xaml.cs`** - Window logic and event handling

### ViewModels (MVVM Pattern)
- **`ViewModels/ViewModelBase.cs`** - Base class with INotifyPropertyChanged implementation
- **`ViewModels/MainViewModel.cs`** - Main window data binding and command handling
- **`ViewModels/RelayCommand.cs`** - Command implementation for button actions

## 🎨 UI Features

### Real-Time Display
- **Game Status**: Shows MTG Arena running/stopped state
- **Monitoring Status**: Displays active/stopped monitoring with color coding
- **Library Statistics**: Total cards and unique cards with live updates
- **Card List**: Scrollable list of remaining cards with quantities

### Interactive Controls
- **Start/Stop Monitoring**: Buttons to control MTG Arena process monitoring
- **Load Demo**: Button to load sample deck for testing
- **Clear Deck**: Button to reset current deck state
- **Simulate Draw**: Text input and button to test card drawing

### Modern Design
- **Dark Theme**: Professional dark color scheme matching gaming aesthetics
- **Responsive Layout**: Proper scaling and resizing behavior
- **Live Updates**: Real-time data binding with automatic UI refresh

## 🔧 Technical Implementation

### Event-Driven Architecture
```csharp
// Service events automatically update UI
_gameMonitor.GameStateChanged += OnGameStateChanged;
_cardTracker.LibraryChanged += OnLibraryChanged;
```

### Data Binding Examples
```xml
<!-- Two-way binding for card simulation -->
<TextBox Text="{Binding SimulationCardName, UpdateSourceTrigger=PropertyChanged}"/>

<!-- Command binding for actions -->
<Button Content="Start Monitoring" Command="{Binding StartMonitoringCommand}"/>

<!-- Collection binding for card list -->
<ListBox ItemsSource="{Binding RemainingCards}" DisplayMemberPath="Display"/>
```

### Thread-Safe Updates
```csharp
// Ensures UI updates happen on main thread
Application.Current.Dispatcher.Invoke(() =>
{
    UpdateCardDisplay();
});
```

## 🚀 Installation & Usage

### Prerequisites
- Windows 10/11
- .NET 8.0 with WPF workload
- Visual Studio 2022 or JetBrains Rider

### Setup Steps
1. **Enable WPF in Project**: Add `<UseWPF>true</UseWPF>` to `.csproj`
2. **Move UI Files**: Copy all files from this directory to project root
3. **Update Program.cs**: Integrate WPF application startup
4. **Build & Run**: Standard WPF application workflow

### Project File Configuration
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
</Project>
```

## 📱 UI Screenshots & Layout

### Main Window Layout
```
┌─────────── ArenaGlass MVP ───────────┐
│        MTG Arena Deck Tracker        │
├─────────────────────────────────────┤
│ 🎮 MTG Arena: Running               │
│ 📊 Monitoring: Active               │
├─────────────────────────────────────┤
│  Total Cards: 15    Unique: 10      │
├─────────────────────────────────────┤
│ Remaining Cards in Library          │
│ ┌─────────────────────────────────┐ │
│ │ 2x Counterspell                 │ │
│ │ 1x Dark Ritual                  │ │
│ │ 2x Lightning Bolt               │ │
│ │ 1x Island                       │ │
│ │ ...                             │ │
│ └─────────────────────────────────┘ │
├─────────────────────────────────────┤
│ [Start] [Stop] [Demo] [Clear]       │
│ Simulate: [__________] [Draw Card]  │
└─────────────────────────────────────┘
```

## 🔮 Future Enhancements

### Overlay Mode
- Transparent, click-through window
- Position over MTG Arena game window
- Always-on-top display mode

### Advanced Features
- Deck import/export functionality
- Statistics and analytics
- Custom card database integration
- Multi-deck support

## 🧪 Testing

The UI components integrate seamlessly with the existing service architecture:
- All game monitoring logic remains unchanged
- Event system provides real-time updates
- Demo mode works identically to console version
- Service cleanup handles proper resource disposal

## 💡 Benefits Over Console

### User Experience
- **Visual Clarity**: Cards displayed in organized, scrollable list
- **Real-time Feedback**: Immediate visual updates on game state changes
- **Intuitive Controls**: Point-and-click interface vs. command typing
- **Modern Aesthetics**: Professional appearance suitable for streaming

### Functionality
- **Multitasking**: Non-blocking UI allows other Windows interactions
- **Persistent Display**: Always visible deck state without console scrolling
- **Scalability**: Easy to add new features and visual elements
- **Integration Ready**: Foundation for overlay and advanced features

This UI implementation provides the complete foundation for a professional MTG Arena deck tracker while maintaining all the robust service architecture of the console version.