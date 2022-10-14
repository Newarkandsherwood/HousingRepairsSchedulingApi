using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HousingRepairsSchedulingApi.Gateways
{
    using Domain;

    public interface IAppointmentsGateway
    {
        Task<IEnumerable<AppointmentSlot>> GetAvailableAppointments(string sorCode, string locationId, DateTime? fromDate = null, IEnumerable<AppointmentSlotTimeSpan> allowedAppointmentSlots = default);

        Task<string> BookAppointment(string bookingReference, string sorCode, string locationId, DateTime startDateTime, DateTime endDateTime);
    }
}
