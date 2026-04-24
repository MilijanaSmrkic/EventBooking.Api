using EventBooking.Domain.Entities;

namespace EventBooking.Application.Abstractions.Repositories
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> LocationExistsAsync(int locationId, CancellationToken cancellationToken = default);
        Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default);
        void Add(Event @event);
    }
}
