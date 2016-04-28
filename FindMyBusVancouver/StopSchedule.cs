namespace FindMyBusVancouver
{
    public class StopSchedule
    {
        public string Pattern { get; set; }
        public string Destination { get; set; }
        public string ExpectedLeaveTime { get; set; }
        public int ExpectedCountdown { get; set; }
        public string ScheduleStatus { get; set; }
        public bool CancelledTrip { get; set; }
        public bool CancelledStop { get; set; }
        public bool AddedTrip { get; set; }
        public bool AddedStop { get; set; }
        public string LastUpdate { get; set; }
    }
}