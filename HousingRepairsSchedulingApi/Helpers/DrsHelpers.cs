using System;
using NodaTime;

namespace HousingRepairsSchedulingApi.Helpers
{
    public static class DrsHelpers
    {
        private static DateTimeZone LondonTimeZone => DateTimeZoneProviders.Tzdb["Europe/London"];

        private static DateTime DateTimeToUtcDateTime(DateTime dateTime) => new(dateTime.Ticks, DateTimeKind.Utc);

        public static DateTime ConvertToDrsTimeZone(DateTime dateTime)
        {
            var utcDateTime = DateTimeToUtcDateTime(dateTime);
            var local = Instant.FromDateTimeUtc(utcDateTime).InUtc();
            return local.WithZone(LondonTimeZone).ToDateTimeUnspecified();
        }

        public static DateTime ConvertFromDrsTimeZone(DateTime dateTime)
        {
            var utcDateTime = DateTimeToUtcDateTime(dateTime);
            return dateTime - LondonTimeZone.GetUtcOffset(Instant.FromDateTimeUtc(utcDateTime)).ToTimeSpan();
        }
    }
}
