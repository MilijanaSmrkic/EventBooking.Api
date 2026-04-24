using EventBooking.Application.Abstractions.Repositories;
using EventBooking.Domain.Entities;
using EventBooking.Infrastructure.Persistence;

namespace EventBooking.Infrastructure.Repositories
{
    internal sealed class LocationRepository : ILocationRepository
    {
        private readonly ApplicationDbContext _context;

        public LocationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(Location location) => _context.Locations.Add(location);
    }
}
