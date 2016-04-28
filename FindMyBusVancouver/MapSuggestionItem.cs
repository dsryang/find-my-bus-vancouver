using Windows.Devices.Geolocation;

namespace FindMyBusVancouver
{
    public class MapSuggestionItem
    {
        public string name { get; set; }
        public Geopoint point { get; set; }
    }
}
