using EventBooking.Application.Abstractions;
using EventBooking.Application.Abstractions.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventBooking.Application.Reservations.Events
{
    internal sealed class ReservationCancelledEventHandler : INotificationHandler<ReservationCancelledEvent>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly ILogger<ReservationCancelledEventHandler> _logger;

        public ReservationCancelledEventHandler(
            IReservationRepository reservationRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher,
            ILogger<ReservationCancelledEventHandler> logger)
        {
            _reservationRepository = reservationRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task Handle(ReservationCancelledEvent notification, CancellationToken cancellationToken)
        {
            var waitlistReservation = await _reservationRepository.GetOldestWaitlistAsync(
                notification.EventId,
                notification.FreedSeats,
                cancellationToken);

            if (waitlistReservation is null)
            {
                _logger.LogDebug(
                    "No waitlist reservation eligible for promotion on event {EventId} (freed {FreedSeats} seats)",
                    notification.EventId, notification.FreedSeats);
                return;
            }

            waitlistReservation.Approve();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Waitlist reservation {ReservationId} promoted to PENDING — user {UserId}, event {EventId} ({FreedSeats} seats freed by cancellation of {CancelledReservationId})",
                waitlistReservation.Id, waitlistReservation.UserId, notification.EventId,
                notification.FreedSeats, notification.CancelledReservationId);

            // Send email only when the user has a valid address; approval itself always happens.
            if (!string.IsNullOrWhiteSpace(waitlistReservation.User?.Email))
            {
                await _publisher.Publish(new ReservationApprovedEvent(
                    waitlistReservation.Id,
                    waitlistReservation.User.Email,
                    waitlistReservation.Event?.Title,
                    waitlistReservation.ReservationDate,
                    waitlistReservation.SeatNumbers), cancellationToken);
            }
        }
    }
}
