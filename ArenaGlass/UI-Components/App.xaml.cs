#if WINDOWS
using System.Windows;

namespace ArenaGlass;

/// <summary>
/// WPF Application class for ArenaGlass
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Check if we should run in console mode
        if (e.Args.Length > 0 && e.Args[0] == "--console")
        {
            // Run console version
            Shutdown();
            Program.RunConsoleMode();
            return;
        }
        
        // Continue with WPF startup
    }
}
#endif