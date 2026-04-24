using MediatR;

namespace EventBooking.Application.Events.Commands.DeactivateEvent
{
    public record DeactivateEventCommand(Guid Id) : IRequest<Unit>;
}
