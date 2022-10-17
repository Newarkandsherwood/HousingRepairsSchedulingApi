namespace HousingRepairsSchedulingApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;
    using Microsoft.AspNetCore.Mvc;
    using Sentry;
    using UseCases;

    [ApiController]
    [Route("[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IRetrieveAvailableAppointmentsUseCase retrieveAvailableAppointmentsUseCase;
        private readonly IBookAppointmentUseCase bookAppointmentUseCase;

        public AppointmentsController(IRetrieveAvailableAppointmentsUseCase retrieveAvailableAppointmentsUseCase,
            IBookAppointmentUseCase bookAppointmentUseCase)
        {
            this.retrieveAvailableAppointmentsUseCase = retrieveAvailableAppointmentsUseCase;
            this.bookAppointmentUseCase = bookAppointmentUseCase;
        }

        [HttpGet]
        [Route("AvailableAppointments")]
        public async Task<IActionResult> AvailableAppointments([FromQuery] string sorCode, [FromQuery] string priority,
            [FromQuery] string locationId, [FromQuery] DateTime? fromDate = null, [FromBody] IEnumerable<AppointmentSlotTimeSpan> allowedAppointmentSlots = default)
        {
            try
            {
                var result = await retrieveAvailableAppointmentsUseCase.Execute(sorCode, locationId, fromDate, allowedAppointmentSlots);
                return Ok(result);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("BookAppointment")]
        public async Task<IActionResult> BookAppointment([FromQuery] string bookingReference,
            [FromQuery] string sorCode,
            [FromQuery] string priority,
            [FromQuery] string locationId,
            [FromQuery] DateTime startDateTime,
            [FromQuery] DateTime endDateTime,
            [FromBody] RepairDescription repairDescriptionText)
        {
            try
            {
                var result = await bookAppointmentUseCase.Execute(bookingReference, sorCode, locationId, startDateTime, endDateTime, repairDescriptionText.Text);

                return Ok(result);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
