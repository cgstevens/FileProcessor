using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using Akka.Persistence;
using SharedLibrary.Helpers;

namespace Demo.Actors.Persistence
{
    public static class PersistenceDemo
    {
        public static void Start()
        {
            var systemConfig = ConfigurationFactory.ParseString(File.ReadAllText("akka.hocon"));
            var myConfig = systemConfig.GetConfig("myactorsystem");
            var systemName = myConfig.GetString("actorsystem");
            
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


            // Use File System as storage - Default
            //SystemActors.System = ActorSystem.Create(systemName);

            // Use SQL as storage
            SystemActors.System = ActorSystem.Create(systemName, specString);


            var myJobs = SystemActors.System.ActorOf(Props.Create(() => new MyJobsActor()), "MyJobManager");

            var waiter = new Waiter();
            for (int i = 0; i < 12; i++)
            {
                // Send a message randomly up to 2 seconds.
                // This is to pretend like we recieved 12 jobs.
                var random = new Random(i);
                int randomNumber = random.Next(0, 2000);
                waiter.Wait(TimeSpan.FromMilliseconds(randomNumber));
                myJobs.Tell(new MyJobsActor.StartJob($"MyJob_{i}"));
            }

            // Does the actor have all the jobs?
            var result = myJobs.Ask(new MyJobsActor.GetJobs(), TimeSpan.FromSeconds(1)).Result as Array;
            Console.WriteLine(String.Join(", ", result));
        }
        
        public class MyJobsActor : ReceivePersistentActor
        {
            public class GetJobs { }

            public class StartJob
            {
                public string Name { get; }

                public StartJob(string name)
                {
                    Name = name;
                }
            }
            public class Job
            {
                public int Id { get; set; }
                public string Name { get; set; }

                public Job() { }
                public Job(int id, string name)
                {
                    Id = id;
                    Name = name;
                }
            }

            private readonly ILoggingAdapter _log = Context.GetLogger();
            private List<Job> _jobs = new List<Job>(); //INTERNAL STATE

            private int _nextJobId
            {
                get { return _jobs.Count() + 1; }
            }

            private int _msgsSinceLastSnapshot = 0;

            public override string PersistenceId => Context.Self.Path.Name;

            public MyJobsActor()
            {

                Recover<string>(job =>
                {
                    // from the journal
                    _log.Info($"Load Journal : Job loaded={job}_{_nextJobId}");

                    _jobs.Add(new Job(_nextJobId, job));

                });

                // recover
                Recover<Job>(job =>
                {// from the journal
                    _log.Info($"Load Journal : Job loaded={job.Id}");

                    _jobs.Add(job);

                }); 
                Recover<SnapshotOffer>(offer => 
                {// from snapshot

                    var jobs = offer.Snapshot as List<Job>;
                    if (jobs != null)
                    {
                        _log.Info($"Load Snapshot : Jobs loaded={jobs.Count}");
                        _log.Info(String.Join(",", jobs.Select(x => x.Name)));
                        _jobs = _jobs.Concat(jobs).ToList();
                    }
                });

                // commands
                Command<StartJob>(job => Persist(job.Name, s =>
                {
                    var name = $"{job.Name}_{_nextJobId}";
                    _jobs.Add(new Job(_nextJobId, name)); //add msg to in-memory event store after persisting to data store

                    _log.Info($"Job:{name}");

                    if (++_msgsSinceLastSnapshot % 5 == 0)
                    {
                        //time to save a snapshot
                        _log.Info("Save Snapshot");
                        SaveSnapshot(_jobs);

                    }
                }));
                Command<SaveSnapshotSuccess>(success => {
                    // soft-delete the journal up until the sequence # at
                    // which the snapshot was taken
                    _log.Info("Save Snapshot Success; Deleting Journal Messages");
                    DeleteMessages(success.Metadata.SequenceNr);
                });
                Command<SaveSnapshotFailure>(failure => {
                    // handle snapshot save failure...
                    _log.Info("Save Snapshot Failure");
                });

                Command<DeleteMessagesFailure>(failure => {
                    // handle snapshot save failure...
                    _log.Info("Delete Messages Failure");
                });

                Command<DeleteMessagesSuccess>(success => {
                    // handle snapshot save success...
                    _log.Info("Delete Messages Success");
                });



                Command<GetJobs>(get =>
                {
                    Sender.Tell(_jobs.ToImmutableList());
                });
            }
        }


    }
}