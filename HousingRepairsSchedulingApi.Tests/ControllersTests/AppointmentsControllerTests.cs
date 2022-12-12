using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace HousingRepairsSchedulingApi.Tests.ControllersTests
{
    using System;
    using Controllers;
    using Domain;
    using UseCases;

    public class AppointmentsControllerTests : ControllerTests
    {
        private const string SorCode = "SOR Code";
        private const string Priority = "Priority";
        private const string LocationId = "locationId";
        private RepairDescription orderComments = new RepairDescription { Text = "something" };
        private AppointmentsController systemUndertest;
        private Mock<IRetrieveAvailableAppointmentsUseCase> availableAppointmentsUseCaseMock;
        private Mock<IBookAppointmentUseCase> bookAppointmentUseCaseMock;

        public AppointmentsControllerTests()
        {
            availableAppointmentsUseCaseMock = new Mock<IRetrieveAvailableAppointmentsUseCase>();
            bookAppointmentUseCaseMock = new Mock<IBookAppointmentUseCase>();
            systemUndertest = new AppointmentsController(
                availableAppointmentsUseCaseMock.Object,
                bookAppointmentUseCaseMock.Object);
        }

        [Fact]
        public async Task TestAvailableAppointmentsEndpoint()
        {
            var result = await systemUndertest.AvailableAppointments(SorCode, Priority, LocationId);
            GetStatusCode(result).Should().Be(200);
            availableAppointmentsUseCaseMock.Verify(x => x.Execute(SorCode, Priority, LocationId, null, null), Times.Once);
        }


        [Fact]
        public async Task ReturnsErrorWhenFailsToGetAvailableAppointments()
        {

            const string errorMessage = "An error message";
            availableAppointmentsUseCaseMock.Setup(x => x.Execute(It.IsAny<String>(), Priority, It.IsAny<String>(), null, null))
                .Throws(new Exception(errorMessage));

            var result = await systemUndertest.AvailableAppointments(SorCode, Priority, LocationId);

            GetStatusCode(result).Should().Be(500);
            GetResultData<string>(result).Should().Be(errorMessage);
        }

        [Fact]
        public async Task TestBookAppointmentEndpoint()
        {
            const string bookingReference = "bookingReference";
            var startDateTime = It.IsAny<DateTime>();
            var endDateTime = It.IsAny<DateTime>();

            var result = await systemUndertest.BookAppointment(bookingReference, SorCode, Priority, LocationId, startDateTime, endDateTime, orderComments);
            GetStatusCode(result).Should().Be(200);
        }

        [Fact]
#pragma warning disable CA1707
        public async Task GivenAFromDate_WhenRequestingAvailableAppointment_ThenResultsAreReturned()
#pragma warning restore CA1707
        {
            // Arrange
            const string sorCode = "sorCode";
            const string locationId = "locationId";
            var fromDate = new DateTime(2021, 12, 15);

            // Act
            var result = await systemUndertest.AvailableAppointments(sorCode, Priority, locationId, fromDate);

            // Assert
            GetStatusCode(result).Should().Be(200);
            availableAppointmentsUseCaseMock.Verify(x => x.Execute(sorCode, Priority, locationId, fromDate, null), Times.Once);
        }

        [Fact]
        public async Task ReturnsErrorWhenFailsToBookAppointments()
        {
            const string bookingReference = "bookingReference";
            var startDateTime = It.IsAny<DateTime>();
            var endDateTime = It.IsAny<DateTime>();

            const string errorMessage = "An error message";
            bookAppointmentUseCaseMock.Setup(x => x.Execute(It.IsAny<String>(), It.IsAny<String>(), Priority, It.IsAny<String>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>())).Throws(new Exception(errorMessage));

            var result = await systemUndertest.BookAppointment(bookingReference, SorCode, Priority, LocationId, startDateTime, endDateTime, orderComments);

            GetStatusCode(result).Should().Be(500);
            GetResultData<string>(result).Should().Be(errorMessage);
        }

        [Fact]
#pragma warning disable CA1707
        public async Task GivenNonCancelledAppointmentBookingReference_WhenCancellingAppointment_ThenResponseHasStatus200()
#pragma warning restore CA1707
        {
            // Arrange
            var bookingReference = "bookingReference";

            // Act
            var result = await systemUndertest.CancelAppointment(bookingReference);

            // Assert
            GetStatusCode(result).Should().Be(200);
        }

        [Fact]
#pragma warning disable CA1707
        public async Task GivenCancelledAppointmentBookingReference_WhenCancellingAppointment_ThenResponseHasStatus200()
#pragma warning restore CA1707
        {
            // Arrange
            var cancelledBookingReference = "cancelledBookingReference";

            // Act
            var result = await systemUndertest.CancelAppointment(cancelledBookingReference);

            // Assert
            GetStatusCode(result).Should().Be(200);
        }

        [Fact]
#pragma warning disable CA1707
        public async Task GivenAppointmentIsNotFound_WhenCancellingAppointment_ThenResponseHasStatus404()
#pragma warning restore CA1707
        {
            // Arrange
            var noAppointmentBookingReference = "noAppointmentBookingReference";

            // Act
            var result = await systemUndertest.CancelAppointment(noAppointmentBookingReference);

            // Assert
            GetStatusCode(result).Should().Be(404);
        }

        [Fact]
#pragma warning disable CA1707
        public async Task GivenAppointmentWasNotCancelled_WhenCancellingAppointment_ThenResponseHasStatus500()
#pragma warning restore CA1707
        {
            // Arrange
            var unableToCancelAppointmentBookingReference = "unableToCancelAppointmentBookingReference";

            // Act
            var result = await systemUndertest.CancelAppointment(unableToCancelAppointmentBookingReference);

            // Assert
            GetStatusCode(result).Should().Be(500);
        }

        [Fact]
#pragma warning disable CA1707
        public async Task GivenNonCancelledAppointmentBookingReference_WhenUpdatingAppointment_ThenResponseHasStatus200()
#pragma warning restore CA1707
        {
            // Arrange
            var bookingReference = "bookingReference";

            // Act
            var result = await systemUndertest.UpdateAppointmentSlot(bookingReference, new DateTime(), new DateTime());

            // Assert
            GetStatusCode(result).Should().Be(200);
        }

        [Fact]
#pragma warning disable CA1707
        public async Task GivenNotExisingAppointmentBookingReference_WhenUpdatingAppointment_ThenResponseHasStatus404()
#pragma warning restore CA1707
        {
            // Arrange
            var bookingReference = "noAppointmentBookingReference";

            // Act
            var result = await systemUndertest.UpdateAppointmentSlot(bookingReference, new DateTime(), new DateTime());

            // Assert
            GetStatusCode(result).Should().Be(404);
        }

        [Fact]
#pragma warning disable CA1707
        public async Task GivenErrorAppointmentBookingReference_WhenUpdatingAppointment_ThenResponseHasStatus500()
#pragma warning restore CA1707
        {
            // Arrange
            var bookingReference = "UnableToUpdateAppointmentSlot";

            // Act
            var result = await systemUndertest.UpdateAppointmentSlot(bookingReference, new DateTime(), new DateTime());

            // Assert
            GetStatusCode(result).Should().Be(500);
        }
    }
}
