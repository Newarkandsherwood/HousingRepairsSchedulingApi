namespace HousingRepairsSchedulingApi.UseCases;

using System.Threading.Tasks;

public interface ICancelAppointmentUseCase
{
    public Task<CancelAppointmentUseCaseResult> Execute(string bookingReference);
}
