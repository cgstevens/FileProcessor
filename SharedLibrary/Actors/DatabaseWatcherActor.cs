using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;
using Shared.Helpers;
using Shared.Messages;
using Shared.Models;
using Shared.PubSub;
using Shared.Repos;

namespace Shared.Actors
{
    public class DatabaseWatcherActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly ILoggingAdapter _logger;
        private readonly IFileProcessorRepository _fileProcessorRepository;
        private Dictionary<string, LocationModel> _locations;
        private CancellationTokenSource _cancelToken;
        private ICancelable _cancelable;
        private readonly IActorRef _mediator = DistributedPubSub.Get(Context.System).Mediator;
        private Dictionary<IActorRef, ObjectSubscription> _subscriptionsToObjects;
        public IStash Stash { get; set; }

        public DatabaseWatcherActor(IFileProcessorRepository fileProcessorRepository)
        {
            _fileProcessorRepository = fileProcessorRepository ?? throw new ArgumentNullException(nameof(fileProcessorRepository));
            _logger = Context.GetLogger();
            _locations = new Dictionary<string, LocationModel>();
            _subscriptionsToObjects = new Dictionary<IActorRef, ObjectSubscription>();
            _cancelToken = new CancellationTokenSource();
            BecomeGettingLocations();
        }
        
        protected override void PostStop()
        {
            _cancelable?.Cancel(false);
            _cancelToken?.Cancel(false);
            _cancelToken?.Dispose();
            base.PostStop();
        }

        protected override void PreStart()
        {
            _locations = new Dictionary<string, LocationModel>();
            _subscriptionsToObjects = new Dictionary<IActorRef, ObjectSubscription>();
            _cancelToken = new CancellationTokenSource();
        }

        private void GetLocations()
        {
            var self = Self;
            Task.Run(() =>
                {

                    var locations = new List<LocationModel>();
                    try
                    {
                        locations = _fileProcessorRepository.GetLocationsWithFileSettings().ToList();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error getting locations.");
                    }
                    return new LocationsFromDatabase(locations);

                }, _cancelToken.Token).ContinueWith(x =>
                    {
                        switch (x.Status)
                        {
                            case TaskStatus.RanToCompletion:
                                _logger.Info("Successfully checked for location.");
                                break;
                            case TaskStatus.Canceled:
                                _logger.Error(x.Exception, "Task was canceled.");
                                break;
                            case TaskStatus.Faulted:
                                _logger.Error(x.Exception, "Task faulted.");
                                break;
                        }

                        return x.Result;

                    }, TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                .PipeTo(self);

        }


        private void BecomeGettingLocations()
        {
            Self.Tell(new GetLocations(), Self);
            Become(WaitingForLocations);
        }

        private void WaitingForLocations()
        {
            Receive<SubscribeToObjectChanges>(s =>
            {
                Stash.Stash();
            });

            Receive<UnSubscribeToObjectChanges>(s =>
            {
                Stash.Stash();
            });

            Receive<GetLocations>(f =>
            {
                LogToEverything(Context, "Getting Locations.");
                GetLocations();
            });

            Receive<LocationsFromDatabase>(f => !f.Locations.Any(), f =>
            {
                LogToEverything(Context, "No locations Found.");
                var locationsToRemove = _locations.GroupJoin(f.Locations, o => o.Value.Id, i => i.Id, (x, y) => new { x.Value.Name, Location = y.Select(s => s.Name).SingleOrDefault() }).Where(x => x.Location == null).Select(x => x.Name).ToList();
                foreach (var location in locationsToRemove)
                {
                    _locations.Remove(location);
                    LogToEverything(Context, $"{location} location was removed from cache.");

                    // Send subscribers that we removed a location
                    foreach (var subscriber in _subscriptionsToObjects.Where(x => x.Value.Name == "*"))
                    {
                        subscriber.Key.Tell(new RemoveLocation(location));
                    }
                }
                _cancelable = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromSeconds(30), Self, new GetLocations(), Self);
            });

            Receive<LocationsFromDatabase>(f =>
            {
                // TODO: Validation
                foreach (var location in f.Locations)
                {
                    if (!_locations.ContainsKey(location.Name))
                    {
                        _locations.Add(location.Name, location);
                        LogToEverything(Context, $"{location.Name} location added to cache.");

                        // Send subscribers that we added a new location
                        foreach (var subscriber in _subscriptionsToObjects.Where(x => x.Value.Name == "*"))
                        {
                            subscriber.Key.Tell(new AddLocation(location.Id, location.Name));
                        }
                    }
                    else
                    {
                        var objectChanged = _locations[location.Name].FileSettings.DeepCompare(location.FileSettings);
                        if (!objectChanged)
                        {
                            _locations[location.Name] = location;
                            _logger.Info($"{location.Name} location cache was updated.");

                            // Send subscribers that we updated a location
                            foreach (var subscriber in _subscriptionsToObjects.Where(x => x.Value.Name == "*"))
                            {
                                subscriber.Key.Tell(new UpdateLocation(location.Id, location.Name));
                            }

                            foreach (var subscriber in _subscriptionsToObjects.Where(x => x.Value.Name == location.Name))
                            {
                                var value = location.GetPropertyValue(subscriber.Value.ObjectPath);

                                subscriber.Key.Tell(new ObjectChanged(subscriber.Value.ObjectPath, value));
                            }
                        }
                    }

                }

                var locationsToRemove = _locations.GroupJoin(f.Locations, o => o.Value.Id, i => i.Id, (x, y) => new {x.Value.Name, Location = y.Select(s => s.Name).SingleOrDefault()}).Where(x => x.Location == null).Select(x => x.Name).ToList();
                foreach (var location in locationsToRemove)
                {
                    _locations.Remove(location);
                    LogToEverything(Context, $"{location} location was removed from cache.");

                    // Send subscribers that we removed a location
                    foreach (var subscriber in _subscriptionsToObjects.Where(x => x.Value.Name == "*"))
                    {
                        subscriber.Key.Tell(new RemoveLocation(location));
                    }
                }


                Become(WaitingForSuscriptions);
                Stash.UnstashAll();
                _cancelable = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromSeconds(30), Self, new GetLocations(), Self);
            });

            ReceiveAny(task =>
            {
                _logger.Error(" [x] Oh Snap! Unhandled message: \r\n{0}", task);
            });
        }
        
        private void WaitingForSuscriptions()
        {
            Receive<GetLocations>(f =>
            {
                LogToEverything(Context, "Refereshing Locations.");
                BecomeGettingLocations();
            });

            Receive<UnSubscribeToObjectChanges>(s =>
            {
                if (_subscriptionsToObjects.ContainsKey(s.Requestor))
                {
                    _subscriptionsToObjects.Remove(s.Requestor);
                    LogToEverything(Context, $"Subscriber has been removed: {s.Requestor}");
                }
            });
            

            Receive<SubscribeToObjectChanges>(s =>
            {
                if (!_subscriptionsToObjects.ContainsKey(s.Requestor))
                {
                    _subscriptionsToObjects.Add(s.Requestor, new ObjectSubscription(s.Location, s.ObjectToSubscribeTo));
                    LogToEverything(Context, $"Subscriber To Objects Added: {s.Requestor}");

                    foreach (var location in _locations.Where(x => x.Key == s.Location))
                    {
                        var value = location.Value.GetPropertyValue(s.ObjectToSubscribeTo);

                        Sender.Tell(new ObjectChanged(s.ObjectToSubscribeTo, value));
                    }
                }
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

