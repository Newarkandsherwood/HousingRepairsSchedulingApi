namespace HousingRepairsSchedulingApi.Tests.UseCasesTests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Gateways;
using Moq;
using UseCases;
using Xunit;

public class CancelAppointmentUseCaseTests
{
    private const string BookingReference = "BookingReference";

    private CancelAppointmentUseCase systemUnderTest;
    private Mock<IAppointmentsGateway> appointmentsGatewayMock;

    public CancelAppointmentUseCaseTests()
    {
        appointmentsGatewayMock = new Mock<IAppointmentsGateway>();
        systemUnderTest = new CancelAppointmentUseCase(appointmentsGatewayMock.Object);
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
        var act = async () => await systemUnderTest.Execute(bookingReference);

        // Assert
        await act.Should().ThrowExactlyAsync<T>();
    }

    public static IEnumerable<object[]> InvalidArgumentTestData()
    {
        yield return new object[] { new ArgumentNullException(), null };
        yield return new object[] { new ArgumentException(), "" };
        yield return new object[] { new ArgumentException(), " " };
    }

    [Fact]
#pragma warning disable CA1707
    public async void GivenValidArguments_WhenExecute_ThenBookingReferenceIsReturned()
#pragma warning restore CA1707
    {
        // Arrange
        appointmentsGatewayMock.Setup(x => x.CancelAppointment(
            It.IsAny<string>()
        )).ReturnsAsync(BookingReference);

        // Act
        var actual = await systemUnderTest.Execute(BookingReference);

        // Assert
        Assert.Equal(BookingReference, actual);
    }

    [Fact]
#pragma warning disable CA1707
    public async void GivenValidArguments_WhenExecute_ThenAppointmentGatewayUpdateAppointmentIsCalled()
#pragma warning restore CA1707
    {
        // Arrange
        appointmentsGatewayMock.Setup(x => x.CancelAppointment(
            It.IsAny<string>()
        )).ReturnsAsync(BookingReference);

        // Act
        _ = await systemUnderTest.Execute(BookingReference);

        // Assert
        appointmentsGatewayMock.Verify(x => x.CancelAppointment(BookingReference), Times.Once);
    }
}
