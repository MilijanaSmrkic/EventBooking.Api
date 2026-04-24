using MediatR;

namespace EventBooking.Application.Reservations.Queries.GetReservationById
{
    public record GetReservationByIdQuery(Guid Id) : IRequest<GetReservationByIdResponse>;
}
