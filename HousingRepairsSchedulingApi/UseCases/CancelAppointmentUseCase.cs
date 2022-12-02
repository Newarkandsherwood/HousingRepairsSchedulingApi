namespace HousingRepairsSchedulingApi.UseCases;

using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Domain;
using Gateways;

public class CancelAppointmentUseCase : ICancelAppointmentUseCase
{
    private readonly IAppointmentsGateway appointmentsGateway;

    public CancelAppointmentUseCase(IAppointmentsGateway appointmentsGateway)
    {
        this.appointmentsGateway = appointmentsGateway;
    }

    public async Task<CancelAppointmentStatus> Execute(string bookingReference)
    {
        Guard.Against.NullOrWhiteSpace(bookingReference, nameof(bookingReference));

        var result = await appointmentsGateway.CancelAppointment(bookingReference);

        return result;
    }
}
