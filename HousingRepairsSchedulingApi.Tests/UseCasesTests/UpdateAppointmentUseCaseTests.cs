namespace HousingRepairsSchedulingApi.Tests.UseCasesTests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Gateways;
using Moq;
using UseCases;
using Xunit;

public class UpdateAppointmentUseCaseTests
{
    private const string BookingReference = "BookingReference";

    private UpdateAppointmentUseCase systemUnderTest;
    private Mock<IAppointmentsGateway> appointmentsGatewayMock;

    public UpdateAppointmentUseCaseTests()
    {
        appointmentsGatewayMock = new Mock<IAppointmentsGateway>();
        systemUnderTest = new UpdateAppointmentUseCase(appointmentsGatewayMock.Object);
    }

    [Theory]
    [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
    public async void GivenAnInvalidBookingReference_WhenExecute_ThenExceptionIsThrown<T>(T exception,
        string bookingReference) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
    {
        // Arrange

        // Act
        Func<Task> act = async () =>
            await systemUnderTest.Execute(bookingReference, It.IsAny<DateTime>(), It.IsAny<DateTime>());

        // Assert
        await act.Should().ThrowExactlyAsync<T>();
    }

    [Fact]
#pragma warning disable CA1707
    public async void GivenAnEndDateEarlierThanTheStartDate_WhenExecute_ThenInvalidExceptionIsThrown()
#pragma warning restore CA1707
    {
        // Arrange
        var startDate = new DateTime(2022, 1, 21);
        var endDate = startDate.AddDays(-1);

        // Act
        Func<Task> act = async () =>
            await systemUnderTest.Execute(BookingReference, startDate, endDate);

        // Assert
        await act.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
    }

    public static IEnumerable<object[]> InvalidArgumentTestData()
    {
        yield return new object[] { new ArgumentNullException(), null };
        yield return new object[] { new ArgumentException(), "" };
        yield return new object[] { new ArgumentException(), " " };
    }

    [Fact]
#pragma warning disable CA1707
    public async void GivenValidArguments_WhenExecute_ThenBookingIdIsReturned()
#pragma warning restore CA1707
    {
        // Arrange
        appointmentsGatewayMock.Setup(x => x.UpdateAppointment(
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>()
        )).ReturnsAsync(BookingReference);

        // Act
        var startDateTime = It.IsAny<DateTime>();
        var actual = await systemUnderTest.Execute(BookingReference, startDateTime, startDateTime.AddDays(1));

        // Assert
        Assert.Equal(BookingReference, actual);
    }

    [Fact]
#pragma warning disable CA1707
    public async void GivenValidArguments_WhenExecute_ThenAppointmentGatewayUpdateAppointmentIsCalled()
#pragma warning restore CA1707
    {
        // Arrange
        appointmentsGatewayMock.Setup(x => x.UpdateAppointment(
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>()
        )).ReturnsAsync(BookingReference);

        var startDateTime = It.IsAny<DateTime>();
        var endDateTime = startDateTime.AddDays(1);

        // Act
        _ = await systemUnderTest.Execute(BookingReference, startDateTime, endDateTime);

        // Assert
        appointmentsGatewayMock.Verify(x => x.UpdateAppointment(BookingReference, startDateTime, endDateTime), Times.Once);
    }
}
