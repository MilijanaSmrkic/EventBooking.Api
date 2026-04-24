using EventBooking.Application.Abstractions;
using EventBooking.Application.Models;
using MediatR;
using Microsoft.Extensions.Options;

namespace EventBooking.Application.Reservations.Events
{
    internal sealed class ReservationApprovedEventHandler : INotificationHandler<ReservationApprovedEvent>
    {
        private readonly IEmailQueueService _emailQueueService;
        private readonly ReservationApprovedEmailOptions _emailOptions;

        public ReservationApprovedEventHandler(
            IEmailQueueService emailQueueService,
            IOptions<ReservationApprovedEmailOptions> emailOptions)
        {
            _emailQueueService = emailQueueService;
            _emailOptions = emailOptions.Value;
        }

        public async Task Handle(ReservationApprovedEvent notification, CancellationToken cancellationToken)
        {
            var seats = string.Join(", ", notification.SeatNumbers);
            var date = notification.ReservationDate.ToString("dd.MM.yyyy HH:mm");

            var subject = _emailOptions.Subject
                .Replace("{EventTitle}", notification.EventTitle);

            var body = _emailOptions.Body
                .Replace("{EventTitle}", notification.EventTitle)
                .Replace("{ReservationDate}", date)
                .Replace("{SeatNumbers}", seats)
                .Replace("{ReservationId}", notification.ReservationId.ToString());

            var message = new EmailMessage
            {
                To = notification.RecipientEmail,
                Subject = subject,
                Body = body
            };

            await _emailQueueService.QueueEmailAsync(message, cancellationToken);
        }
    }
}
