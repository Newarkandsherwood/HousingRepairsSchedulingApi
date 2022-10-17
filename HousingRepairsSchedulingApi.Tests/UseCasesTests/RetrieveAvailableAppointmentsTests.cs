using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using HousingRepairsSchedulingApi.Gateways;
using HousingRepairsSchedulingApi.UseCases;
using Moq;
using Xunit;
namespace HousingRepairsSchedulingApi.Tests.UseCasesTests
{

    public class RetrieveAvailableAppointmentsTests
    {
        private const string Priority = "Priority";

        private readonly RetrieveAvailableAppointmentsUseCase sytemUndertest;
        private readonly Mock<IAppointmentsGateway> appointmentsGatewayMock;

        public RetrieveAvailableAppointmentsTests()
        {
            appointmentsGatewayMock = new Mock<IAppointmentsGateway>();
            sytemUndertest = new RetrieveAvailableAppointmentsUseCase(appointmentsGatewayMock.Object);
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenAnInvalidSorCode_WhenExecute_ThenExceptionIsThrown<T>(T exception, string sorCode) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange
            var systemUnderTest = new RetrieveAvailableAppointmentsUseCase(appointmentsGatewayMock.Object);

            // Act
            Func<Task> act = async () => await systemUnderTest.Execute(sorCode, Priority, "locationId");

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenAnInvalidPriority_WhenExecute_ThenExceptionIsThrown<T>(T exception, string priority) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange
            var systemUnderTest = new RetrieveAvailableAppointmentsUseCase(appointmentsGatewayMock.Object);

            // Act
            Func<Task> act = async () => await systemUnderTest.Execute("sorCode", priority, "locationId");

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenAnInvalidLocationId_WhenExecute_ThenExceptionIsThrown<T>(T exception, string locationId) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange
            var systemUnderTest = new RetrieveAvailableAppointmentsUseCase(appointmentsGatewayMock.Object);

            // Act
            Func<Task> act = async () => await systemUnderTest.Execute("uprn", Priority, locationId);

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Fact]
#pragma warning disable CA1707
        public async void GivenANullFromDate_WhenExecute_ThenNoExceptionIsThrown()
#pragma warning restore CA1707
        {
            // Arrange
            var systemUnderTest = new RetrieveAvailableAppointmentsUseCase(appointmentsGatewayMock.Object);

            // Act
            Func<Task> act = async () => await systemUnderTest.Execute("SoR Code", Priority, "location Id", null);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
#pragma warning disable CA1707
        public async void GivenANullAllowedAppointmentSlotsParameter_WhenExecute_ThenNoExceptionIsThrown()
#pragma warning restore CA1707
        {
            // Arrange
            var systemUnderTest = new RetrieveAvailableAppointmentsUseCase(appointmentsGatewayMock.Object);

            // Act
            Func<Task> act = async () =>
                await systemUnderTest.Execute("SoR Code", Priority, "location Id", allowedAppointmentSlots: null);

            // Assert
            await act.Should().NotThrowAsync();
        }

        public static IEnumerable<object[]> InvalidArgumentTestData()
        {
            yield return new object[] { new ArgumentNullException(), null };
            yield return new object[] { new ArgumentException(), "" };
            yield return new object[] { new ArgumentException(), " " };
        }

        [Fact]
#pragma warning disable CA1707
        public async void GivenParameters_WhenExecute_ThenGetAvailableAppointmentsGatewayIsCalled()
#pragma warning restore CA1707
        {
            const string uprn = "uprn";
            const string locationId = "locationId";
            await sytemUndertest.Execute("uprn", Priority, "locationId");
            appointmentsGatewayMock.Verify(x => x.GetAvailableAppointments(uprn, Priority, locationId, null, null), Times.Once);
        }
    }
}
