namespace HousingRepairsSchedulingApi.Tests.UseCasesTests;

using System;
using Domain;
using FluentAssertions;
using Gateways;
using Moq;
using UseCases;
using Xunit;

public class CancelAppointmentUseCaseTests
{
    private readonly CancelAppointmentUseCase systemUnderTest;

    private readonly Mock<IAppointmentsGateway> appointmentsGatewayMock = new();

    public CancelAppointmentUseCaseTests()
    {
        systemUnderTest = new(appointmentsGatewayMock.Object);
    }

    [Theory]
    [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable CA1707
    public async void GivenInvalidBookingReference_WhenExecuting_ThenArgumentExceptionIsThrown<T>(T exception, string bookingReference) where T:Exception
#pragma warning restore CA1707
    {
        // Arrange

        // Act
        var act = async () => await systemUnderTest.Execute(bookingReference);

        // Assert
        await act.Should().ThrowExactlyAsync<T>();
    }

    public static TheoryData<ArgumentException, string> InvalidArgumentTestData() =>
        new()
        {
            { new ArgumentNullException(), null },
            { new ArgumentException(), "" },
            { new ArgumentException(), " " },
        };

    [Fact]
    public async void GivenValidBookingReference_WhenExecuting_TheGatewayIsCalled()
    {
        // Arrange
        var bookingReference = "bookingReference";

        // Act
        _ = await systemUnderTest.Execute(bookingReference);

        // Assert
        appointmentsGatewayMock.Verify(x => x.CancelAppointment(bookingReference));
    }

    [Fact]
    public async void GivenValidBookingReference_WhenExecuting_TheGatewayResponseIsReturned()
    {
        // Arrange
        var bookingReference = "bookingReference";
        var expected = CancelAppointmentStatus.Success;
        appointmentsGatewayMock.Setup(x => x.CancelAppointment(bookingReference))
            .ReturnsAsync(expected);

        // Act
        var actual = await systemUnderTest.Execute(bookingReference);

        // Assert
        actual.Should().Be(CancelAppointmentStatus.Success);
    }
}
