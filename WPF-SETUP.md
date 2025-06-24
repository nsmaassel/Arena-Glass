# WPF UI Quick Setup Guide

This guide shows how to quickly enable the full WPF UI on Windows.

## 🚀 Quick Setup (5 minutes)

### 1. Enable WPF in Project
Edit `ArenaGlass.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
</Project>
```

### 2. Move UI Components to Root
```bash
# Move WPF files to project root
mv ArenaGlass/UI-Components/App.xaml ArenaGlass/
mv ArenaGlass/UI-Components/App.xaml.cs ArenaGlass/
mv ArenaGlass/UI-Components/MainWindow.xaml ArenaGlass/
mv ArenaGlass/UI-Components/MainWindow.xaml.cs ArenaGlass/
mv ArenaGlass/UI-Components/ViewModels ArenaGlass/
```

### 3. Update Program.cs
Replace the Main method:
```csharp
[STAThread]
static void Main(string[] args)
{
    if (args.Length > 0 && args[0] == "--console")
    {
        RunConsoleMode().Wait();
        return;
    }

    // Run WPF application
    var app = new App();
    app.Run();
}
```

### 4. Build and Run
```bash
dotnet build
dotnet run
```

## 🎯 Result
- Modern WPF interface launches
- All existing functionality preserved
- Real-time UI updates working
- Ready for MTG Arena integration

## 🔄 Reverting to Console
```bash
dotnet run --console
```

The UI components are designed to coexist with the console version, allowing easy switching between modes during development and testing.