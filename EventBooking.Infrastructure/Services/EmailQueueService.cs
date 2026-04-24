using EventBooking.Application.Abstractions;
using EventBooking.Application.Models;
using Microsoft.Extensions.Logging;

namespace EventBooking.Infrastructure.Services
{
    public class EmailQueueService : IEmailQueueService
    {
        private readonly ILogger<EmailQueueService> _logger;

        public EmailQueueService(ILogger<EmailQueueService> logger)
        {
            _logger = logger;
        }

        public Task QueueEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            // TODO: zamijeniti sa stvarnim slanjem u message queue (npr. RabbitMQ, Azure Service Bus)
            _logger.LogInformation("Email queued. To: {To}, Subject: {Subject}", message.To, message.Subject);
            return Task.CompletedTask;
        }
    }
}
