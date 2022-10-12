namespace HousingRepairsSchedulingApi.Tests.UseCasesTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Gateways;
    using Moq;
    using UseCases;
    using Xunit;

    public class BookAppointmentUseCaseTests
    {
        private const string BookingReference = "BookingReference";
        private const string SorCode = "SOR Code";
        private const string LocationId = "locationId";
        const string orderComment = "order comment";


        private BookAppointmentUseCase systemUnderTest;
        private Mock<IAppointmentsGateway> appointmentsGatewayMock;

        public BookAppointmentUseCaseTests()
        {
            appointmentsGatewayMock = new Mock<IAppointmentsGateway>();
            systemUnderTest = new BookAppointmentUseCase(appointmentsGatewayMock.Object);
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenAnInvalidBookingReference_WhenExecute_ThenExceptionIsThrown<T>(T exception, string bookingReference) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange

            // Act
            Func<Task> act = async () => await systemUnderTest.Execute(bookingReference, SorCode, LocationId,
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>());

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
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

            // Act
            Func<Task> act = async () => await systemUnderTest.Execute(BookingReference, sorCode, LocationId,
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>());

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

            // Act
            Func<Task> act = async () => await systemUnderTest.Execute(BookingReference, SorCode, locationId,
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>());

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Fact]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenOrderCommentsLongerThan255Characters_WhenExecute_ThenExceptionIsThrown()
#pragma warning restore xUnit1026
        {
            // Arrange
            var orderComments =
                "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis,.";
            // Act
            Func<Task> act = async () => await systemUnderTest.Execute(BookingReference, SorCode, LocationId,
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), orderComments);

            // Assert
            await act.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
        }

        [Fact]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenOrderCommentsOf0Characters_WhenExecute_ThenExceptionIsThrown()
#pragma warning restore xUnit1026
        {
            // Arrange
            var orderComments = "";

            // Act
            Func<Task> act = async () => await systemUnderTest.Execute(BookingReference, SorCode, LocationId,
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), orderComments);

            // Assert
            await act.Should().ThrowExactlyAsync<ArgumentException>();
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
                await systemUnderTest.Execute(BookingReference, SorCode, LocationId, startDate, endDate, orderComment);

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
            appointmentsGatewayMock.Setup(x => x.BookAppointment(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>()
                )
            ).ReturnsAsync(BookingReference);

            // Act
            var startDateTime = It.IsAny<DateTime>();
            var actual = await systemUnderTest.Execute(BookingReference, SorCode, LocationId,
                startDateTime, startDateTime.AddDays(1), orderComment);

            // Assert
            Assert.Equal(BookingReference, actual);
        }
    }
}
