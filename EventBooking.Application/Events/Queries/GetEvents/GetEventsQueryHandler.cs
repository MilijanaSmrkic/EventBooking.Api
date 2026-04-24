using EventBooking.Application.Abstractions;
using EventBooking.Application.Common;
using EventBooking.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Events.Queries.GetEvents
{
    internal sealed class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, PagedResult<GetEventsResponse>>
    {
        private readonly IApplicationDbContext _context;

        public GetEventsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<GetEventsResponse>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Events.AsNoTracking();

            query = ApplyFilters(query, request);
            query = ApplySort(query, request);

            var totalCount = await query.CountAsync(cancellationToken);

            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var pageNumber = Math.Max(request.PageNumber, 1);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new GetEventsResponse
                {
                    Id = e.Id,
                    Title = e.Title,
                    LocationId = e.LocationId,
                    City = e.Location!.City,
                    PostalCode = e.Location!.PostalCode,
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category != null ? e.Category.Name : null,
                    EventDate = e.EventDate,
                    BasePrice = e.BasePrice,
                    Capacity = e.Capacity,
                    IsActive = e.IsActive
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<GetEventsResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        private static IQueryable<Event> ApplyFilters(IQueryable<Event> query, GetEventsQuery request)
        {
            if (!request.IncludeInactive)
                query = query.Where(e => e.IsActive);

            if (!string.IsNullOrWhiteSpace(request.City))
                query = query.Where(e => e.Location!.City.Contains(request.City));

            if (request.CategoryId.HasValue)
                query = query.Where(e => e.CategoryId == request.CategoryId.Value);

            if (request.DateFrom.HasValue)
                query = query.Where(e => e.EventDate >= request.DateFrom.Value);

            if (request.DateTo.HasValue)
                query = query.Where(e => e.EventDate <= request.DateTo.Value);

            return query;
        }

        private static IQueryable<Event> ApplySort(IQueryable<Event> query, GetEventsQuery request)
        {
            return request.SortBy switch
            {
                "Title"     => request.SortDescending ? query.OrderByDescending(e => e.Title)          : query.OrderBy(e => e.Title),
                "BasePrice" => request.SortDescending ? query.OrderByDescending(e => e.BasePrice)      : query.OrderBy(e => e.BasePrice),
                "Capacity"  => request.SortDescending ? query.OrderByDescending(e => e.Capacity)       : query.OrderBy(e => e.Capacity),
                "City"      => request.SortDescending ? query.OrderByDescending(e => e.Location!.City) : query.OrderBy(e => e.Location!.City),
                _           => request.SortDescending ? query.OrderByDescending(e => e.EventDate)      : query.OrderBy(e => e.EventDate)
            };
        }
    }
}
