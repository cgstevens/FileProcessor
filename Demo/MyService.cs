using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Demo.Actors;
using Demo.Actors.AtLeastOnceDelivery;

namespace Demo
{
    public class MyService
    {
        public void Start()
        {
            // Show how to use a task to get results from multiple actors
            // Demo 
            //FutureDemo.Start();


            //AtLeastOnceDeliveryDemo.Start();

            PersistenceDemo.Start();


        }

        public Task TerminationHandle => SystemActors.System.WhenTerminated;

        public async Task StopAsync()
        {
            await CoordinatedShutdown.Get(SystemActors.System).Run();
        }
    }
}
