using MediatR;

namespace EventBooking.Application.Reservations.Commands.CancelReservation
{
    public record CancelReservationCommand(Guid Id) : IRequest<Unit>;
}
