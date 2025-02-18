namespace HousingRepairsSchedulingApi.Gateways
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Domain;
    using Helpers;
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

        public async Task<IEnumerable<AppointmentSlot>> GetAvailableAppointments(string sorCode, string priority, string locationId,
            DateTime? fromDate = null, IEnumerable<AppointmentSlotTimeSpan> allowedAppointmentSlots = default)
        {
            Guard.Against.NullOrWhiteSpace(sorCode, nameof(sorCode));
            Guard.Against.NullOrWhiteSpace(priority, nameof(priority));
            Guard.Against.NullOrWhiteSpace(locationId, nameof(locationId));

            var desiredAppointmentSlots = allowedAppointmentSlots;

            if (allowedAppointmentSlots != null)
            {
                var dayLightSavingsTimeAdjustedDesiredAppointmentSlots = allowedAppointmentSlots.Select(x =>
                    new AppointmentSlotTimeSpan
                    {
                        StartTime = x.StartTime.Add(TimeSpan.FromHours(-1)),
                        EndTime = x.EndTime.Add(TimeSpan.FromHours(-1))
                    });
                desiredAppointmentSlots =
                    desiredAppointmentSlots.Concat(dayLightSavingsTimeAdjustedDesiredAppointmentSlots);
            }

            var earliestDate = fromDate ?? DateTime.Today.AddDays(appointmentLeadTimeInDays);
            var appointmentSlots = Enumerable.Empty<AppointmentSlot>();

            var numberOfRequests = 0;
            while (numberOfRequests < maximumNumberOfRequests && appointmentSlots.Select(x => x.StartTime.Date).Distinct().Count() < requiredNumberOfAppointmentDays)
            {
                numberOfRequests++;
                var appointments = await drsService.CheckAvailability(sorCode, priority, locationId, earliestDate);
                appointments = appointments.Where(x => desiredAppointmentSlots == null || desiredAppointmentSlots.Any(slot =>
                    slot.StartTime == x.StartTime.TimeOfDay && slot.EndTime == x.EndTime.TimeOfDay)
                );
                appointmentSlots = appointmentSlots.Concat(appointments);
                earliestDate = earliestDate.AddDays(appointmentSearchTimeSpanInDays);
            }

            appointmentSlots = appointmentSlots.GroupBy(x => x.StartTime.Date).Take(requiredNumberOfAppointmentDays)
                .SelectMany(x => x.Select(y => y));

            return appointmentSlots;
        }

        public async Task<string> BookAppointment(string bookingReference, string sorCode, string priority, string locationId,
            DateTime startDateTime, DateTime endDateTime, string orderComments)
        {
            Guard.Against.NullOrWhiteSpace(bookingReference, nameof(bookingReference));
            Guard.Against.NullOrWhiteSpace(sorCode, nameof(sorCode));
            Guard.Against.NullOrWhiteSpace(priority, nameof(priority));
            Guard.Against.NullOrWhiteSpace(locationId, nameof(locationId));
            Guard.Against.NullOrWhiteSpace(orderComments, nameof(orderComments));
            Guard.Against.OutOfRange(endDateTime, nameof(endDateTime), startDateTime, DateTime.MaxValue);

            var bookingId = await drsService.CreateOrder(bookingReference, sorCode, priority, locationId, orderComments);

            await ScheduleBooking(bookingReference, bookingId, startDateTime, endDateTime);

            return bookingReference;
        }

        public async Task<string> UpdateAppointment(string bookingReference, DateTime startDateTime, DateTime endDateTime)
        {
            Guard.Against.NullOrWhiteSpace(bookingReference, nameof(bookingReference));
            Guard.Against.OutOfRange(endDateTime, nameof(endDateTime), startDateTime, DateTime.MaxValue);

            var order = await drsService.SelectOrder(bookingReference);
            var bookingId = order.theBookings.First().bookingId;

            await ScheduleBooking(bookingReference, bookingId, startDateTime, endDateTime);

            return bookingReference;
        }

        public async Task<string> CancelAppointment(string bookingReference)
        {
            Guard.Against.NullOrWhiteSpace(bookingReference, nameof(bookingReference));

            var order = await drsService.SelectOrder(bookingReference);
            var bookingId = order.theBookings.First().bookingId;

            await drsService.DeleteBooking(bookingReference, bookingId);

            return bookingReference;
        }

        private async Task ScheduleBooking(string bookingReference, int bookingId, DateTime startDateTime, DateTime endDateTime)
        {
            var convertedStartTime = DrsHelpers.ConvertToDrsTimeZone(startDateTime);
            var convertedEndTime = DrsHelpers.ConvertToDrsTimeZone(endDateTime);
            await drsService.ScheduleBooking(bookingReference, bookingId, convertedStartTime, convertedEndTime);
        }
    }
}
