namespace HousingRepairsSchedulingApi.UseCases;

using System;
using System.Threading.Tasks;

public interface IUpdateAppointmentUseCase
{
    public Task<string> Execute(string bookingReference, DateTime startDateTime, DateTime endDateTime);
}
