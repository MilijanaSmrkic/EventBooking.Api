using EventBooking.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Reservations.Queries.GetReservationById
{
    internal sealed class GetReservationByIdQueryHandler : IRequestHandler<GetReservationByIdQuery, GetReservationByIdResponse>
    {
        private readonly IApplicationDbContext _context;

        public GetReservationByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GetReservationByIdResponse> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.Reservations
                .AsNoTracking()
                .Where(r => r.Id == request.Id)
                .Select(r => new GetReservationByIdResponse
                {
                    Id = r.Id,
                    EventId = r.EventId,
                    EventTitle = r.Event!.Title,
                    UserId = r.UserId,
                    UserEmail = r.User!.Email,
                    SeatNumbers = r.SeatNumbers,
                    ReservationDate = r.ReservationDate,
                    StatusCode = r.StatusCode,
                    StatusName = r.ReservationStatus!.Name
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException($"Reservation with id {request.Id} was not found.");
        }
    }
}
