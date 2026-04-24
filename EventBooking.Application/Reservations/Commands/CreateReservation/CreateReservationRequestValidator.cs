using FluentValidation;

namespace EventBooking.Application.Reservations.Commands.CreateReservation
{
    public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
    {
        public CreateReservationRequestValidator()
        {
            RuleFor(r => r.EventId).NotEmpty();
            RuleFor(r => r.UserId).GreaterThan(0);
            RuleFor(r => r.SeatNumbers).NotEmpty();
        }
    }
}
