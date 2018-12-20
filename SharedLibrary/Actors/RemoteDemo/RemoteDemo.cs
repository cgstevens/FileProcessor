using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using SharedLibrary.Helpers;

namespace SharedLibrary.Actors.RemoteDemo
{
    public class StartJob { }
    public class GetStatus { }

    public class ReceivedStatus
    {
        public Job Job { get; }
        public string Status { get; }
        public ReceivedStatus(Job job, string status)
        {
            Job = job;
            Status = status;
        }
    }
    public class JobComplete
    {
        public Job Job { get; }
        public JobComplete(Job job)
        {
            Job = job;
        }
    }
    public class JobActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private Waiter _waiter = new Waiter();
        public JobActor()
        {
            Receive<Job>(job =>
            {
                // Pretend like we are doing some work.
                var random = new Random();
                int randomNumber = random.Next(1000, 10000);
                _log.Info($"Working on {job.Key} and will take {randomNumber} milliseconds.");

                _waiter.Wait(TimeSpan.FromMilliseconds(randomNumber));
                Sender.Tell(job, Self);
            });
        }
    }

    public class ReportActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private Waiter _waiter = new Waiter();
        private ICancelable _helloTask;
        public ReportActor()
        {
            _helloTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5), Context.Self, new GetStatus(), ActorRefs.NoSender);
            
            Receive<GetStatus>(status =>
            {
                var reportActor = Context.System.ActorSelection("/user/remotejob/*");
                reportActor.Tell(new GetStatus());
            });

            Receive<ReceivedStatus>(status =>
            {
                _log.Info($"{status.Job.Key} : {status.Status}");
            });
        }
    }

    public class JobWithBehaviorActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private Job _currentJob = new Job("NoJob");
        private int _waitTime;
        private IActorRef _currentSender;
        private Dictionary<string, Job> _totalMessagesInQueue;

        public JobWithBehaviorActor()
        {
            _totalMessagesInQueue = new Dictionary<string, Job>();
            Waiting();
        }

        private void Waiting()
        {
            Receive<GetStatus>(status =>
            {
                Sender.Tell(new ReceivedStatus(_currentJob, "Not working on anything"));
            });

            Receive<Job>(job =>
            {
                _currentJob = job;
                _currentSender = Sender;
                // Pretend like we are doing some work.
                var random = new Random();
                _waitTime = random.Next(1000, 10000);
                _log.Info($"Working on {job.Key} and will take {_waitTime} milliseconds.");

                Become(Working);
                Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromMilliseconds(_waitTime), Self, new JobComplete(job), Self);
            });
        }

        private void Working()
        {
            Receive<GetStatus>(status =>
            {
                int total = _totalMessagesInQueue.Count;
                _log.Info($"GetStatus : Total in queue {total}");
                Sender.Tell(new ReceivedStatus(_currentJob, $"Working; messages in queue {total}"));
            });

            Receive<JobComplete>(job =>
            {
                _currentSender.Tell(job);
                _currentJob = new Job("NoJob");
                Become(Waiting);
                _totalMessagesInQueue.Remove(job.Job.Key);
                Stash.UnstashAll();
            });

            Receive<Job>(job =>
            {
                _log.Info($"Working on {_currentJob.Key} and will stash {job.Key}. Total {_totalMessagesInQueue.Count}");

                if(!_totalMessagesInQueue.ContainsKey(job.Key))
                    _totalMessagesInQueue.Add(job.Key, job);

                Stash.Stash();
            });
        }


        public IStash Stash { get; set; }
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

            Receive<JobComplete>(job =>
            {
                _log.Info($"Received job complete for {job.Job.Key}");
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
                TimeSpan.FromSeconds(1), Context.Self, new StartJob(), ActorRefs.NoSender);

        }

        protected override void PostStop()
        {
            _helloTask.Cancel();
        }
    }
}
