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


            var specString = @"
                    akka.persistence {
                        publish-plugin-commands = on
                        journal {
                            plugin = ""akka.persistence.journal.sql-server""
                            sql-server {
                                class = ""Akka.Persistence.SqlServer.Journal.SqlServerJournal, Akka.Persistence.SqlServer""
                                plugin-dispatcher = ""akka.actor.default-dispatcher""
                                table-name = DemoJournal
                                schema-name = dbo
                                auto-initialize = on
                                connection-string = ""Server=.;Database=AkkaFileProcessor;Integrated Security=SSPI;""
                            }
                        }

                        snapshot-store {
                            plugin = ""akka.persistence.snapshot-store.sql-server""
                            sql-server {
                                class = ""Akka.Persistence.SqlServer.Snapshot.SqlServerSnapshotStore, Akka.Persistence.SqlServer""
                                plugin-dispatcher = ""akka.actor.default-dispatcher""
                                table-name = DemoSnapShot
                                schema-name = dbo
                                auto-initialize = on
                                connection-string = ""Server=.;Database=AkkaFileProcessor;Integrated Security=SSPI;""
                            }
                        }
                    }";


            SystemActors.System = ActorSystem.Create(systemName, specString);

            var actorA = SystemActors.System.ActorOf(Props.Create(() => new ActorA()), "ActorA");

            var waiter = new Waiter();
            for (int i = 0; i < 12; i++)
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
                Recover<string>(str =>
                {// from the journal
                    _log.Info($"Load Journal : Messages loaded={str}");

                    _msgs.Add(str);

                }); 
                Recover<SnapshotOffer>(offer => 
                {// from snapshot

                    var messages = offer.Snapshot as List<string>;
                    if (messages != null)
                    {
                        _log.Info($"Load Snapshot : Messages loaded={messages.Count}");

                        _msgs = _msgs.Concat(messages).ToList();
                    }
                });

                // commands
                Command<string>(str => Persist(str, s => {
                    _msgs.Add(str); //add msg to in-memory event store after persisting to data store

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


    }
}