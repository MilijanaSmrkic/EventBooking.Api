using EventBooking.Domain.Entities;

namespace EventBooking.Application.Abstractions.Repositories
{
    public interface ILocationRepository
    {
        void Add(Location location);
    }
}
