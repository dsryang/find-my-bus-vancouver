namespace FindMyBusVancouver
{
    public class StopSearchResult
    {
        public int StopNo { get; set; }
        public string Name { get; set; }
        public string BayNo { get; set; }
        public string City { get; set; }
        public string OnStreet { get; set; }
        public string AtStreet { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int WheelchairAccess { get; set; }
        public int Distance { get; set; }
        public string Routes { get; set; }
    }
}
