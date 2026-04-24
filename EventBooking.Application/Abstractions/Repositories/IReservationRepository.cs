using EventBooking.Domain.Entities;

namespace EventBooking.Application.Abstractions.Repositories
{
    public interface IReservationRepository
    {
        Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Loads the reservation with User and Event navigations (needed for approval flow).</summary>
        Task<Reservation?> GetByIdWithNavigationsAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Total seats occupied by PENDING and CONFIRMED reservations for an event.</summary>
        Task<int> GetUsedSeatsAsync(Guid eventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Oldest WAITLIST reservation for the event whose SeatCount fits within freed seats.
        /// Loads User and Event navigations.
        /// </summary>
        Task<Reservation?> GetOldestWaitlistAsync(Guid eventId, int maxSeats, CancellationToken cancellationToken = default);

        /// <summary>All seat numbers currently held by PENDING or CONFIRMED reservations for an event.</summary>
        Task<List<int>> GetOccupiedSeatNumbersAsync(Guid eventId, CancellationToken cancellationToken = default);

        void Add(Reservation reservation);
    }
}
