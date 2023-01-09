namespace HousingRepairsSchedulingApi.UseCases;

using System.Threading.Tasks;

public interface ICancelAppointmentUseCase
{
    Task<string> Execute(string bookingReference);
}
