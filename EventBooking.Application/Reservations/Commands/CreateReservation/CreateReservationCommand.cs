using MediatR;

namespace EventBooking.Application.Reservations.Commands.CreateReservation
{
    public record CreateReservationCommand(
        Guid EventId,
        int UserId,
        List<int> SeatNumbers) : IRequest<Guid>;
}
