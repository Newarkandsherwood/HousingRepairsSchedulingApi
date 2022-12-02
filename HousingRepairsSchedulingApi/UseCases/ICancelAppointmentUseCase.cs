namespace HousingRepairsSchedulingApi.UseCases;

using System.Threading.Tasks;
using Domain;

public interface ICancelAppointmentUseCase
{
    public Task<CancelAppointmentStatus> Execute(string bookingReference);
}
