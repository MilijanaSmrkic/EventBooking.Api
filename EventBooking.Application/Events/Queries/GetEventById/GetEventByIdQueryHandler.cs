using EventBooking.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Events.Queries.GetEventById
{
    internal sealed class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, GetEventByIdResponse>
    {
        private readonly IApplicationDbContext _context;

        public GetEventByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GetEventByIdResponse> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.Events
                .AsNoTracking()
                .Where(e => e.Id == request.Id)
                .Select(e => new GetEventByIdResponse
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    LocationId = e.LocationId,
                    City = e.Location!.City,
                    PostalCode = e.Location!.PostalCode,
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category != null ? e.Category.Name : null,
                    EventDate = e.EventDate,
                    BasePrice = e.BasePrice,
                    Capacity = e.Capacity,
                    CreatedAt = e.CreatedAt,
                    IsActive = e.IsActive
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException($"Event with id {request.Id} was not found.");
        }
    }
}
