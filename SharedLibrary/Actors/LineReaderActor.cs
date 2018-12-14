using System;
using System.Collections.Generic;
using System.Threading;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.DI.Core;
using Akka.Event;
using Akka.Routing;
using SharedLibrary.Helpers;
using SharedLibrary.Messages;
using SharedLibrary.PubSub;

namespace SharedLibrary.Actors
{
    public class LineReaderActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly ILoggingAdapter _logger;
        private readonly IActorRef _parentActorRef;
        private readonly IActorRef _createIdentityRef;
        private readonly IActorRef _createUserRef;
        private readonly IActorRef _createPrivilegeRef;
        private readonly IActorRef _mediator = DistributedPubSub.Get(Context.System).Mediator;
        private string _currentRecord;
        private int _currentCount;
        private CancellationTokenSource _cancelToken;
        public IStash Stash { get; set; }
        private IActorRef _workerRouter;

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                maxNrOfRetries: 10,
                withinTimeRange: TimeSpan.FromSeconds(60),
                localOnlyDecider: ex =>
                {
                    switch (ex)
                    {
                        case ArithmeticException ae:
                            return Directive.Resume;
                        case BadDataException nre:
                            return Directive.Restart;
                        case ArgumentException are:
                            return Directive.Stop;
                        default:
                            return Directive.Escalate;
                    }
                });
        }

        /// <summary>
        /// This actor demonstrates how to create child actors that are put into a broadgroup.
        /// The route will send the message to all the actors in the group
        /// </summary>
        /// <param name="parent"></param>
        public LineReaderActor(IActorRef parent)
        {
            _logger = Context.GetLogger();
            _parentActorRef = parent; //Context.Parent;
            _currentRecord = string.Empty;
            Become(WaitingToWork);
            _cancelToken = new CancellationTokenSource();
            _currentCount = 0;
            
            _createIdentityRef = Context.ActorOf(Context.DI().Props<CreateIdentityActor>(), "CreateIdentity");
            _createUserRef = Context.ActorOf(Context.DI().Props<CreateUserActor>(), "CreateUser");
            _createPrivilegeRef = Context.ActorOf(Context.DI().Props<CreatePrivilegeActor>(), "CreatePrivilegeActor");

            var workers = new[] { _createIdentityRef.Path.ToString(), _createUserRef.Path.ToString(), _createPrivilegeRef.Path.ToString() };
            _workerRouter = Context.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(workers)), "LineReaderGroup");

        }
        protected override void PostStop()
        {
            _cancelToken?.Cancel(false);
            _cancelToken?.Dispose();
            base.PostStop();
        }

        private void WaitingToWork()
        {
            Receive<ProcessLine>(record =>
            {
                LogToEverything(Context, $"{record.UserName} is starting to be processed.");
                _currentCount = 0;
                _currentRecord = record.UserName;
                Become(Working);

                // *** BroadCast the message to all worker actors.
                _workerRouter.Tell(new ProcessLine(record.UserName));

                // Switch States
                Become(Working);
            });

            ReceiveAny(task =>
            {
                _logger.Error(" [x] Oh Snap! Unhandled message: \r\n{0}", task);
            });
        }


        private void Working()
        {
            Receive<BadDataShutdown>(failed =>
            {
                // Send Again after giving the failed actor a second to come back up.
                Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(1), failed.FailedActor, new ProcessLine(failed.CurrentRecord), Self);
            });

            Receive<ProcessLine>(file =>
            {
                Stash.Stash();
            });

            Receive<RecordHasBeenProcessed>(file => !file.Successful, file =>
            {
                _currentCount++;

                LogToEverything(Context, $"Something happened and didn't processing record {_currentRecord}");

                if (_currentCount == 3)
                {
                    // TODO: Need to process record again?
                    _currentRecord = String.Empty;
                    Become(WaitingToWork);
                    Stash.UnstashAll();
                }

            });

            Receive<RecordHasBeenProcessed>(file => file.Successful, file =>
            {
                _currentCount++;

                if (_currentCount == 3)
                {
                    LogToEverything(Context, $"{_currentRecord} Completed processing line.");

                    _parentActorRef.Tell(new LineComplete(_currentRecord));
                    _currentRecord = String.Empty;
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
            //context.ActorSelection("akka.tcp://mysystem@127.0.0.1:4063/user/StatusActor").Tell(new SignalRMessage(StaticMethods.GetServiceName(), "LineReader", message));
            _mediator.Tell(new Publish(Topics.Status, new SignalRMessage($"{DateTime.Now}: {StaticMethods.GetSystemUniqueName()}", "LineReader", message)), context.Self);
            _logger.Info(message);
        }
    }
}

