using EventBooking.Application.Common;
using MediatR;

namespace EventBooking.Application.Reservations.Queries.GetReservations
{
    public record GetReservationsQuery(
        Guid? EventId = null,
        int? UserId = null,
        string? StatusCode = null,
        DateTime? DateFrom = null,
        DateTime? DateTo = null,
        string SortBy = "ReservationDate",
        bool SortDescending = false,
        int PageNumber = 1,
        int PageSize = 20) : IRequest<PagedResult<GetReservationsResponse>>;
}
