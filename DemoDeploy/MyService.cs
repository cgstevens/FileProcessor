using System.Threading.Tasks;
using Akka.Actor;
using DemoDeploy.Actors;

namespace DemoDeploy
{
    public class MyService
    {
        public void Start()
        {

            var remoteString = @"
                    akka {
                        actor.provider = remote
                        remote {
                            dot-netty.tcp {
                                port = 4080
                                hostname = 127.0.0.1
                            }
                        }
                }";

            SystemActors.System = ActorSystem.Create("MyWorker", remoteString);
        }

        public Task TerminationHandle => SystemActors.System.WhenTerminated;

        public async Task StopAsync()
        {
            await CoordinatedShutdown.Get(SystemActors.System).Run();
        }
    }
}
