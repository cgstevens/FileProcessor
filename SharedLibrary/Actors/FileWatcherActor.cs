using System;
using System.IO;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;
using SharedLibrary.Helpers;
using SharedLibrary.Messages;
using SharedLibrary.PubSub;

namespace SharedLibrary.Actors
{
    public class FileWatcherActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly ILoggingAdapter _logger;
        private FileSystemWatcher _watcher;
        private readonly IActorRef _locationActorRef;
        private readonly IActorRef _fileReaderRef;
        private DirectoryInfo _folderPath;
        protected ICancelable ValidateFolderAndSettings;
        private readonly int _scheduleDelay;
        private readonly IActorRef _mediator = DistributedPubSub.Get(Context.System).Mediator;
        private readonly string _name;

        public IStash Stash { get; set; }

        public FileWatcherActor(IActorRef locationActorRef, IActorRef fileReaderRef, string name)
        {
            _name = name;
            _scheduleDelay = 300;
            _locationActorRef = locationActorRef;
            _fileReaderRef = fileReaderRef;
            _logger = Context.GetLogger();
            BecomeStartup();
        }

        protected override void PostStop()
        {
            ValidateFolderAndSettings?.Cancel(false);
            _watcher?.Dispose();
            var self = Self;
            Context.ActorSelection("/user/SettingWatchers").Tell(new UnSubscribeToObjectChanges(self));
            LogToEverything(Context, $"FileSystemWatcher has stopped monitoring {_folderPath.FullName.ToString()}");

            _watcher = null;
            base.PostStop();
        }

        private void BecomeStartup()
        {
            var self = Self;
            Context.ActorSelection("/user/SettingWatchers").Tell(new SubscribeToObjectChanges(self, _name, "FileSettings.InboundFolder"));
            ValidateFolderAndSettings = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(_scheduleDelay), self, new ValidateFileWatcher(), self);
            Become(WaitingForSettings);
        }

        private void WaitingForSettings()
        {
            Receive<ObjectChanged>(f =>
            {
                LogToEverything(Context, $"Folder has been set to {f.ObjectValue.ToString()}");
                _folderPath = f.ObjectValue as DirectoryInfo;
                Become(UpdatingFileWatcher);
                Stash.UnstashAll();
            });

            ReceiveAny(task =>
            {
                Stash.Stash();
            });
        }

        private void UpdatingFileWatcher()
        {
            Receive<ValidateFileWatcher>(f =>
            {
                var self = Self;
                _watcher?.Dispose();
                _watcher = null;

                _watcher = new FileSystemWatcher(_folderPath.FullName.ToString(), "*.*")
                {
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
                };

                //_watcher.Changed += (sender, args) => self.Tell(new FileChanged(sender, args));
                _watcher.Created += (sender, args) => self.Tell(new FileCreated(sender, args));
                _watcher.Deleted += (sender, args) => self.Tell(new FileDeleted(sender, args));
                _watcher.EnableRaisingEvents = true;

                LogToEverything(Context, $"FileSystemWatcher is monitoring {_folderPath.FullName.ToString()}");
                Become(WaitingForWork);
                Stash.UnstashAll();
            });

            ReceiveAny(task =>
            {
                // Stash any message that we could get from the watcher.
                Stash.Stash();
            });
        }


        private void WaitingForWork()
        {
            Receive<ObjectChanged>(f =>
            {
                var directory = f.ObjectValue as DirectoryInfo;
                if (directory != null && _folderPath.FullName.ToString() != directory.FullName.ToString())
                {
                    LogToEverything(Context, $"Folder was changed from {_folderPath} to {directory.FullName.ToString()}");
                    _folderPath = directory;
                    Become(UpdatingFileWatcher);
                    Self.Tell(new ValidateFileWatcher());
                }
            });

            Receive<ValidateFileWatcher>(f =>
            {
                Become(UpdatingFileWatcher);
                Self.Tell(new ValidateFileWatcher());
            });

            Receive<FileChanged>(f =>
            {
                LogToEverything(Context, $"{f.Args.Name} was changed.");

                Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(1), _fileReaderRef, new ReadFile(f.Sender, f.Args), Self);
            });

            Receive<FileCreated>(f =>
            {
                LogToEverything(Context, $"{f.Args.Name} was created.");

                Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(1), _fileReaderRef, new ReadFile(f.Sender, f.Args), Self);
            });

            Receive<FileDeleted>(f =>
            {
                LogToEverything(Context, $"{f.Args.Name} was deleted.");
            });

            ReceiveAny(task =>
            {
                _logger.Error(" [x] Oh Snap! Unhandled message: \r\n{0}", task);
            });
        }
        private void LogToEverything(IUntypedActorContext context, string message)
        {
            //context.ActorSelection("akka.tcp://mysystem@127.0.0.1:4063/user/StatusActor").Tell(new SignalRMessage(StaticMethods.GetServiceName(), "FileWatcher", message));
            _mediator.Tell(new Publish(Topics.Status, new SignalRMessage($"{DateTime.Now}: {StaticMethods.GetSystemUniqueName()}", "FileWatcher", message)), context.Self);
            _logger.Info(message);
        }
    }
}

