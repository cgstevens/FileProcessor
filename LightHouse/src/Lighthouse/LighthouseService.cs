// Copyright 2014-2015 Aaron Stannard, Petabridge LLC
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.DI.Core;
using Akka.DI.Ninject;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using SharedLibrary.Actors;
using SharedLibrary.Repos;

namespace Lighthouse
{
    public class LighthouseService
    {
        public void Start()
        {
            SystemActors.ClusterSystem = SystemHostFactory.Launch();
            // Create and build the container
            var container = new Ninject.StandardKernel();
            container.Bind<IFileProcessorRepository>().To(typeof(FileProcessorRepository)).InTransientScope();

            // Create the dependency resolver for the actor system
            IDependencyResolver resolver = new NinjectDependencyResolver(container, SystemActors.ClusterSystem);

            var pbm = PetabridgeCmd.Get(SystemActors.ClusterSystem);
            pbm.RegisterCommandPalette(ClusterCommands.Instance); // enable cluster management commands
            pbm.Start();

            SystemActors.Mediator = DistributedPubSub.Get(SystemActors.ClusterSystem).Mediator;
        }

        /// <summary>
        /// Task completes once the Lighthouse <see cref="ActorSystem"/> has terminated.
        /// </summary>
        /// <remarks>
        /// Doesn't actually invoke termination. Need to call <see cref="StopAsync"/> for that.
        /// </remarks>
        public Task TerminationHandle => SystemActors.ClusterSystem.WhenTerminated;

        public async Task StopAsync()
        {
            await CoordinatedShutdown.Get(SystemActors.ClusterSystem).Run();
        }
    }
}
