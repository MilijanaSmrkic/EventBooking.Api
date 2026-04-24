using MediatR;

namespace EventBooking.Application.Events.Commands.Create
{
    public record CreateEventCommand(
        string? Title,
        string? Description,
        int LocationId,
        int? CategoryId,
        DateTime? EventDate,
        decimal BasePrice,
        int Capacity) : IRequest<Guid>;
}
