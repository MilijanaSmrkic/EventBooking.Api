using MediatR;

namespace EventBooking.Application.Reservations.Events
{
    public record ReservationApprovedEvent(
        Guid ReservationId,
        string RecipientEmail,
        string? EventTitle,
        DateTime ReservationDate,
        List<int> SeatNumbers) : INotification;
}
