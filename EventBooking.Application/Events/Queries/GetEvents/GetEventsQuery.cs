using EventBooking.Application.Common;
using MediatR;

namespace EventBooking.Application.Events.Queries.GetEvents
{
    public record GetEventsQuery(
        string? City = null,
        int? CategoryId = null,
        DateTime? DateFrom = null,
        DateTime? DateTo = null,
        bool IncludeInactive = false,
        string SortBy = "EventDate",
        bool SortDescending = false,
        int PageNumber = 1,
        int PageSize = 20) : IRequest<PagedResult<GetEventsResponse>>;
}
