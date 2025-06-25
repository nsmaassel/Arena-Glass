using ArenaGlass.Core.Extensions;
using ArenaGlass.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArenaGlass.UI;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("ArenaGlass - Live Deck Tracking Overlay");
        Console.WriteLine("=======================================");
        Console.WriteLine();

        var host = CreateHostBuilder(args).Build();
        
        var app = host.Services.GetRequiredService<ArenaGlassApplication>();
        await app.RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddArenaGlassCore();
                services.AddSingleton<ArenaGlassApplication>();
            });
}