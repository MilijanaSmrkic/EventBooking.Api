using MediatR;

namespace EventBooking.Application.Reservations.Events
{
    public record ReservationCancelledEvent(
        Guid CancelledReservationId,
        Guid EventId,
        int FreedSeats) : INotification;
}
