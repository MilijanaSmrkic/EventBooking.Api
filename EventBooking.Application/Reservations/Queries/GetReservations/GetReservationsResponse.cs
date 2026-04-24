namespace EventBooking.Application.Reservations.Queries.GetReservations
{
    public record GetReservationsResponse
    {
        public Guid Id { get; init; }
        public Guid EventId { get; init; }
        public int UserId { get; init; }
        public List<int> SeatNumbers { get; init; } = [];
        public DateTime ReservationDate { get; init; }
        public string StatusCode { get; init; } = string.Empty;
    }
}
