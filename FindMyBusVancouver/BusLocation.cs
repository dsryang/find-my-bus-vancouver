namespace FindMyBusVancouver
{
    public class BusLocation
    {
        public string VehicleNo { get; set; }
        public int TripId { get; set; }
        public string RouteNo { get; set; }
        public string Direction { get; set; }
        public string Destination { get; set; }
        public string Pattern { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string RecordedTime { get; set; }
    }
}
