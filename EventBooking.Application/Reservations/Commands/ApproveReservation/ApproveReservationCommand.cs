using MediatR;

namespace EventBooking.Application.Reservations.Commands.ApproveReservation
{
    public record ApproveReservationCommand(Guid Id) : IRequest<Unit>;
}
