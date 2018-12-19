namespace Demo.Actors.AtLeastOnceDelivery
{
    public class ReliableDeliveryEnvelope<TJob>
    {
        public ReliableDeliveryEnvelope(TJob job, long jobId)
        {
            Job = job;
            JobId = jobId;
        }

        public TJob Job { get; private set; }

        public long JobId { get; private set; }
    }

    public class ReliableDeliveryAck
    {
        public ReliableDeliveryAck(long jobId)
        {
            JobId = jobId;
        }

        public long JobId { get; private set; }
    }

    public class DeliverJob
    {
        public DeliverJob(string content)
        {
            Content = content;
        }

        public string Content { get; private set; }
    }
}
