
namespace MessengerRando.RO
{
    public class LocationRO
    {
        public string LocationName { get; private set; }
        public string PrettyLocationName { get; private set; } //Can be same as LocationName, used for logging and spoiler log
        public EItems VanillaItem;

        public LocationRO(string name, string prettyName, EItems item = EItems.NONE)
        {
            LocationName = name;
            PrettyLocationName = prettyName;
            VanillaItem = item;
        }

        public LocationRO(string name) : this(name, name){}

        //overrides for archipelago since logic is already handled
        public LocationRO(string name, EItems item) : this(name, item.ToString(), item){}

        public override bool Equals(object obj)
        {
            return obj is LocationRO rO &&
                   LocationName == rO.LocationName;
        }

        public bool Equals(string locName)
        {
            return LocationName == locName || PrettyLocationName == locName;
        }

        public override int GetHashCode()
        {
            return 771690509 + LocationName.GetHashCode();
        }
    }
}
