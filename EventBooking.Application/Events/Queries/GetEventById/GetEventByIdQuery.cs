using MediatR;

namespace EventBooking.Application.Events.Queries.GetEventById
{
    public record GetEventByIdQuery(Guid Id) : IRequest<GetEventByIdResponse>;
}
