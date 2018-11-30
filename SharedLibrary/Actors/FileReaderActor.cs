using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Routing;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.DI.Core;
using Akka.Event;
using Akka.Routing;
using FileHelpers;
using Shared.Enums;
using Shared.Helpers;
using Shared.Messages;
using Shared.Models;
using Shared.PubSub;

namespace Shared.Actors
{
    public class FileReaderActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly ILoggingAdapter _logger;
        private string _currentFile;
        private readonly IActorRef _locationActorRef;
        private readonly string _name;
        private Dictionary<string, UserRecord> _records;
        private readonly IActorRef _workerRouter;
        private readonly IActorRef _mediator = DistributedPubSub.Get(Context.System).Mediator;
        public IStash Stash { get; set; }

        /// <summary>
        /// This actor is responsible for reading the contents of a file.
        /// This example allows me to demonstrate the different ways you can distibute work.
        /// </summary>
        /// <param name="locationActorRef"></param>
        /// <param name="name"></param>
        public FileReaderActor(IActorRef locationActorRef, string name)
        {
            var workType = WorkType.Cluster;
            var self = Self;
            _name = name;
            _logger = Context.GetLogger();
            _locationActorRef = locationActorRef;
            _currentFile = "";
            _records = new Dictionary<string, UserRecord>();

            if (workType == WorkType.Cluster)
            {
                //***Cluster Round Robin example***//
                var clusterMaxWorkerInstancesPerNode = 1;
                var clusterMaxWorkerInstances = 3;
                _workerRouter = Context.ActorOf(new ClusterRouterPool(
                    local: new RoundRobinPool(clusterMaxWorkerInstancesPerNode),
                    settings: new ClusterRouterPoolSettings(clusterMaxWorkerInstances, clusterMaxWorkerInstancesPerNode, false, ClusterRole.FileWorker.ToString())
                ).Props(Props.Create(() => new LineReaderActor(self))));


            }
            else if(workType == WorkType.Local)
            {
                //*** Create a pool of actors ***// RoundRobinPool,  BroadcastPool

                var props = Props.Create<LineReaderActor>(self).WithRouter(new RoundRobinPool(3));
                _workerRouter = Context.ActorOf(props, "LineReaderActor");

            }
            else if (workType == WorkType.ConsistentHashing)
            {
                //*** Demonstrate ConsistentHashing ***//
                // TODO: Show how you can start work for a user and then cancel it using the key to route to the one actor.
                //public class SomeMessage : IConsistentHashable
                //{
                //    public Guid GroupID { get; private set; }
                //    public object ConsistentHashKey { get { return GroupID; } }
                //}

            }

            Become(WaitingToWork);
        }

        private void WaitingToWork()
        {
            Receive<ReadFile>(file =>
            {
                var self = Self;

                LogToEverything(Context, $"Recieved File {file.Args.FullPath}");
                _currentFile = file.Args.FullPath;

                // Ask if there are any routees to workers so that we can start the process.
                _workerRouter.Ask<Routees>(new GetRoutees(), TimeSpan.FromSeconds(5)).ContinueWith(tr =>
                    {
                        if (tr.IsFaulted)
                        {
                            _logger.Error(tr.Exception, "WorkerRouter was Faulted ");
                            return new FoundAvailableWorkers(0, new ReadFile(file.Sender, file.Args));
                        }
                        if (tr.IsCanceled)
                        {
                            _logger.Error(tr.Exception, "WorkerRouter was Canceled ");
                            return new FoundAvailableWorkers(0, new ReadFile(file.Sender, file.Args));
                        }

                        return new FoundAvailableWorkers(tr.Result.Members.Count(), new ReadFile(file.Sender, file.Args));
                    }, TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously).PipeTo(self);
                
            });

            Receive<FoundAvailableWorkers>(found => found.WorkersAvailable == 0, found =>
            {
                _logger.Warning("No workers found so file was not processed.");
            });

            Receive<FoundAvailableWorkers>(found =>
            {
                Self.Tell(new WorkFile(found.ReadFile.Sender, found.ReadFile.Args));
                Become(Working);
            });

            ReceiveAny(task =>
            {
                _logger.Error(" [x] Oh Snap! Unhandled message: \r\n{0}", task);
            });
        }


        private void Working()
        {
            Receive<ReadFile>(file =>
            {
                if (_currentFile == file.Args.FullPath)
                {
                    LogToEverything(Context, $"Already working Recieved File {file.Args.FullPath}");
                }
                else
                {
                    LogToEverything(Context, $"Stashing Recieved File {file.Args.FullPath}");
                    Stash.Stash();
                }
            });

            Receive<WorkFile>(file =>
            {
                LogToEverything(Context, $"Working File {file.Args.FullPath}");

                var engine = new FileHelperEngine<FileModel>();
                var records = engine.ReadFile(file.Args.FullPath);
                int x = 0;
                foreach (var record in records)
                {
                    var userRecord = new UserRecord(record.AdUserName, x++);
                    if (!_records.ContainsKey(userRecord.AdUserName))
                    {
                        LogToEverything(Context, $"Working Row {userRecord.AdUserName} ");
                        _workerRouter.Tell(new ProcessLine(userRecord.AdUserName));
                        _records.Add(userRecord.AdUserName, userRecord);
                    }
                    else
                    {
                        LogToEverything(Context, $"Duplicate Row {userRecord.AdUserName} ");
                    }
                }

            });

            Receive<LineComplete>(user =>
            {
                if(_records.ContainsKey(user.UserName))
                {
                    LogToEverything(Context, $"The line has been processed for {user.UserName}");
                    _records[user.UserName].Processed = true;
                }

                if (_records.Values.All(x => x.Processed))
                {
                    LogToEverything(Context, $"File has been processed: {_currentFile} ");

                    _currentFile = "";
                    _records = new Dictionary<string, UserRecord>();
                    Become(WaitingToWork);
                    Stash.UnstashAll();
                }

                
            });
            

            ReceiveAny(task =>
            {
                _logger.Error(" [x] Oh Snap! Unhandled message: \r\n{0}", task);
            });
        }

        private void LogToEverything(IUntypedActorContext context, string message)
        {
            //context.ActorSelection("akka.tcp://mysystem@127.0.0.1:4063/user/StatusActor").Tell(new SignalRMessage(StaticMethods.GetServiceName(), "FileReader", message));
            _mediator.Tell(new Publish(Topics.Status, new SignalRMessage($"{DateTime.Now}: {StaticMethods.GetSystemUniqueName()}", "FileReader", message)), context.Self);
            _logger.Info(message);
        }
    }
}

