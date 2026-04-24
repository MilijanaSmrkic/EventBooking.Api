namespace EventBooking.Application.Reservations.Commands.CreateReservation
{
    public class CreateReservationRequest
    {
        public Guid EventId { get; set; }
        public int UserId { get; set; }
        public List<int> SeatNumbers { get; set; } = [];
    }
}
