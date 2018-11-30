using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.DI.Core;
using Akka.DI.Ninject;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using Shared;
using Shared.Actors;
using Shared.Repos;

namespace ClusterWorkerWestCoast
{
    public class MyService
    {
        public void Start()
        {
            SystemActors.ClusterSystem = SystemHostFactory.Launch();

            var pbm = PetabridgeCmd.Get(SystemActors.ClusterSystem);
            pbm.RegisterCommandPalette(ClusterCommands.Instance); // enable cluster management commands
            pbm.Start();

            // Create and build the container
            var container = new Ninject.StandardKernel();
            container.Bind<IFileProcessorRepository>().To(typeof(FileProcessorRepository)).InTransientScope();

            // Create the dependency resolver for the actor system
            IDependencyResolver resolver = new NinjectDependencyResolver(container, SystemActors.ClusterSystem);
            
            SystemActors.Mediator = DistributedPubSub.Get(SystemActors.ClusterSystem).Mediator;
            SystemActors.SettingsWatcherRef = SystemActors.ClusterSystem.ActorOf(SystemActors.ClusterSystem.DI().Props<DatabaseWatcherActor>(), "SettingWatchers");
        }

        public Task TerminationHandle => SystemActors.ClusterSystem.WhenTerminated;

        public async Task StopAsync()
        {
            await CoordinatedShutdown.Get(SystemActors.ClusterSystem).Run();
        }

    }
}
