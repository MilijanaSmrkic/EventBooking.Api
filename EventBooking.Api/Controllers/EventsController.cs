using EventBooking.Application.Common;
using EventBooking.Application.Events.Commands.Create;
using EventBooking.Application.Events.Commands.DeactivateEvent;
using EventBooking.Application.Events.Commands.UpdateEvent;
using EventBooking.Application.Events.Queries.GetEventById;
using EventBooking.Application.Events.Queries.GetEvents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    [Produces("application/json")]
    public class EventsController : ControllerBase
    {
        private readonly ISender _sender;

        public EventsController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>Returns a paged list of events with optional filtering and sorting.</summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<GetEventsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEvents(
            [FromQuery] string? city,
            [FromQuery] int? categoryId,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] bool includeInactive = false,
            [FromQuery] string sortBy = "EventDate",
            [FromQuery] bool sortDescending = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _sender.Send(
                new GetEventsQuery(city, categoryId, dateFrom, dateTo, includeInactive, sortBy, sortDescending, pageNumber, pageSize),
                cancellationToken);

            return Ok(result);
        }

        /// <summary>Returns details of a single event by ID.</summary>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GetEventByIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetEventByIdQuery(id), cancellationToken);
            return Ok(result);
        }

        /// <summary>Creates a new event. Requires Admin role.</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateEvent(
            [FromBody] CreateEventRequest request,
            CancellationToken cancellationToken)
        {
            var id = await _sender.Send(
                new CreateEventCommand(
                    request.Title,
                    request.Description,
                    request.LocationId,
                    request.CategoryId,
                    request.EventDate,
                    request.BasePrice,
                    request.Capacity),
                cancellationToken);

            return CreatedAtAction(nameof(GetEventById), new { id }, id);
        }

        /// <summary>Updates an existing event. Requires Admin role.</summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateEvent(
            Guid id,
            [FromBody] UpdateEventRequest request,
            CancellationToken cancellationToken)
        {
            await _sender.Send(
                new UpdateEventCommand(
                    id,
                    request.Title,
                    request.Description,
                    request.LocationId,
                    request.CategoryId,
                    request.EventDate,
                    request.BasePrice,
                    request.Capacity),
                cancellationToken);

            return NoContent();
        }

        /// <summary>Deactivates an event (soft delete). Requires Admin role.</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeactivateEvent(Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new DeactivateEventCommand(id), cancellationToken);
            return NoContent();
        }
    }
}
