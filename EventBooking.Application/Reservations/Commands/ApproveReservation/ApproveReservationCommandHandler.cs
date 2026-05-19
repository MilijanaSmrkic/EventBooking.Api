using EventBooking.Application.Abstractions;
using EventBooking.Application.Abstractions.Repositories;
using EventBooking.Application.Reservations.Events;
using EventBooking.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventBooking.Application.Reservations.Commands.ApproveReservation
{
    internal sealed class ApproveReservationCommandHandler : IRequestHandler<ApproveReservationCommand, Unit>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly ILogger<ApproveReservationCommandHandler> _logger;

        public ApproveReservationCommandHandler(
            IReservationRepository reservationRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher,
            ILogger<ApproveReservationCommandHandler> logger)
        {
            _reservationRepository = reservationRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<Unit> Handle(ApproveReservationCommand request, CancellationToken cancellationToken)
        {
            var reservation = await _reservationRepository.GetByIdWithNavigationsAsync(request.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Reservation with id {request.Id} was not found.");

            if (reservation.StatusCode != ReservationStatuses.Pending)
                throw new InvalidOperationException(
                    $"Only pending reservations can be approved. Current status: {reservation.StatusCode}.");

            if (string.IsNullOrWhiteSpace(reservation.User?.Email))
                throw new InvalidOperationException(
                    $"User for reservation {request.Id} does not have a valid email address.");

            reservation.Approve();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Reservation {ReservationId} approved — user {UserId} ({Email}), event '{EventTitle}', seats {SeatCount}",
                reservation.Id, reservation.UserId, reservation.User.Email,
                reservation.Event?.Title, reservation.SeatNumbers.Count);

            await _publisher.Publish(new ReservationApprovedEvent(
                reservation.Id,
                reservation.User.Email,
                reservation.Event?.Title,
                reservation.ReservationDate,
                reservation.SeatNumbers), cancellationToken);

            return Unit.Value;
        }
    }
}
