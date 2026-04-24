using EventBooking.Application.Locations.Commands.CreateLocation;
using EventBooking.Application.Locations.Queries.GetLocations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.Api.Controllers
{
    [ApiController]
    [Route("api/locations")]
    [Produces("application/json")]
    public class LocationsController : ControllerBase
    {
        private readonly ISender _sender;

        public LocationsController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>Returns all locations, optionally filtered by city name.</summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<GetLocationsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLocations(
            [FromQuery] string? city,
            CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetLocationsQuery(city), cancellationToken);
            return Ok(result);
        }

        /// <summary>Creates a new location. Requires Admin role.</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateLocation(
            [FromBody] CreateLocationRequest request,
            CancellationToken cancellationToken)
        {
            var id = await _sender.Send(
                new CreateLocationCommand(request.City, request.PostalCode),
                cancellationToken);

            return CreatedAtAction(nameof(GetLocations), new { id }, id);
        }
    }
}
