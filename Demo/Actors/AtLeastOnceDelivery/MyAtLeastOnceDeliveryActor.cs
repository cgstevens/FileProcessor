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
        private ICancelable _recurringJobSend;
        private ICancelable _recurringSnapshotCleanup;
        private readonly IActorRef _targetActor;
        private class StartJob { }
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

            Command<StartJob>(send =>
            {
                _counter++;

                _logger.Info($"StartJob {_counter}");
                Self.Tell(new DeliverJob($"Job count={_counter};"));
            });

            Command<DeliverJob>(write =>
            {
                Deliver(_targetActor.Path, jobId =>
                {
                    _logger.Info($"DeliverJob jobId:{jobId}; content:{write.Content}");
                    return new ReliableDeliveryEnvelope<DeliverJob>(write, jobId);

                });

                // save the full state of the at least once delivery actor
                // so we don't lose any messages upon crash
                SaveSnapshot(GetDeliverySnapshot());
            });

            Command<ReliableDeliveryAck>(ack =>
            {
                _logger.Info($"ReliableDeliveryAck jobId:{ack.JobId}");
                ConfirmDelivery(ack.JobId);
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
            _recurringJobSend = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(10), Self, new StartJob(), Self);

            //_recurringSnapshotCleanup =
            //    Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(10),
            //        TimeSpan.FromSeconds(10), Self, new CleanSnapshots(), ActorRefs.NoSender);

            base.PreStart();
        }

        protected override void PostStop()
        {
            _recurringSnapshotCleanup?.Cancel();
            _recurringJobSend?.Cancel();

            base.PostStop();
        }
    }
}