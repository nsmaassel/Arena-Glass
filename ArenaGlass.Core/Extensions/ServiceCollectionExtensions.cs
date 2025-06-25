using ArenaGlass.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ArenaGlass.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddArenaGlassCore(this IServiceCollection services)
    {
        services.AddSingleton<IGameMonitorService, GameMonitorService>();
        services.AddSingleton<ILogParserService, LogParserService>();
        services.AddSingleton<ICardTrackingService, CardTrackingService>();
        
        return services;
    }
}