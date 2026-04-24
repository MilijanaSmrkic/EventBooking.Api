using FluentValidation;

namespace EventBooking.Application.Events.Commands.Create
{
    public class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
    {
        public CreateEventRequestValidator()
        {
            RuleFor(command => command.Title).NotEmpty().MaximumLength(50);
            RuleFor(command => command.LocationId).GreaterThan(0);
            RuleFor(command => command.EventDate).GreaterThan(DateTime.UtcNow);
            RuleFor(command => command.BasePrice).GreaterThan(0);
            RuleFor(command => command.Capacity).GreaterThan(0);
        }
    }
}
