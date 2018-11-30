using System;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;
using LocationStatusViewer.Hubs;
using Shared.Messages;
using Shared.PubSub;

namespace LocationStatusViewer.Actors
{
    /// <summary>
    /// Actor used to wrap a signalr hub
    /// </summary>
    public class SignalRActor : ReceiveActor, IWithUnboundedStash
    {
        #region Messages

        public class SetHub : INoSerializationVerificationNeeded
        {
            public SetHub(StatusHubHelper hub)
            {
                Hub = hub;
            }
            public StatusHubHelper Hub { get; }
        }

        #endregion


        private StatusHubHelper _hub;
        private readonly ILoggingAdapter _logger;


        public SignalRActor()
        {
            var self = Self;
            _logger = Context.GetLogger();
            SystemActors.Mediator.Tell(new Subscribe(Topics.Status, self));
            WaitingForHub();
        }
        protected override void PostStop()
        {
            var self = Self;
            SystemActors.Mediator.Tell(new Unsubscribe(Topics.Status, self));
            base.PostStop();
        }

        private void HubAvailable()
        {
            Receive<SignalRMessage>(ic =>
            {
                _hub.WriteMessage(ic.System, ic.Actor, ic.Message);
            });

            Receive<UnsubscribeAck>(ic =>
            {
                _logger.Info($"Successfully unsubscribed from group:{ic.Unsubscribe.Group} and topic:{ic.Unsubscribe.Topic}");
            });

            Receive<SubscribeAck>(ic =>
            {
                _logger.Info($"Successfully subscribed to group:{ic.Subscribe.Group} and topic:{ic.Subscribe.Topic}");
            });
        }

        private void WaitingForHub()
        {
            Receive<SetHub>(h =>
            {
                _hub = h.Hub;
                Become(HubAvailable);
                Stash.UnstashAll();
            });

            ReceiveAny(_ => Stash.Stash());
        }


        public IStash Stash { get; set; }
    }
}