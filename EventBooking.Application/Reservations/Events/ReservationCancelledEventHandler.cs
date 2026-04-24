using EventBooking.Application.Abstractions;
using EventBooking.Application.Abstractions.Repositories;
using MediatR;

namespace EventBooking.Application.Reservations.Events
{
    internal sealed class ReservationCancelledEventHandler : INotificationHandler<ReservationCancelledEvent>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public ReservationCancelledEventHandler(
            IReservationRepository reservationRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _reservationRepository = reservationRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task Handle(ReservationCancelledEvent notification, CancellationToken cancellationToken)
        {
            var waitlistReservation = await _reservationRepository.GetOldestWaitlistAsync(
                notification.EventId,
                notification.FreedSeats,
                cancellationToken);

            if (waitlistReservation is null)
                return;

            waitlistReservation.Approve();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
