using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;
using SharedLibrary.Helpers;
using SharedLibrary.Messages;
using SharedLibrary.PubSub;
using SharedLibrary.Repos;

namespace SharedLibrary.Actors
{
    public class CreateUserActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly ILoggingAdapter _logger;

        private readonly IFileProcessorRepository _fileProcessorRepository;
        private readonly IActorRef _parentActorRef;
        private string _currentRecord;
        private CancellationTokenSource _cancelToken;
        private readonly IActorRef _mediator = DistributedPubSub.Get(Context.System).Mediator;
        public IStash Stash { get; set; }

        public CreateUserActor(IFileProcessorRepository fileProcessorRepository)
        {
            _fileProcessorRepository = fileProcessorRepository ?? throw new ArgumentNullException(nameof(fileProcessorRepository));
            _logger = Context.GetLogger();
            _parentActorRef = Context.Parent;
            _currentRecord = String.Empty;
            Become(WaitingToWork);
            _cancelToken = new CancellationTokenSource();
        }

        protected override void PreStart()
        {
            LogToEverything(Context, "CreateUserActor is pre-starting.");
            base.PreStart();
        }

        protected override void PostStop()
        {
            _cancelToken?.Cancel(false);
            _cancelToken?.Dispose();

            Sender?.Tell(new BadDataShutdown(Self, _currentRecord));
            LogToEverything(Context, "CreateUserActor shutting down.");
            base.PostStop();
        }



        private void WaitingToWork()
        {
            
            Receive<ProcessLine>(record =>
            {
                var self = Self;
                _logger.Info($"{record.UserName} : User process is starting.");
                _currentRecord = record.UserName;

                var random = new Random();
                int randomNumber = random.Next(0, 5);
                if(randomNumber == 1 || randomNumber == 4)
                    throw new BadDataException("Something happened... please help me.");


                Become(Working);

                ProcessIdentity();
            });

            ReceiveAny(task =>
            {
                _logger.Error(" [x] Oh Snap! Unhandled message: \r\n{0}", task);
            });
        }

        private void ProcessIdentity()
        {
            var self = Self;
            LogToEverything(Context, $"{_currentRecord} Creating User...");
            Task.Run(() =>
                {
                    Action<string> callback = (x) => self.Tell(new RepoMessage(x));

                    var random = new Random();
                    int randomNumber = random.Next(10, 20);
                    _fileProcessorRepository.LongRunningProcess(_currentRecord, randomNumber, callback);

                    return new RecordHasBeenProcessed(true, null);

                }, _cancelToken.Token).ContinueWith(x =>
                    {
                        switch (x.Status)
                        {
                            case TaskStatus.RanToCompletion:
                                return new RecordHasBeenProcessed(true, $"{_currentRecord} user was successfully created!");
                            case TaskStatus.Canceled:
                                _logger.Error(x.Exception, "Task was canceled.");
                                return new RecordHasBeenProcessed(false, x.Exception.Message);
                            case TaskStatus.Faulted:
                                _logger.Error(x.Exception, "Task faulted.");
                                return new RecordHasBeenProcessed(false, x.Exception.Message);
                        }

                        return x.Result;

                    }, TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                .PipeTo(self);
        }


        private void Working()
        {
            Receive<ProcessLine>(file =>
            {
                Stash.Stash();
            });

            Receive<RepoMessage>(m =>
            {
                LogToEverything(Context, $"Sql: {m.Message}");
            });

            Receive<RecordHasBeenProcessed>(file => !file.Successful, file =>
            {
                LogToEverything(Context, $"Something happened and didn't processing record {_currentRecord} and will try again.");
                
                ProcessIdentity();
            });

            Receive<RecordHasBeenProcessed>(file => file.Successful, file =>
            {
                LogToEverything(Context, $"{_currentRecord} User was processed.");

                _parentActorRef.Tell(file);

                _currentRecord = String.Empty;
                Become(WaitingToWork);
                Stash.UnstashAll();
            });

            ReceiveAny(task =>
            {
                _logger.Error(" [x] Oh Snap! Unhandled message: \r\n{0}", task);
            });
        }

        private void LogToEverything(IUntypedActorContext context, string message)
        {
            //context.ActorSelection("akka.tcp://mysystem@127.0.0.1:4063/user/StatusActor").Tell(new SignalRMessage(StaticMethods.GetServiceName(), "User", message));
            _mediator.Tell(new Publish(Topics.Status, new SignalRMessage($"{DateTime.Now}: {StaticMethods.GetSystemUniqueName()}", "User", message)), context.Self);
            _logger.Info(message);
        }
    }
}

