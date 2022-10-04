namespace HousingRepairsSchedulingApi.Tests.ServicesTests.Drs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Domain;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using Services.Drs;
    using Xunit;

    [SuppressMessage("Naming", "CA1707", MessageId = "Identifiers should not contain underscores")]
    public class DrsServiceTests
    {
        private const string SorCode = "SorCode";
        private const string LocationId = "LocationId";
        private const string BookingReference = "BookingReference";
        private const int BookingId = 12345;

        private Mock<SOAP> soapMock;

        private DrsService systemUnderTest;

        public DrsServiceTests()
        {
            var drsOptionsMock = new Mock<IOptions<DrsOptions>>();
            drsOptionsMock.Setup(x => x.Value)
                .Returns(new DrsOptions { Login = "login", Password = "password" });

            soapMock = new Mock<SOAP>();
            soapMock.Setup(x => x.openSessionAsync(It.IsAny<openSession>()))
                .ReturnsAsync(new openSessionResponse
                {
                    @return = new xmbOpenSessionResponse { sessionId = "sessionId" }
                });

            systemUnderTest = CreateDrsService(soapMock.Object, drsOptionsMock.Object);
        }

        [Fact]
        public void GivenNullDrsSoapClientParameter_WhenInstantiating_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange

            // Act
            Func<DrsService> act = () => CreateDrsService(null, It.IsAny<IOptions<DrsOptions>>());

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("drsSoapClient");
        }

        [Fact]
        public void GivenNullDrsOptionsParameter_WhenInstantiating_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange

            // Act
            Func<DrsService> act = () => CreateDrsService(new Mock<SOAP>().Object, null);

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("drsOptions");
        }

        [Theory]
        [MemberData(nameof(UnavailableSlotsTestData))]
        public async void
            GivenDrsCheckAvailabilityResponseContainsUnavailableSlots_WhenCheckingAvailability_ThenOnlyAvailableSlotsAreReturned(DateTime searchDate, daySlotsInfo[] daySlots, IEnumerable<AppointmentSlot> expected)
        {
            // Arrange

            soapMock.Setup(x => x.checkAvailabilityAsync(It.IsAny<checkAvailability>()))
                .ReturnsAsync(new checkAvailabilityResponse(
                    new xmbCheckAvailabilityResponse { theSlots = daySlots, status = responseStatus.success }));

            // Act
            var appointmentSlots = await systemUnderTest.CheckAvailability(SorCode, LocationId, searchDate);

            // Assert
            appointmentSlots.Should().BeEquivalentTo(expected);
        }

        public static IEnumerable<object[]> UnavailableSlotsTestData()
        {
            var date = new DateTime(2022, 1, 19);

            yield return new object[]
            {
                date,
                new[]
                {
                    new daySlotsInfo
                    {
                        day = date,
                        slotsForDay = new[]
                        {
                            new slotInfo
                            {
                                available = availableValue.NO,
                                beginDate = date.AddHours(8),
                                endDate = date.AddHours(12),
                            },
                        }
                    }
                },
                Enumerable.Empty<AppointmentSlot>()
            };

            yield return new object[]
            {
                date,
                new[]
                {
                    new daySlotsInfo
                    {
                        day = date,
                        slotsForDay = new[]
                        {
                            new slotInfo
                            {
                                available = availableValue.YES,
                                beginDate = date.AddHours(8),
                                endDate = date.AddHours(12),
                            },
                        }
                    }
                },
                new []{new AppointmentSlot{StartTime = date.AddHours(8), EndTime = date.AddHours(12)}}
            };

            yield return new object[]
            {
                date,
                new[]
                {
                    new daySlotsInfo
                    {
                        day = date,
                        slotsForDay = new[]
                        {
                            new slotInfo
                            {
                                available = availableValue.NO,
                                beginDate = date.AddHours(8),
                                endDate = date.AddHours(12),
                            },
                            new slotInfo
                            {
                                available = availableValue.YES,
                                beginDate = date.AddHours(12),
                                endDate = date.AddHours(16),
                            },
                        }
                    }
                },
                new []{new AppointmentSlot{StartTime = date.AddHours(12), EndTime = date.AddHours(16)}}
            };
        }

        [Fact]
        public async void
            GivenDrsCheckAvailabilityResponseContainsDaysWithNoSlots_WhenCheckingAvailability_ThenArgumentNullExceptionIsNotThrown()
        {
            // Arrange
            var dateTime = new DateTime(2022, 1, 19);

            soapMock.Setup(x => x.checkAvailabilityAsync(It.IsAny<checkAvailability>()))
                .ReturnsAsync(new checkAvailabilityResponse(
                    new xmbCheckAvailabilityResponse { theSlots = new[] { new daySlotsInfo { day = dateTime } } }));

            // Act
            var appointmentSlots = await systemUnderTest.CheckAvailability(SorCode, LocationId, dateTime);
            Func<IEnumerable<AppointmentSlot>> act = () => appointmentSlots.ToArray();

            // Assert
            act.Should().NotThrow<ArgumentNullException>();
        }

        [Theory]
        [MemberData(nameof(NonSuccessResponseStatuses))]
#pragma warning disable CA1707
        public async void GivenDrsCheckAvailabilityResponseDoesNotHaveSuccessStatus_WhenCheckingAvailability_ThenNoAppointmentsAreReturned(responseStatus status)
#pragma warning restore CA1707
        {
            // Arrange
            soapMock.Setup(x => x.checkAvailabilityAsync(It.IsAny<checkAvailability>()))
                .ReturnsAsync(new checkAvailabilityResponse(
                    new xmbCheckAvailabilityResponse { status = status }));
            var searchDate = new DateTime(2022, 1, 1);

            // Act
            var appointmentSlots = await systemUnderTest.CheckAvailability(SorCode, LocationId, searchDate);

            // Assert
            appointmentSlots.Should().BeEmpty();
        }

        [Theory]
        [MemberData(nameof(NonSuccessResponseStatuses))]
#pragma warning disable CA1707
        public async void GivenDrsCheckAvailabilityResponseDoesNotHaveSuccessStatus_WhenCheckingAvailability_ThenInformationLogAdded(responseStatus status)
#pragma warning restore CA1707
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DrsService>>();
            var createLogger = () => loggerMock.Object;
            var (systemUnderTest, soapMock) = CreateSystemUnderTestAndSoapMock(drsOptions => { }, createLogger);
            soapMock.Setup(x => x.checkAvailabilityAsync(It.IsAny<checkAvailability>()))
                .ReturnsAsync(new checkAvailabilityResponse(
                    new xmbCheckAvailabilityResponse { status = status }));
            var searchDate = new DateTime(2022, 1, 1);

            // Act
            _ = await systemUnderTest.CheckAvailability(SorCode, LocationId, searchDate);

            // Assert
            Assert.Equal(1, loggerMock.Invocations.Count);
            Assert.Equal(LogLevel.Information, loggerMock.Invocations.Single().Arguments[0]);
        }

        public static TheoryData<responseStatus> NonSuccessResponseStatuses()
        {
            var responseStatuses = Enum.GetValues<responseStatus>();
            var nonSuccessResponseStatuses = responseStatuses.Where(x => x != responseStatus.success);

            var result = new TheoryData<responseStatus>();
            foreach (var nonSuccessResponseStatus in nonSuccessResponseStatuses)
            {
                result.Add(nonSuccessResponseStatus);
            }
            return result;

        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenInvalidBookingReference_WhenCreatingAnOrder_ThenExceptionIsThrown<T>(T exception, string bookingReference) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange

            // Act
            Func<Task> act = async () => await systemUnderTest.CreateOrder(bookingReference, It.IsAny<string>(), It.IsAny<string>());

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }


        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenInvalidSorCode_WhenCreatingAnOrder_ThenExceptionIsThrown<T>(T exception, string sorCode) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange

            // Act
            Func<Task> act = async () => await systemUnderTest.CreateOrder(BookingReference, sorCode, It.IsAny<string>());

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }


        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenInvalidLocationId_WhenCreatingAnOrder_ThenExceptionIsThrown<T>(T exception, string locationId) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange

            // Act
            Func<Task> act = async () => await systemUnderTest.CreateOrder(BookingReference, SorCode, locationId);

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Fact]
#pragma warning disable CA1707
        public async void GivenCreateOrderResponse_WhenCreatingAnOrder_ThenBookingIdIsPresentInTheResponse()
#pragma warning restore CA1707
        {
            // Arrange
            soapMock.Setup(x => x.createOrderAsync(It.IsAny<createOrder>()))
                .ReturnsAsync(new createOrderResponse(new xmbCreateOrderResponse
                {
                    theOrder = new order { theBookings = new[] { new booking { bookingId = BookingId } } }
                }));

            // Act
            var actual = await systemUnderTest.CreateOrder(BookingReference, SorCode, LocationId);

            // Assert
            Assert.Equal(BookingId, actual);

        }


        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenInvalidBookingReference_WhenSchedulingABooking_ThenExceptionIsThrown<T>(T exception, string bookingReference) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange

            // Act
            Func<Task> act = async () => await systemUnderTest.ScheduleBooking(bookingReference, It.IsAny<int>(),
                It.IsAny<DateTime>(), It.IsAny<DateTime>());

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
                await systemUnderTest.ScheduleBooking(BookingReference, BookingId, startDate, endDate);

            // Assert
            await act.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
        }

        [Fact]
#pragma warning disable CA1707
        public async void GivenScheduleBookingResponse_WhenSchedulingABooking_ThenDrsSoapScheduleBookingIsCalled()
#pragma warning restore CA1707
        {
            // Arrange
            Expression<Action<SOAP>> schedulingBookingExpression = x => x.scheduleBookingAsync(It.IsAny<scheduleBooking>());
            soapMock.Setup(schedulingBookingExpression);
            var startDate = new DateTime(2022, 1, 21);
            var endDate = startDate.AddDays(1);

            // Act
            await systemUnderTest.ScheduleBooking(BookingReference, BookingId, startDate, endDate);

            // Assert
            soapMock.Verify(schedulingBookingExpression);
        }

        public static IEnumerable<object[]> InvalidArgumentTestData()
        {
            yield return new object[] { new ArgumentNullException(), null };
            yield return new object[] { new ArgumentException(), "" };
            yield return new object[] { new ArgumentException(), " " };
        }

        [Theory]
        [InlineData("Contract1")]
        [InlineData("Contract2")]
#pragma warning disable CA1707
        public async void GivenDrsContract_WhenCheckingAvailability_ThenDrsContractIsUsed(string drsContract)
#pragma warning restore CA1707
        {
            // Arrange
            var (systemUnderTest, soapMock) = CreateSystemUnderTestAndSoapMockWithContract(drsContract);

            var dateTime = new DateTime(2022, 1, 1);
            string actualContract = null;

            soapMock.Setup(x => x.checkAvailabilityAsync(It.IsAny<checkAvailability>()))
                .Callback<checkAvailability>(request => actualContract = request.checkAvailability1.theOrder.contract)
                .ReturnsAsync(new checkAvailabilityResponse(
                    new xmbCheckAvailabilityResponse { theSlots = new[] { new daySlotsInfo { day = dateTime } } }));

            // Act
            _ = await systemUnderTest.CheckAvailability(SorCode, LocationId, dateTime);

            // Assert
            actualContract.Should().NotBeNull();
            actualContract.Should().Be(drsContract);
        }

        [Theory]
        [InlineData("Contract1")]
        [InlineData("Contract2")]
#pragma warning disable CA1707
        public async void GivenDrsContract_WhenCreatingOrder_ThenDrsContractIsUsed(string drsContract)
#pragma warning restore CA1707
        {
            // Arrange
            var (systemUnderTest, soapMock) = CreateSystemUnderTestAndSoapMockWithContract(drsContract);

            string actualContract = null;

            soapMock.Setup(x => x.createOrderAsync(It.IsAny<createOrder>()))
                .Callback<createOrder>(request => actualContract = request.createOrder1.theOrder.contract)
                .ReturnsAsync(new createOrderResponse(new xmbCreateOrderResponse
                {
                    theOrder = new order { theBookings = new[] { new booking { bookingId = BookingId } } }
                }));

            // Act
            _ = await systemUnderTest.CreateOrder(BookingReference, SorCode, LocationId);

            // Assert
            actualContract.Should().NotBeNull();
            actualContract.Should().Be(drsContract);
        }

        [Theory]
        [InlineData("Contract1")]
        [InlineData("Contract2")]
#pragma warning disable CA1707
        public async void GivenDrsContract_WhenSchedulingABooking_ThenDrsContractIsUsed(string drsContract)
#pragma warning restore CA1707
        {
            // Arrange
            var (systemUnderTest, soapMock) = CreateSystemUnderTestAndSoapMockWithContract(drsContract);

            string actualContract = null;

            soapMock.Setup(x => x.scheduleBookingAsync(It.IsAny<scheduleBooking>()))
                .Callback<scheduleBooking>(request => actualContract = request.scheduleBooking1.theBooking.contract);
            var startDate = new DateTime(2022, 1, 21);
            var endDate = startDate.AddDays(1);

            // Act
            await systemUnderTest.ScheduleBooking(BookingReference, BookingId, startDate, endDate);

            // Assert
            actualContract.Should().NotBeNull();
            actualContract.Should().Be(drsContract);
        }

        [Theory]
        [InlineData("Priority1")]
        [InlineData("Priority2")]
        public async void GivenDrsPriority_WhenCheckingAvailability_ThenDrsPriorityIsUsed(string drsPriority)
        {
            // Arrange
            var (systemUnderTest, soapMock) = CreateSystemUnderTestAndSoapMockWithPriority(drsPriority);

            var dateTime = new DateTime(2022, 1, 1);
            string actualPriority = null;

            soapMock.Setup(x => x.checkAvailabilityAsync(It.IsAny<checkAvailability>()))
                .Callback<checkAvailability>(request => actualPriority = request.checkAvailability1.theOrder.priority)
                .ReturnsAsync(new checkAvailabilityResponse(
                    new xmbCheckAvailabilityResponse { theSlots = new[] { new daySlotsInfo { day = dateTime } } }));

            // Act
            _ = await systemUnderTest.CheckAvailability(SorCode, LocationId, dateTime);

            // Assert
            actualPriority.Should().NotBeNull();
            actualPriority.Should().Be(drsPriority);
        }

        [Theory]
        [InlineData("Contract1")]
        [InlineData("Contract2")]
#pragma warning disable CA1707
        public async void GivenDrsPriority_WhenCreatingOrder_ThenDrsPriorityIsUsed(string drsPriority)
#pragma warning restore CA1707
        {
            // Arrange
            var (systemUnderTest, soapMock) = CreateSystemUnderTestAndSoapMockWithPriority(drsPriority);

            string actualPriority = null;

            soapMock.Setup(x => x.createOrderAsync(It.IsAny<createOrder>()))
                .Callback<createOrder>(request => actualPriority = request.createOrder1.theOrder.priority)
                .ReturnsAsync(new createOrderResponse(new xmbCreateOrderResponse
                {
                    theOrder = new order { theBookings = new[] { new booking { bookingId = BookingId } } }
                }));

            // Act
            _ = await systemUnderTest.CreateOrder(BookingReference, SorCode, LocationId);

            // Assert
            actualPriority.Should().NotBeNull();
            actualPriority.Should().Be(drsPriority);
        }

        private static DrsService CreateDrsService(SOAP soapClient, IOptions<DrsOptions> drsOptions, Func<ILogger<DrsService>> createLoggerFunction = default)
        {
            var loggerMock = createLoggerFunction != null
                ? createLoggerFunction()
                : new Mock<ILogger<DrsService>>().Object;

            return new DrsService(soapClient, drsOptions, loggerMock);
        }

        private (DrsService, Mock<SOAP>) CreateSystemUnderTestAndSoapMock(Action<DrsOptions> additionalSetup, Func<ILogger<DrsService>> createLoggerFunction = default)
        {
            var drsOptionsMock = new Mock<IOptions<DrsOptions>>();
            var drsOptions = new DrsOptions { Login = "login", Password = "password" };
            additionalSetup(drsOptions);

            drsOptionsMock.Setup(x => x.Value)
                .Returns(drsOptions);


            var soapMock = new Mock<SOAP>();
            soapMock.Setup(x => x.openSessionAsync(It.IsAny<openSession>()))
                .ReturnsAsync(new openSessionResponse
                {
                    @return = new xmbOpenSessionResponse { sessionId = "sessionId" }
                });

            var drsService = CreateDrsService(soapMock.Object, drsOptionsMock.Object, createLoggerFunction);

            return (drsService, soapMock);
        }

        private (DrsService, Mock<SOAP>) CreateSystemUnderTestAndSoapMockWithContract(string contract)
        {
            Action<DrsOptions> setupContract = drsOptions => drsOptions.Contract = contract;

            return CreateSystemUnderTestAndSoapMock(setupContract);
        }

        private (DrsService, Mock<SOAP>) CreateSystemUnderTestAndSoapMockWithPriority(string priority)
        {
            Action<DrsOptions> setupPriority = drsOptions => drsOptions.Priority = priority;

            return CreateSystemUnderTestAndSoapMock(setupPriority);
        }
    }
}
