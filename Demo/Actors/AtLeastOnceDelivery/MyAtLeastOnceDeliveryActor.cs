using System;
using Akka.Actor;
using Akka.Event;
using Akka.Persistence;

namespace Demo.Actors.AtLeastOnceDelivery
{
    public class MyAtLeastOnceDeliveryActor : AtLeastOnceDeliveryReceiveActor
    {
        // Going to use our name for persistence purposes
        public override string PersistenceId => Context.Self.Path.Name;
        private int _counter = 0;
        private readonly ILoggingAdapter _logger;

        private ICancelable _recurringMessageSend;
        private ICancelable _recurringSnapshotCleanup;
        private readonly IActorRef _targetActor;

        private class DoSend { }
        private class CleanSnapshots { }
        
        public MyAtLeastOnceDeliveryActor(IActorRef targetActor)
        {
            _targetActor = targetActor;
            _logger = Context.GetLogger();
            // recover the most recent at least once delivery state
            Recover<SnapshotOffer>(offer => offer.Snapshot is Akka.Persistence.AtLeastOnceDeliverySnapshot, offer =>
            {
                //_logger.Info("SnapshotOffer");
                var snapshot = offer.Snapshot as Akka.Persistence.AtLeastOnceDeliverySnapshot;
                SetDeliverySnapshot(snapshot);
            });

            Command<DoSend>(send =>
            {
                //_logger.Info("DoSend");
                _counter++;
                Self.Tell(new Write($"Message count={_counter};"));
            });

            Command<Write>(write =>
            {
                Deliver(_targetActor.Path, messageId =>
                {
                    _logger.Info($"Write messageId:{messageId}; write:{write.Content}");
                    return new ReliableDeliveryEnvelope<Write>(write, messageId);

                });

                // save the full state of the at least once delivery actor
                // so we don't lose any messages upon crash
                SaveSnapshot(GetDeliverySnapshot());
            });

            Command<ReliableDeliveryAck>(ack =>
            {
                _logger.Info($"ReliableDeliveryAck messageId:{ack.MessageId}");
                ConfirmDelivery(ack.MessageId);
            });

            Command<CleanSnapshots>(clean =>
            {
                //_logger.Info("CleanSnapshots");
                // save the current state (grabs confirmations)
                SaveSnapshot(GetDeliverySnapshot());
            });

            Command<SaveSnapshotSuccess>(saved =>
            {
                //_logger.Info("SaveSnapshotSuccess");
                var seqNo = saved.Metadata.SequenceNr;
                DeleteSnapshots(new SnapshotSelectionCriteria(seqNo, saved.Metadata.Timestamp.AddMilliseconds(-1))); // delete all but the most current snapshot
            });

            Command<SaveSnapshotFailure>(failure =>
            {
                //_logger.Info("SaveSnapshotFailure");
                // log or do something else
            });
        }

        protected override void PreStart()
        {
            _recurringMessageSend = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(10), Self, new DoSend(), Self);

            //_recurringSnapshotCleanup =
            //    Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(10),
            //        TimeSpan.FromSeconds(10), Self, new CleanSnapshots(), ActorRefs.NoSender);

            base.PreStart();
        }

        protected override void PostStop()
        {
            _recurringSnapshotCleanup?.Cancel();
            _recurringMessageSend?.Cancel();

            base.PostStop();
        }
    }
}