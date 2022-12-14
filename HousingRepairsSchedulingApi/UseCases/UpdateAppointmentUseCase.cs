namespace HousingRepairsSchedulingApi.UseCases;

using System;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Gateways;

public class UpdateAppointmentUseCase : IUpdateAppointmentUseCase
{
    private readonly IAppointmentsGateway appointmentsGateway;

    public UpdateAppointmentUseCase(IAppointmentsGateway appointmentsGateway)
    {
        this.appointmentsGateway = appointmentsGateway;
    }

    public async Task<string> Execute(string bookingReference, DateTime startDateTime, DateTime endDateTime)
    {
        Guard.Against.NullOrWhiteSpace(bookingReference, nameof(bookingReference));
        Guard.Against.OutOfRange(endDateTime, nameof(endDateTime), startDateTime, DateTime.MaxValue);

        var result = await appointmentsGateway.UpdateAppointment(bookingReference, startDateTime, endDateTime);

        return result;
    }
}
