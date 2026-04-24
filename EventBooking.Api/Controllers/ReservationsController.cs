using EventBooking.Application.Common;
using EventBooking.Application.Reservations.Commands.ApproveReservation;
using EventBooking.Application.Reservations.Commands.CancelReservation;
using EventBooking.Application.Reservations.Commands.CreateReservation;
using EventBooking.Application.Reservations.Commands.UpdateReservation;
using EventBooking.Application.Reservations.Queries.GetReservationById;
using EventBooking.Application.Reservations.Queries.GetReservations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.Api.Controllers
{
    [ApiController]
    [Route("api/reservations")]
    [Produces("application/json")]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly ISender _sender;

        public ReservationsController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>Returns a paged list of reservations. Requires Admin role.</summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PagedResult<GetReservationsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReservations(
            [FromQuery] Guid? eventId,
            [FromQuery] int? userId,
            [FromQuery] string? statusCode,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string sortBy = "ReservationDate",
            [FromQuery] bool sortDescending = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _sender.Send(
                new GetReservationsQuery(eventId, userId, statusCode, dateFrom, dateTo, sortBy, sortDescending, pageNumber, pageSize),
                cancellationToken);

            return Ok(result);
        }

        /// <summary>Returns details of a single reservation by ID.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(GetReservationByIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReservationById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetReservationByIdQuery(id), cancellationToken);
            return Ok(result);
        }

        /// <summary>Creates a new reservation. If event is full, placed on waitlist.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateReservation(
            [FromBody] CreateReservationRequest request,
            CancellationToken cancellationToken)
        {
            var id = await _sender.Send(
                new CreateReservationCommand(request.EventId, request.UserId, request.SeatNumbers),
                cancellationToken);

            return CreatedAtAction(nameof(GetReservationById), new { id }, id);
        }

        /// <summary>Updates seat numbers for an existing reservation.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReservation(
            Guid id,
            [FromBody] UpdateReservationRequest request,
            CancellationToken cancellationToken)
        {
            await _sender.Send(new UpdateReservationCommand(id, request.SeatNumbers), cancellationToken);
            return NoContent();
        }

        /// <summary>Approves a pending reservation and notifies user via email. Requires Admin role.</summary>
        [HttpPost("{id:guid}/approve")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApproveReservation(Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new ApproveReservationCommand(id), cancellationToken);
            return NoContent();
        }

        /// <summary>Cancels a reservation. If waitlist exists, first in line is auto-approved.</summary>
        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelReservation(Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new CancelReservationCommand(id), cancellationToken);
            return NoContent();
        }
    }
}
