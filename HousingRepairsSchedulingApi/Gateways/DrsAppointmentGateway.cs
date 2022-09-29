namespace HousingRepairsSchedulingApi.Gateways
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Domain;
    using Services.Drs;

    public class DrsAppointmentGateway : IAppointmentsGateway
    {
        private readonly int requiredNumberOfAppointmentDays;
        private readonly int appointmentSearchTimeSpanInDays;
        private readonly int appointmentLeadTimeInDays;
        private readonly int maximumNumberOfRequests;
        private readonly IDrsService drsService;

        public DrsAppointmentGateway(IDrsService drsService, int requiredNumberOfAppointmentDays,
            int appointmentSearchTimeSpanInDays, int appointmentLeadTimeInDays, int maximumNumberOfRequests)
        {
            Guard.Against.Null(drsService, nameof(drsService));
            Guard.Against.NegativeOrZero(requiredNumberOfAppointmentDays, nameof(requiredNumberOfAppointmentDays));
            Guard.Against.NegativeOrZero(appointmentSearchTimeSpanInDays, nameof(appointmentSearchTimeSpanInDays));
            Guard.Against.Negative(appointmentLeadTimeInDays, nameof(appointmentLeadTimeInDays));
            Guard.Against.NegativeOrZero(maximumNumberOfRequests, nameof(maximumNumberOfRequests));

            this.drsService = drsService;
            this.requiredNumberOfAppointmentDays = requiredNumberOfAppointmentDays;
            this.appointmentSearchTimeSpanInDays = appointmentSearchTimeSpanInDays;
            this.appointmentLeadTimeInDays = appointmentLeadTimeInDays;
            this.maximumNumberOfRequests = maximumNumberOfRequests;
        }

        public async Task<IEnumerable<AppointmentSlot>> GetAvailableAppointments(string sorCode, string locationId,
            DateTime? fromDate = null)
        {
            Guard.Against.NullOrWhiteSpace(sorCode, nameof(sorCode));
            Guard.Against.NullOrWhiteSpace(locationId, nameof(locationId));

            var desiredAppointmentSlots = new []
            {
                new { StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(12, 0, 0) },
                new { StartTime = new TimeSpan(12, 0, 0), EndTime = new TimeSpan(16, 0, 0) },
                new { StartTime = new TimeSpan(7, 0, 0), EndTime = new TimeSpan(11, 0, 0) },
                new { StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(15, 0, 0) },
            };

            var earliestDate = fromDate ?? DateTime.Today.AddDays(appointmentLeadTimeInDays);
            var appointmentSlots = Enumerable.Empty<AppointmentSlot>();

            var numberOfRequests = 0;
            while (numberOfRequests < maximumNumberOfRequests && appointmentSlots.Select(x => x.StartTime.Date).Distinct().Count() < requiredNumberOfAppointmentDays)
            {
                numberOfRequests++;
                var appointments = await drsService.CheckAvailability(sorCode, locationId, earliestDate);
                appointments = appointments.Where(x => desiredAppointmentSlots.Any(slot =>
                    slot.StartTime == x.StartTime.TimeOfDay && slot.EndTime == x.EndTime.TimeOfDay)
                );
                appointmentSlots = appointmentSlots.Concat(appointments);
                earliestDate = earliestDate.AddDays(appointmentSearchTimeSpanInDays);
            }

            appointmentSlots = appointmentSlots.GroupBy(x => x.StartTime.Date).Take(requiredNumberOfAppointmentDays)
                .SelectMany(x => x.Select(y => y));

            return appointmentSlots;
        }

        public async Task<string> BookAppointment(string bookingReference, string sorCode, string locationId,
            DateTime startDateTime, DateTime endDateTime)
        {
            Guard.Against.NullOrWhiteSpace(bookingReference, nameof(bookingReference));
            Guard.Against.NullOrWhiteSpace(sorCode, nameof(sorCode));
            Guard.Against.NullOrWhiteSpace(locationId, nameof(locationId));
            Guard.Against.OutOfRange(endDateTime, nameof(endDateTime), startDateTime, DateTime.MaxValue);

            var bookingId = await drsService.CreateOrder(bookingReference, sorCode, locationId);

            await drsService.ScheduleBooking(bookingReference, bookingId, startDateTime, endDateTime);

            return bookingReference;
        }
    }
}
