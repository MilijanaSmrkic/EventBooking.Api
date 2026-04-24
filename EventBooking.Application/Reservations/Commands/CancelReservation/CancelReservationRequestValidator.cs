using FluentValidation;

namespace EventBooking.Application.Reservations.Commands.CancelReservation
{
    public class CancelReservationRequestValidator : AbstractValidator<CancelReservationRequest>
    {
        public CancelReservationRequestValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
        }
    }
}
