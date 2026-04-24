using EventBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Abstractions
{
    public interface IApplicationDbContext
    {
        DbSet<Event> Events { get; }
        DbSet<Reservation> Reservations { get; }
        DbSet<User> Users { get; }
        DbSet<Location> Locations { get; }
        DbSet<Category> Categories { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
