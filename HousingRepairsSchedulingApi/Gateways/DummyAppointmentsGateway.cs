namespace HousingRepairsSchedulingApi.Gateways
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain;

    public class DummyAppointmentsGateway : IAppointmentsGateway
    {
        public async Task<IEnumerable<AppointmentSlot>> GetAvailableAppointments(string sorCode, string priority, string locationId, DateTime? fromDate = null, IEnumerable<AppointmentSlotTimeSpan> allowedAppointmentSlots = default)
        {
            var dateTime = (fromDate ?? DateTime.Today).Date;
            List<AppointmentSlot> unorderedAppointments = new List<AppointmentSlot>
            {
                new ()
                {
                    StartTime = dateTime.AddDays(16).AddHours(8),
                    EndTime= dateTime.AddDays(16).AddHours(12)
                },
                new()
                {
                    StartTime = dateTime.AddDays(20).AddDays(2).AddHours(12),
                    EndTime = dateTime.AddDays(20).AddDays(2).AddHours(16)
                },
                new()
                {
                    StartTime = dateTime.AddDays(7).AddDays(7).AddHours(8),
                    EndTime = dateTime.AddDays(7).AddDays(7).AddHours(12)
                },
                new ()
                {
                    StartTime = dateTime.AddDays(1).AddDays(1).AddHours(8),
                    EndTime = dateTime.AddDays(1).AddDays(1).AddHours(12)
                },
                new ()
                {
                    StartTime = dateTime.AddDays(5).AddDays(5).AddHours(12),
                    EndTime = dateTime.AddDays(5).AddDays(5).AddHours(16)
                }
            };
            var orderedAppointments = unorderedAppointments.OrderBy(x => x.StartTime);
            return orderedAppointments;
        }

        public Task<string> BookAppointment(string bookingReference, string sorCode, string priority, string locationId, DateTime startDateTime,
            DateTime endDateTime, string orderComments) =>
            throw new NotImplementedException();

        public Task<string> UpdateAppointment(string bookingReference, DateTime startDateTime, DateTime endDateTime) => throw new NotImplementedException();

        public Task<string> CancelAppointment(string bookingReference) => throw new NotImplementedException();
    }
}
