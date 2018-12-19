using System;
using Akka.Actor;
using Akka.Event;

namespace Demo.Actors.AtLeastOnceDelivery
{
    public class MyRecipientActor : ReceiveActor
    {
        private readonly ILoggingAdapter _logger = Context.GetLogger();
        public MyRecipientActor()
        {
            Receive<ReliableDeliveryEnvelope<DeliverJob>>(write =>
            {
                _logger.Info("Received message {0} [id: {1}] from {2} - accept?", write.Job.Content, write.JobId, Sender);
                var response = Console.ReadLine()?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(response) && (response.Equals("yes") || response.Equals("y")))
                {
                    // confirm delivery only if the user explicitly agrees
                    Sender.Tell(new ReliableDeliveryAck(write.JobId));
                    _logger.Info("Confirmed delivery of JobId {0}", write.JobId);
                }
                else
                {
                    _logger.Info("Did not confirm delivery of JobId {0}", write.JobId);
                }
            });
        }
    }
}