namespace HousingRepairsSchedulingApi.Services.Drs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Domain;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class DrsService : IDrsService
    {
        private const string DummyPrimaryOrderNumber = "HousingRepairsOnlineDummyPrimaryOrderNumber";
        private const string DummyUserId = "HousingRepairsOnlineUserId";

        private readonly SOAP drsSoapClient;
        private readonly IOptions<DrsOptions> drsOptions;
        private readonly ILogger<DrsService> logger;

        private string sessionId;

        public DrsService(SOAP drsSoapClient, IOptions<DrsOptions> drsOptions, ILogger<DrsService> logger)
        {
            Guard.Against.Null(drsSoapClient, nameof(drsSoapClient));
            Guard.Against.Null(drsOptions, nameof(drsOptions));

            this.drsSoapClient = drsSoapClient;
            this.drsOptions = drsOptions;
            this.logger = logger;
        }

        public async Task<IEnumerable<AppointmentSlot>> CheckAvailability(string sorCode, string priority, string locationId, DateTime earliestDate)
        {
            Guard.Against.NullOrWhiteSpace(priority, nameof(priority));

            await EnsureSessionOpened();

            var checkAvailability = new xmbCheckAvailability
            {
                sessionId = sessionId,
                periodBegin = earliestDate,
                periodBeginSpecified = true,
                periodEnd = earliestDate.AddDays(drsOptions.Value.SearchTimeSpanInDays - 1),
                periodEndSpecified = true,
                theOrder = new order
                {
                    userId = DummyUserId,
                    contract = drsOptions.Value.Contract,
                    locationID = locationId,
                    primaryOrderNumber = DummyPrimaryOrderNumber,
                    priority = priority,
                    theBookingCodes = new[]{
                        new bookingCode {
                            bookingCodeSORCode = sorCode,
                            itemNumberWithinBooking = "1",
                            primaryOrderNumber = DummyPrimaryOrderNumber,
                            quantity = "1",
                        }
                    }
                }
            };

            var checkAvailabilityResponse = await drsSoapClient.checkAvailabilityAsync(new checkAvailability(checkAvailability));

            IEnumerable<AppointmentSlot> appointmentSlots;
            var xmbCheckAvailabilityResponse = checkAvailabilityResponse.@return;
            var responseStatuses = Enum.GetValues<responseStatus>();
            var nonSuccessResponseStatuses = responseStatuses.Where(x => x != responseStatus.success);
            if (nonSuccessResponseStatuses.Contains(xmbCheckAvailabilityResponse.status))
            {
                appointmentSlots = Enumerable.Empty<AppointmentSlot>();

                logger.LogInformation(
                    "DRS checkAvailability request unsuccessful (Response status: {Status};Error message: {ErrorMsg})",
                    xmbCheckAvailabilityResponse.status,
                    xmbCheckAvailabilityResponse.errorMsg);
            }
            else
            {
                appointmentSlots = xmbCheckAvailabilityResponse.theSlots
                    .Where(x => x.slotsForDay != null)
                    .SelectMany(x =>
                        x.slotsForDay.Where(y => y.available == availableValue.YES).Select(y =>
                            new AppointmentSlot
                            {
                                StartTime = y.beginDate,
                                EndTime = y.endDate,
                            }
                        )
                    );
            }

            return appointmentSlots;
        }

        public async Task<int> CreateOrder(string bookingReference, string sorCode, string priority, string locationId, string orderComments)
        {
            Guard.Against.NullOrWhiteSpace(bookingReference, nameof(bookingReference));
            Guard.Against.NullOrWhiteSpace(sorCode, nameof(sorCode));
            Guard.Against.NullOrWhiteSpace(priority, nameof(priority));
            Guard.Against.NullOrWhiteSpace(locationId, nameof(locationId));

            await EnsureSessionOpened();

            var createOrder = new xmbCreateOrder
            {
                sessionId = sessionId,
                theOrder = new order
                {
                    contract = drsOptions.Value.Contract,
                    locationID = locationId,
                    orderComments = orderComments,
                    primaryOrderNumber = bookingReference,
                    priority = priority,
                    targetDate = DateTime.Today.AddDays(20),
                    userId = DummyUserId,
                    theBookingCodes = new[]
                    {
                        new bookingCode
                        {
                            bookingCodeSORCode = sorCode,
                            itemNumberWithinBooking = "1",
                            primaryOrderNumber = bookingReference,
                            quantity = "1",
                        }
                    },
                }
            };

            var createOrderResponse = await drsSoapClient.createOrderAsync(new createOrder(createOrder));
            var result = createOrderResponse.@return.theOrder.theBookings[0].bookingId;

            return result;
        }

        public async Task ScheduleBooking(string bookingReference, int bookingId, DateTime startDateTime, DateTime endDateTime)
        {
            Guard.Against.NullOrWhiteSpace(bookingReference, nameof(bookingReference));
            Guard.Against.OutOfRange(endDateTime, nameof(endDateTime), startDateTime, DateTime.MaxValue);

            await EnsureSessionOpened();

            var scheduleBooking = new xmbScheduleBooking
            {
                sessionId = sessionId,
                theBooking = new booking
                {
                    bookingId = bookingId,
                    contract = drsOptions.Value.Contract,
                    primaryOrderNumber = bookingReference,
                    planningWindowStart = startDateTime,
                    planningWindowEnd = endDateTime,
                }
            };

            _ = await drsSoapClient.scheduleBookingAsync(new scheduleBooking(scheduleBooking));
        }

        private async Task OpenSession()
        {
            var xmbOpenSession = new xmbOpenSession
            {
                login = drsOptions.Value.Login,
                password = drsOptions.Value.Password
            };
            var response = await drsSoapClient.openSessionAsync(new openSession(xmbOpenSession));

            sessionId = response.@return.sessionId;
        }

        private async Task EnsureSessionOpened()
        {
            if (sessionId == null)
            {
                await OpenSession();
            }
        }
    }
}
