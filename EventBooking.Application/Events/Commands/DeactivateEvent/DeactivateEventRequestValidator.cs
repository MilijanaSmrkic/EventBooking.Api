using FluentValidation;

namespace EventBooking.Application.Events.Commands.DeactivateEvent
{
    internal sealed class DeactivateEventRequestValidator : AbstractValidator<DeactivateEventRequest>
    {
        public DeactivateEventRequestValidator()
        {
            RuleFor(command => command.Id).NotEmpty();
        }
    }
}