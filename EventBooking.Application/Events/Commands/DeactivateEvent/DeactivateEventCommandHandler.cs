using EventBooking.Application.Abstractions;
using EventBooking.Application.Abstractions.Repositories;
using MediatR;

namespace EventBooking.Application.Events.Commands.DeactivateEvent
{
    internal sealed class DeactivateEventCommandHandler : IRequestHandler<DeactivateEventCommand, Unit>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateEventCommandHandler(IEventRepository eventRepository, IUnitOfWork unitOfWork)
        {
            _eventRepository = eventRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeactivateEventCommand request, CancellationToken cancellationToken)
        {
            var @event = await _eventRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Event with id {request.Id} was not found.");

            @event.Deactivate();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
