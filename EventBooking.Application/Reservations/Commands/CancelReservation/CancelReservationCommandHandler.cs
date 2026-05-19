using EventBooking.Application.Abstractions;
using EventBooking.Application.Abstractions.Repositories;
using EventBooking.Application.Reservations.Events;
using EventBooking.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventBooking.Application.Reservations.Commands.CancelReservation
{
    internal sealed class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand, Unit>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CancelReservationCommandHandler> _logger;

        public CancelReservationCommandHandler(
            IReservationRepository reservationRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher,
            ICurrentUserService currentUserService,
            ILogger<CancelReservationCommandHandler> logger)
        {
            _reservationRepository = reservationRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Unit> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
        {
            var reservation = await _reservationRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Reservation with id {request.Id} was not found.");

            if (!_currentUserService.IsAdmin && reservation.UserId != _currentUserService.UserId)
                throw new UnauthorizedAccessException("You are not authorized to cancel this reservation.");

            if (reservation.StatusCode == ReservationStatuses.Cancelled)
                throw new InvalidOperationException("Reservation is already cancelled.");

            var wasOccupyingCapacity =
                reservation.StatusCode == ReservationStatuses.Pending ||
                reservation.StatusCode == ReservationStatuses.Confirmed;

            var freedSeats = reservation.SeatCount;
            var eventId = reservation.EventId;

            reservation.Cancel(_currentUserService.UserId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Reservation {ReservationId} cancelled by user {CancelledByUserId} (was {PreviousStatus}, freed {FreedSeats} seats, event {EventId})",
                reservation.Id, _currentUserService.UserId, wasOccupyingCapacity ? "occupying" : "waitlist",
                freedSeats, eventId);

            if (wasOccupyingCapacity)
                await _publisher.Publish(
                    new ReservationCancelledEvent(reservation.Id, eventId, freedSeats),
                    cancellationToken);

            return Unit.Value;
        }
    }
}
