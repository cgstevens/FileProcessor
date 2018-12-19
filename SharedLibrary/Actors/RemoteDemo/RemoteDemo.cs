using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Akka.Event;

namespace SharedLibrary.Actors.RemoteDemo
{
    public class StartJob { }
    public class JobActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        public JobActor()
        {
            Receive<Job>(job =>
            {
                _log.Info($"Working on {job.Key}");

                Sender.Tell(job, Self);
            });
        }
    }

    public class Job
    {
        public Job(string key)
        {
            Key = key;
        }

        public string Key { get; private set; }
    }

    public class JobManagerActor : ReceiveActor
    {
        private IActorRef _remoteActor;
        private int _jobounter;
        private ICancelable _helloTask;
        private readonly ILoggingAdapter _log = Context.GetLogger();
        public JobManagerActor(IActorRef remoteActor)
        {
            _remoteActor = remoteActor;
            Receive<Job>(job =>
            {
                _log.Info($"Received job complete for {job.Key}");
            });

            Receive<StartJob>(job =>
            {
                _jobounter++;
                _remoteActor.Tell(new Job($"Job_{_jobounter}"));
                _log.Info($"Sending Job_{_jobounter}");
            });
        }

        protected override void PreStart()
        {
            _helloTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5), Context.Self, new StartJob(), ActorRefs.NoSender);
        }

        protected override void PostStop()
        {
            _helloTask.Cancel();
        }
    }
}
