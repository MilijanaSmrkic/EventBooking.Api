using EventBooking.Application.Abstractions.Repositories;
using EventBooking.Domain.Entities;
using EventBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Infrastructure.Repositories
{
    internal sealed class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;

        public EventRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        public Task<bool> LocationExistsAsync(int locationId, CancellationToken cancellationToken = default)
            => _context.Locations.AnyAsync(l => l.Id == locationId, cancellationToken);

        public Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default)
            => _context.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken);

        public void Add(Event @event) => _context.Events.Add(@event);
    }
}
