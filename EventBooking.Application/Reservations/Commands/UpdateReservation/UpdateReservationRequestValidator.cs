using FluentValidation;

namespace EventBooking.Application.Reservations.Commands.UpdateReservation
{
    public class UpdateReservationRequestValidator : AbstractValidator<UpdateReservationRequest>
    {
        public UpdateReservationRequestValidator()
        {
            RuleFor(r => r.SeatNumbers).NotEmpty();
        }
    }
}
