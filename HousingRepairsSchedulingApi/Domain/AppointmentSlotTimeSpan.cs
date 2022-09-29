namespace HousingRepairsSchedulingApi.Domain;

using System;

public class AppointmentSlotTimeSpan
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
