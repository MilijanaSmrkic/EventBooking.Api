using MediatR;

namespace EventBooking.Application.Locations.Commands.CreateLocation
{
    public record CreateLocationCommand(string City, string PostalCode) : IRequest<int>;
}
