using EventBooking.Application.Abstractions.Repositories;
using EventBooking.Domain.Entities;
using EventBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Infrastructure.Repositories
{
    internal sealed class ReservationRepository : IReservationRepository
    {
        private readonly ApplicationDbContext _context;

        public ReservationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.Reservations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        public Task<Reservation?> GetByIdWithNavigationsAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.Reservations
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        public Task<int> GetUsedSeatsAsync(Guid eventId, CancellationToken cancellationToken = default)
            => _context.Reservations
                .Where(r => r.EventId == eventId &&
                            (r.StatusCode == ReservationStatuses.Pending ||
                             r.StatusCode == ReservationStatuses.Confirmed))
                .SumAsync(r => r.SeatCount, cancellationToken);

        public Task<Reservation?> GetOldestWaitlistAsync(Guid eventId, int maxSeats, CancellationToken cancellationToken = default)
            => _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Event)
                .Where(r => r.EventId == eventId &&
                            r.StatusCode == ReservationStatuses.Waitlist &&
                            r.SeatCount <= maxSeats)
                .OrderBy(r => r.ReservationDate)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<List<int>> GetOccupiedSeatNumbersAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            var seatLists = await _context.Reservations
                .Where(r => r.EventId == eventId &&
                            (r.StatusCode == ReservationStatuses.Pending ||
                             r.StatusCode == ReservationStatuses.Confirmed))
                .Select(r => r.SeatNumbers)
                .ToListAsync(cancellationToken);

            return seatLists.SelectMany(s => s).ToList();
        }

        public void Add(Reservation reservation) => _context.Reservations.Add(reservation);
    }
}
