using System;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;
using SharedLibrary.Helpers;
using SharedLibrary.Messages;
using SharedLibrary.PubSub;

namespace SharedLibrary.Actors
{
    public class LocationActor : ReceiveActor
    {
        private readonly ILoggingAdapter _logger;
        private IActorRef _fileReaderActorRef;
        private IActorRef _fileWatcherActorRef;
        private readonly IActorRef _parentActorRef;
        private readonly IActorRef _mediator = DistributedPubSub.Get(Context.System).Mediator;
        public string Name;

        

        public LocationActor(IActorRef parentActorRef, string name)
        {
            Name = name;
            _parentActorRef = parentActorRef;
            _logger = Context.GetLogger();
            BecomeStartup();
        }

        public LocationActor(IActorRef parentActorRef) : this(parentActorRef, "New")
        {
        }

        protected override void PostStop()
        {
            _logger.Info($"Actor has stopped.");
            base.PostStop();
        }

        private void BecomeStartup()
        {
            _fileReaderActorRef = Context.ActorOf(Props.Create(() => new FileReaderActor(Self, Name)), "FileReader");
            _fileWatcherActorRef = Context.ActorOf(Props.Create(() => new FileWatcherActor(Self, _fileReaderActorRef, Name)), "FileWatcher");
            LogToEverything(Context, $"{Name} location actor is starting.");
            Become(Startup);
        }

        // Become State
        // When we start up we need to reset any jobs that this member was running.
        private void Startup()
        {
            ReceiveAny(task =>
            {
                _logger.Error(" [x] Oh Snap! Unhandled message: \r\n{0}", task);
            });
        }
        private void LogToEverything(IUntypedActorContext context, string message)
        {
            //context.ActorSelection("akka.tcp://mysystem@127.0.0.1:4063/user/StatusActor").Tell(new SignalRMessage(StaticMethods.GetServiceName(), "Location", message));
            _mediator.Tell(new Publish(Topics.Status, new SignalRMessage($"{DateTime.Now}: {StaticMethods.GetSystemUniqueName()}", "Location", message)), context.Self);
            _logger.Info(message);
        }
    }
}

