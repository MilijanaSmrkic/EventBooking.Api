using MediatR;

namespace EventBooking.Application.Reservations.Commands.UpdateReservation
{
    public record UpdateReservationCommand(Guid Id, List<int> SeatNumbers) : IRequest<Unit>;
}
