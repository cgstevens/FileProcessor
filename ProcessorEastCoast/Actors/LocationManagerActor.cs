using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using SharedLibrary.Actors;
using SharedLibrary.Messages;

namespace ProcessorEastCoast.Actors
{
    public class LocationManagerActor : ReceiveActor
    {
        private readonly ILoggingAdapter _logger;
        private readonly Dictionary<string, IActorRef> _locations;

        public LocationManagerActor()
        {
            _locations = new Dictionary<string, IActorRef>();
            _logger = Context.GetLogger();
            BecomeStartup();
        }

        private void BecomeStartup()
        {
            var self = Self;
            SystemActors.SettingsWatcherRef.Tell(new SubscribeToObjectChanges(self, "*", "*"));
            Become(Startup);
        }

        private void Startup()
        {
            Receive<AddLocation>(t =>
            {
                var self = Self;
                AddNewLocation(t.Id, t.Name, self);
            });

            Receive<RemoveLocation>(t =>
            {
                IActorRef removeLocation;
                var locationExists = _locations.TryGetValue(t.Name, out removeLocation);
                if (locationExists)
                {
                    _locations.Remove(t.Name);
                    removeLocation.Tell(PoisonPill.Instance);
                    _logger.Info($"{t.Name} has been removed and Actor was sent a PoisonPill.");
                }
            });

            Receive<UpdateLocation>(t =>
            {
                var self = Self;
                IActorRef updateLocation;
                var locationExists = _locations.TryGetValue(t.Name, out updateLocation);
                if (locationExists)
                {
                    _logger.Info($"{t.Name} has updated.");
                }
                else
                {
                    AddNewLocation(t.Id, t.Name, self);
                }

            });

            ReceiveAny(task =>
            {
                _logger.Error(" [x] Oh Snap! Unhandled message: \r\n{0}", task);
            });
        }

        private void AddNewLocation(int Id, string Name, IActorRef self)
        {
            if (!_locations.ContainsKey(Name))
            {
                var newLocation = Context.ActorOf(Props.Create(() => new LocationActor(self, Name)), Name);
                _locations.Add(Name, newLocation);
                _logger.Info($"{Name} was added.");
            }
        }
    }
}

