using EventBooking.Application.Abstractions;
using EventBooking.Application.Abstractions.Repositories;
using EventBooking.Domain.Entities;
using MediatR;

namespace EventBooking.Application.Reservations.Commands.UpdateReservation
{
    internal sealed class UpdateReservationCommandHandler : IRequestHandler<UpdateReservationCommand, Unit>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateReservationCommandHandler(
            IReservationRepository reservationRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _reservationRepository = reservationRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(UpdateReservationCommand request, CancellationToken cancellationToken)
        {
            var reservation = await _reservationRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Reservation with id {request.Id} was not found.");

            if (!_currentUserService.IsAdmin && reservation.UserId != _currentUserService.UserId)
                throw new UnauthorizedAccessException("You are not authorized to update this reservation.");

            if (reservation.StatusCode != ReservationStatuses.Pending)
                throw new InvalidOperationException(
                    $"Only pending reservations can be updated. Current status: {reservation.StatusCode}.");

            reservation.Update(request.SeatNumbers, _currentUserService.UserId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
