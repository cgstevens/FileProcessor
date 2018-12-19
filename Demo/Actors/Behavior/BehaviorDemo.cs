using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using SharedLibrary.Helpers;

namespace Demo.Actors.Behavior
{
    public static class BehaviorDemo
    {
        public static void Start()
        {
            SystemActors.System = ActorSystem.Create("MySystem");
            var actorA = SystemActors.System.ActorOf(Props.Create(() => new ActorA()), "Marcus");
            
            Console.WriteLine("Type exit to quit.");
            Console.Write("Control Behavior of the actor. Actions [start, stop]: ");

            var consoleString = Console.ReadLine();
            while (consoleString != "exit")
            {
                var result = actorA.Ask(consoleString, TimeSpan.FromSeconds(5)).Result;
                Console.WriteLine($"Actor Response: {result}");

                Console.Write("Action: ");
                consoleString = Console.ReadLine();
            }
        }

        public class ActorA : ReceiveActor, IWithUnboundedStash
        {
            private readonly ILoggingAdapter _log = Context.GetLogger();

            public IStash Stash { get; set; }

            public ActorA()
            {
                Waiting();
            }

            // This actor is just sitting here waiting for someone to tell them to start.
            private void Waiting()
            {
                Receive<string>(s => s == "stop", s =>
                {
                    Sender.Tell("Nothing to stop as the actor has not started and is waiting to start.");
                });

                Receive<string>(s => s == "start", s =>
                {
                    BecomeStarting();
                });

                Receive<string>(s =>
                {
                    Sender.Tell($"Stashing {s}");
                    Stash.Stash();
                });
            }

            private void Started()
            {
                Receive<string>(s => s == "stop", s =>
                {
                    Sender.Tell("Stopping the actor.");
                    Become(Stopped);
                });

                Receive<string>(s => s == "start", s =>
                {
                    Sender.Tell("The actor has already been started.");
                });

                Receive<string>(s =>
                {
                    Sender.Tell($"{s}");
                    Console.WriteLine($"Echo: {s}");
                });
            }

            private void Stopped()
            {
                Receive<string>(s => s == "start", s =>
                {
                    BecomeStarting();
                });

                Receive<string>(s =>
                {
                    Sender.Tell($"Echo {s}");
                });
            }

            private void BecomeStarting()
            {
                Sender.Tell("Starting the actor.");
                Become(Started);

                Stash.UnstashAll();
            }
        }
        
    }
}