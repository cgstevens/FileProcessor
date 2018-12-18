using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using Akka.Persistence;
using SharedLibrary.Helpers;

namespace Demo.Actors
{
    public static class PersistenceDemo
    {
        public static void Start()
        {
            var systemConfig = ConfigurationFactory.ParseString(File.ReadAllText("akka.hocon"));
            var myConfig = systemConfig.GetConfig("myactorsystem");
            var systemName = myConfig.GetString("actorsystem");

            var persistenceConfig = systemConfig.GetConfig("akka.persistence");
            SystemActors.System = ActorSystem.Create(systemName, persistenceConfig);

            var actorA = SystemActors.System.ActorOf(Props.Create(() => new ActorA()), "A");

            var waiter = new Waiter();
            for (int i = 0; i < 10; i++)
            {
                var random = new Random(i);
                int randomNumber = random.Next(0, 3000);
                waiter.Wait(TimeSpan.FromMilliseconds(randomNumber));
                actorA.Tell($"Test Message:{i}");
            }
        }
        
        public class ActorA : ReceivePersistentActor
        {
            public class GetMessages { }

            private readonly ILoggingAdapter _log = Context.GetLogger();
            private List<string> _msgs = new List<string>(); //INTERNAL STATE
            private int _msgsSinceLastSnapshot = 0;

            public override string PersistenceId => Context.Self.Path.Name;

            public ActorA()
            {
                // recover
                Recover<string>(str => _msgs.Add(str)); // from the journal
                Recover<SnapshotOffer>(offer => {
                    var messages = offer.Snapshot as List<string>;
                    if (messages != null)
                    {
                        _log.Info($"Load Snapshot : Messages loaded={messages.Count}");

                        _msgs = _msgs.Concat(messages).ToList();
                    }
                });

                // commands
                Command<string>(str => Persist(str, s => {
                    _msgs.Add(str); //add msg to in-memory event store after persisting

                    _log.Info($"Message:{str}");

                    if (++_msgsSinceLastSnapshot % 5 == 0)
                    {
                        //time to save a snapshot
                        _log.Info("Save Snapshot");
                        SaveSnapshot(_msgs);

                    }
                }));
                Command<SaveSnapshotSuccess>(success => {
                    // soft-delete the journal up until the sequence # at
                    // which the snapshot was taken
                    DeleteMessages(success.Metadata.SequenceNr);
                });
                Command<SaveSnapshotFailure>(failure => {
                    // handle snapshot save failure...
                });
                Command<GetMessages>(get =>
                {
                    Sender.Tell(_msgs.ToImmutableList());
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