using EventBooking.Application.Abstractions;
using EventBooking.Application.Abstractions.Repositories;
using EventBooking.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Reservations.Commands.CreateReservation
{
    internal sealed class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Guid>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public CreateReservationCommandHandler(
            IEventRepository eventRepository,
            IReservationRepository reservationRepository,
            IApplicationDbContext context,
            IUnitOfWork unitOfWork)
        {
            _eventRepository = eventRepository;
            _reservationRepository = reservationRepository;
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
        {
            var @event = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);

            if (@event is null || !@event.IsActive)
                throw new KeyNotFoundException($"Active event with id {request.EventId} was not found.");

            var userExists = await _context.Users
                .AnyAsync(u => u.Id == request.UserId, cancellationToken);

            if (!userExists)
                throw new KeyNotFoundException($"User with id {request.UserId} was not found.");

            var usedSeats = await _reservationRepository.GetUsedSeatsAsync(request.EventId, cancellationToken);
            var availableSeats = @event.Capacity - usedSeats;

            Reservation reservation;

            if (request.SeatNumbers.Count <= availableSeats)
            {
                // Validate that the specific seats are not already taken
                var occupiedSeats = await _reservationRepository.GetOccupiedSeatNumbersAsync(request.EventId, cancellationToken);
                var conflicting = request.SeatNumbers.Intersect(occupiedSeats).ToList();

                if (conflicting.Count > 0)
                    throw new InvalidOperationException(
                        $"Seats {string.Join(", ", conflicting)} are already reserved.");

                reservation = Reservation.Create(request.EventId, request.UserId, request.SeatNumbers);
            }
            else
            {
                reservation = Reservation.CreateWaitlist(request.EventId, request.UserId, request.SeatNumbers);
            }

            _reservationRepository.Add(reservation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return reservation.Id;
        }
    }
}
