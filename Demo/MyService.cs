using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.DI.Core;
using Akka.DI.Ninject;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using ProcessorCentral.Actors;
using SharedLibrary.Actors;
using SharedLibrary.Helpers;
using SharedLibrary.Repos;

namespace ProcessorCentral
{
    public class MyService
    {
        public void Start()
        {
            SystemActors.System = ActorSystem.Create("MySystem");

            // Show how to use a task to get results from multiple actors
            // Demo 
            FutureDemo.Start();

        }

        public Task TerminationHandle => SystemActors.System.WhenTerminated;

        public async Task StopAsync()
        {
            await CoordinatedShutdown.Get(SystemActors.System).Run();
        }
    }
}
