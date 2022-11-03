using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using AvalonMonitor.Actors;
using SharedLibrary.Actors;
using Splat;

namespace AvalonMonitor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        readonly IProcessLoggerItems _logger;

        public MainWindowViewModel(IProcessLoggerItems logger)
        {
            _logger = logger;
            InitializeClusters();
            InitializeActors();
        }

        void InitializeClusters()
        {
            SystemActors.ClusterSystem = SystemHostFactory.Launch();
            _logger.Process("Actor System Started");
        }

        void InitializeActors()
        {
            SystemActors.ClusterManagerActor = SystemActors.ClusterSystem.ActorOf(Props.Create(() => new ClusterStatusActor(
                Locator.Current.GetService<LoggerViewModel>(null),
                Locator.Current.GetService<ClusterViewModel>(null),
                Locator.Current.GetService<SeenByViewModel>(null),
                Locator.Current.GetService<UnreachableViewModel>(null))), "monitor");
            SystemActors.Mediator = DistributedPubSub.Get(SystemActors.ClusterSystem).Mediator;
            _logger.Process("Cluster Manager Actor Started");
        }

        public object LoggerViewContext => Locator.Current.GetService<LoggerViewModel>();
        public object ClusterViewContext => Locator.Current.GetService<ClusterViewModel>();
        public object SeenByViewContext => Locator.Current.GetService<SeenByViewModel>();
        public object UnreachableViewContext => Locator.Current.GetService<UnreachableViewModel>();
    }
}
