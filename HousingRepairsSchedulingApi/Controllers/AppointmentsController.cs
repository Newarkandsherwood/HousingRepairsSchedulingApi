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
        private readonly IUpdateAppointmentUseCase updateAppointmentUseCase;
        private readonly ICancelAppointmentUseCase cancelAppointmentUseCase;

        public AppointmentsController(IRetrieveAvailableAppointmentsUseCase retrieveAvailableAppointmentsUseCase,
            IBookAppointmentUseCase bookAppointmentUseCase, IUpdateAppointmentUseCase updateAppointmentUseCase, ICancelAppointmentUseCase cancelAppointmentUseCase)
        {
            this.retrieveAvailableAppointmentsUseCase = retrieveAvailableAppointmentsUseCase;
            this.bookAppointmentUseCase = bookAppointmentUseCase;
            this.updateAppointmentUseCase = updateAppointmentUseCase;
            this.cancelAppointmentUseCase = cancelAppointmentUseCase;
        }

        [HttpGet]
        [Route("AvailableAppointments")]
        public async Task<IActionResult> AvailableAppointments([FromQuery] string sorCode, [FromQuery] string priority,
            [FromQuery] string locationId, [FromQuery] DateTime? fromDate = null, [FromBody] IEnumerable<AppointmentSlotTimeSpan> allowedAppointmentSlots = default)
        {
            try
            {
                var result = await retrieveAvailableAppointmentsUseCase.Execute(sorCode, priority, locationId, fromDate, allowedAppointmentSlots);
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
                var result = await bookAppointmentUseCase.Execute(bookingReference, sorCode, priority, locationId, startDateTime, endDateTime, repairDescriptionText.Text);

                return Ok(result);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route(nameof(CancelAppointment))]
        public async Task<IActionResult> CancelAppointment([FromQuery] string bookingReference)
        {
            try
            {
                var result = await cancelAppointmentUseCase.Execute(bookingReference);
                if (string.IsNullOrEmpty(result))
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route(nameof(UpdateAppointmentSlot))]
        public async Task<IActionResult> UpdateAppointmentSlot([FromQuery] string bookingReference,
            [FromQuery] DateTime startDateTime,
            [FromQuery] DateTime endDateTime)
        {
            try
            {
                var result = await updateAppointmentUseCase.Execute(bookingReference, startDateTime, endDateTime);
                if (string.IsNullOrEmpty(result))
                {
                    return NotFound();
                }

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
