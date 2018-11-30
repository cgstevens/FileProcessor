using System.Collections.Generic;
using Shared.Models;

namespace Shared.Messages
{

    public class LocationsFromDatabase
    {
        public IEnumerable<LocationModel> Locations { get; }
        public LocationsFromDatabase(IEnumerable<LocationModel> locations)
        {
            Locations = locations;
        }
    }

    public class AddLocation
    {
        public int Id { get; }
        public string Name { get; }
        public AddLocation(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class UpdateLocation
    {
        public int Id { get; }
        public string Name { get; }
        public UpdateLocation(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class RemoveLocation
    {
        public string Name { get; }
        public RemoveLocation(string name)
        {
            Name = name;
        }
    }

    public class GetLocations
    {
    }

}
