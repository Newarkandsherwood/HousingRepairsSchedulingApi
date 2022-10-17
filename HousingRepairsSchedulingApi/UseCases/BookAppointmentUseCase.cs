namespace HousingRepairsSchedulingApi.UseCases
{
    using System;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Gateways;

    public class BookAppointmentUseCase : IBookAppointmentUseCase
    {
        private readonly IAppointmentsGateway appointmentsGateway;

        public BookAppointmentUseCase(IAppointmentsGateway appointmentsGateway)
        {
            this.appointmentsGateway = appointmentsGateway;
        }

        public Task<string> Execute(string bookingReference, string sorCode, string priority, string locationId,
            DateTime startDateTime, DateTime endDateTime, string orderComments)
        {
            Guard.Against.NullOrWhiteSpace(bookingReference, nameof(bookingReference));
            Guard.Against.NullOrWhiteSpace(sorCode, nameof(sorCode));
            Guard.Against.NullOrWhiteSpace(priority, nameof(priority));
            Guard.Against.NullOrWhiteSpace(locationId, nameof(locationId));
            Guard.Against.NullOrWhiteSpace(orderComments, nameof(orderComments));
            Guard.Against.OutOfRange(endDateTime, nameof(endDateTime), startDateTime, DateTime.MaxValue);
            Guard.Against.OutOfRange(orderComments.Length, nameof(orderComments), 1, 255);

            var result = appointmentsGateway.BookAppointment(bookingReference, sorCode, priority, locationId,
                startDateTime, endDateTime, orderComments);

            return result;
        }
    }
}
