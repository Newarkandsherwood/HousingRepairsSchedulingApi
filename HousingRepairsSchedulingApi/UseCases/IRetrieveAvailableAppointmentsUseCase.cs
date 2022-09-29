namespace HousingRepairsSchedulingApi.UseCases
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;
    using HACT.Dtos;

    public interface IRetrieveAvailableAppointmentsUseCase
    {
        public Task<IEnumerable<Appointment>> Execute(string sorCode, string locationId, DateTime? fromDate, IEnumerable<AppointmentSlotTimeSpan> allowedAppointmentSlots = default);
    }
}
