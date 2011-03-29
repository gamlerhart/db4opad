using System.Collections.Generic;
using System.Linq;

namespace Db4oPad.TestDBs
{
    public class Location
    {
        private Location[] alternativeLocations = new Location[0];
        private string locationName;

        public Location(string locationName)
        {
            this.locationName = locationName;
        }

        public IEnumerable<Location> AlternativeLocations
        {
            get { return alternativeLocations; }
        }

        public void AddAlternativeLocation(Location location)
        {
            this.alternativeLocations = alternativeLocations.Union(new[] {location}).ToArray();
        }

        public string LocationName
        {
            get { return locationName; }
            set { locationName = value; }
        }
    }
}