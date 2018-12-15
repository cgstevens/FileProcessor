using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Akka.Pattern;
using Ninject.Activation;
using SharedLibrary.Helpers;
using SharedLibrary.Messages;

namespace ProcessorCentral.Actors
{
    public static class FutureDemo
    {
        public static void Start()
        {
            SystemActors.System = ActorSystem.Create("MySystem");

            // Show how to use a task to get results from multiple actors
            // Demo 
            var actorA = SystemActors.System.ActorOf(Props.Create(() => new ActorA()), "A");
            var actorB = SystemActors.System.ActorOf(Props.Create(() => new ActorB()), "B");
            var actorC = SystemActors.System.ActorOf(Props.Create(() => new ActorC()), "C");

            // Get Result
            var result = actorA.Ask("[Me]Am I there?", TimeSpan.FromSeconds(1)).Result;
            Console.WriteLine($"ConsoleWriteLine: {result}");

            // WhenAll asks complete continue.
            var actorATask = actorA.Ask("[Me]Is Anderson there?", TimeSpan.FromSeconds(1));
            var actorBTask = actorB.Ask("[Me]What about Neo?", TimeSpan.FromSeconds(5));

            Task.WhenAll(actorATask, actorBTask).ContinueWith(x =>
            {
                switch (x.Status)
                {
                    case TaskStatus.RanToCompletion:
                        return "Successfully checked for location.";
                    case TaskStatus.Canceled:
                        return $"Task was canceled. {x.Exception?.Message}";
                    case TaskStatus.Faulted:
                        return $"Task faulted. {x.Exception?.Message}";
                }

                return x.Result[0].ToString() + "; " + x.Result[1].ToString();
            }).PipeTo(actorC, ActorRefs.Nobody);
        }

        public class ActorA : ReceiveActor
        {
            private readonly ILoggingAdapter _log = Context.GetLogger();
            
            public ActorA()
            {
                Receives();
            }

            private void Receives()
            {
                Receive<string>(s =>
                {
                    _log.Info($"{s}");
                    Sender.Tell($"{s} [ActorA]No!!!", Self);
                });
            }
        }

        public class ActorB : ReceiveActor
        {
            private readonly ILoggingAdapter _log = Context.GetLogger();

            public ActorB()
            {
                Receives();
            }

            private void Receives()
            {
                Receive<string>(s =>
                {
                    var waiter = new Waiter();
                    waiter.Wait(TimeSpan.FromSeconds(4));
                    _log.Info($"{s}");
                    Sender.Tell($"{s} [ActorB]Welcome to the Matrix.", Self);
                });
            }
        }

        public class ActorC : ReceiveActor
        {
            private readonly ILoggingAdapter _log = Context.GetLogger();

            public ActorC()
            {
                Receives();
            }

            private void Receives()
            {
                Receive<string>(s =>
                {
                    _log.Info($"[ActorC]Listens : {s}");
                });

                ReceiveAny(s =>
                {
                    _log.Info($"RecieveAny : {s}");
                });
            }
        }
    }
}