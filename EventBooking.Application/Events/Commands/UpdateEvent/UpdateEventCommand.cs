using MediatR;

namespace EventBooking.Application.Events.Commands.UpdateEvent
{
    public record UpdateEventCommand(
        Guid Id,
        string? Title,
        string? Description,
        int LocationId,
        int? CategoryId,
        DateTime? EventDate,
        decimal BasePrice,
        int Capacity) : IRequest<Unit>;
}
