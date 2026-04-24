using EventBooking.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Application.Categories.Queries.GetCategories
{
    internal sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<GetCategoriesResponse>>
    {
        private readonly IApplicationDbContext _context;

        public GetCategoriesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<GetCategoriesResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new GetCategoriesResponse
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync(cancellationToken);
        }
    }
}
