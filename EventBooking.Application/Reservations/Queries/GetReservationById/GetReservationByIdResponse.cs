namespace EventBooking.Application.Reservations.Queries.GetReservationById
{
    public record GetReservationByIdResponse
    {
        public Guid Id { get; init; }
        public Guid EventId { get; init; }
        public string? EventTitle { get; init; }
        public int UserId { get; init; }
        public string? UserEmail { get; init; }
        public List<int> SeatNumbers { get; init; } = [];
        public DateTime ReservationDate { get; init; }
        public string StatusCode { get; init; } = string.Empty;
        public string? StatusName { get; init; }
    }
}
