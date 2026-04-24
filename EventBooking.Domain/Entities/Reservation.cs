namespace EventBooking.Domain.Entities
{
    public class Reservation
    {
        private Reservation() { }

        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
        public int UserId { get; private set; }
        public List<int> SeatNumbers { get; private set; } = [];
        public int SeatCount { get; private set; }
        public DateTime ReservationDate { get; private set; }
        public string StatusCode { get; private set; } = ReservationStatuses.Pending;

        public int? CancelledByUserId { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public int? LastModifiedByUserId { get; private set; }
        public DateTime? LastModifiedAt { get; private set; }

        public ReservationStatus? ReservationStatus { get; private set; }
        public Event? Event { get; private set; }
        public User? User { get; private set; }

        public static Reservation Create(Guid eventId, int userId, List<int> seatNumbers)
        {
            return new Reservation
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                SeatNumbers = seatNumbers,
                SeatCount = seatNumbers.Count,
                ReservationDate = DateTime.UtcNow,
                StatusCode = ReservationStatuses.Pending
            };
        }

        public static Reservation CreateWaitlist(Guid eventId, int userId, List<int> seatNumbers)
        {
            return new Reservation
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                SeatNumbers = seatNumbers,
                SeatCount = seatNumbers.Count,
                ReservationDate = DateTime.UtcNow,
                StatusCode = ReservationStatuses.Waitlist
            };
        }

        public void Update(List<int> seatNumbers, int? modifiedByUserId = null)
        {
            SeatNumbers = seatNumbers;
            SeatCount = seatNumbers.Count;
            LastModifiedByUserId = modifiedByUserId;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void Approve() => StatusCode = ReservationStatuses.Confirmed;

        public void Cancel(int? cancelledByUserId = null)
        {
            StatusCode = ReservationStatuses.Cancelled;
            CancelledByUserId = cancelledByUserId;
            CancelledAt = DateTime.UtcNow;
        }
    }

    public static class ReservationStatuses
    {
        public const string Pending = "PENDING";
        public const string Confirmed = "CONFIRMED";
        public const string Cancelled = "CANCELLED";
        public const string Waitlist = "WAITLIST";
    }
}
