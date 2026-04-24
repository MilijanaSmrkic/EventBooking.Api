using FluentValidation;

namespace EventBooking.Application.Locations.Commands.CreateLocation
{
    public class CreateLocationRequestValidator : AbstractValidator<CreateLocationRequest>
    {
        public CreateLocationRequestValidator()
        {
            RuleFor(l => l.City).NotEmpty().MaximumLength(100);
            RuleFor(l => l.PostalCode).NotEmpty().MaximumLength(20);
        }
    }
}
