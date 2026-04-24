using MediatR;

namespace EventBooking.Application.Locations.Queries.GetLocations
{
    public record GetLocationsQuery(string? City = null) : IRequest<List<GetLocationsResponse>>;
}
