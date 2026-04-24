using EventBooking.Application.Abstractions;
using EventBooking.Application.Common;
using EventBooking.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Reservations.Queries.GetReservations
{
    internal sealed class GetReservationsQueryHandler : IRequestHandler<GetReservationsQuery, PagedResult<GetReservationsResponse>>
    {
        private readonly IApplicationDbContext _context;

        public GetReservationsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<GetReservationsResponse>> Handle(GetReservationsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Reservations.AsNoTracking();

            query = ApplyFilters(query, request);
            query = ApplySort(query, request);

            var totalCount = await query.CountAsync(cancellationToken);

            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var pageNumber = Math.Max(request.PageNumber, 1);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new GetReservationsResponse
                {
                    Id = r.Id,
                    EventId = r.EventId,
                    UserId = r.UserId,
                    SeatNumbers = r.SeatNumbers,
                    ReservationDate = r.ReservationDate,
                    StatusCode = r.StatusCode
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<GetReservationsResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        private static IQueryable<Reservation> ApplyFilters(IQueryable<Reservation> query, GetReservationsQuery request)
        {
            if (request.EventId.HasValue)
                query = query.Where(r => r.EventId == request.EventId.Value);

            if (request.UserId.HasValue)
                query = query.Where(r => r.UserId == request.UserId.Value);

            if (!string.IsNullOrWhiteSpace(request.StatusCode))
                query = query.Where(r => r.StatusCode == request.StatusCode);

            if (request.DateFrom.HasValue)
                query = query.Where(r => r.ReservationDate >= request.DateFrom.Value);

            if (request.DateTo.HasValue)
                query = query.Where(r => r.ReservationDate <= request.DateTo.Value);

            return query;
        }

        private static IQueryable<Reservation> ApplySort(IQueryable<Reservation> query, GetReservationsQuery request)
        {
            return request.SortBy switch
            {
                "StatusCode" => request.SortDescending ? query.OrderByDescending(r => r.StatusCode) : query.OrderBy(r => r.StatusCode),
                _            => request.SortDescending ? query.OrderByDescending(r => r.ReservationDate) : query.OrderBy(r => r.ReservationDate)
            };
        }
    }
}
