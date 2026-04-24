using EventBooking.Application.Models;

namespace EventBooking.Application.Abstractions
{
    public interface IEmailQueueService
    {
        Task QueueEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);
    }
}
