using EventBooking.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Locations.Queries.GetLocations
{
    internal sealed class GetLocationsQueryHandler : IRequestHandler<GetLocationsQuery, List<GetLocationsResponse>>
    {
        private readonly IApplicationDbContext _context;

        public GetLocationsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<GetLocationsResponse>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Locations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.City))
                query = query.Where(l => l.City.Contains(request.City));

            return await query
                .OrderBy(l => l.City)
                .Select(l => new GetLocationsResponse
                {
                    Id = l.Id,
                    City = l.City,
                    PostalCode = l.PostalCode
                })
                .ToListAsync(cancellationToken);
        }
    }
}
