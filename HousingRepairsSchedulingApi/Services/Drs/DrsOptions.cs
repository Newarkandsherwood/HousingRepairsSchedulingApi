namespace HousingRepairsSchedulingApi.Services.Drs
{
    using System;

    public class DrsOptions
    {
        public Uri ApiAddress { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int SearchTimeSpanInDays { get; set; } = 14;
        public int AppointmentLeadTimeInDays { get; set; } = 7;
        public int MaximumNumberOfRequests { get; set; } = 10;
        public string Contract { get; set; }
        public string Priority { get; set; }
    }
}
