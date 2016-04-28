namespace FindMyBusVancouver
{
    public class StopEstimate
    {
        public string RouteNo { get; set; }
        public string RouteName { get; set; }
        public string Direction { get; set; }
        public StopSchedule[] Schedules { get; set; }
    }
}
