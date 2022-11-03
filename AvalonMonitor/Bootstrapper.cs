using AvalonMonitor.ViewModels;
using Splat;

namespace AvalonMonitor;

public static class Bootstrapper
{
    public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton(() => new ClusterViewModel());
        services.RegisterLazySingleton(() => new LoggerViewModel());
        services.RegisterLazySingleton(() => new SeenByViewModel());
        services.RegisterLazySingleton(() => new UnreachableViewModel());
        services.RegisterLazySingleton(() => new MainWindowViewModel(resolver.GetService<LoggerViewModel>()));
    }
}