using EventBooking.Application.Abstractions;
using EventBooking.Application.Abstractions.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventBooking.Application.Events.Commands.DeactivateEvent
{
    internal sealed class DeactivateEventCommandHandler : IRequestHandler<DeactivateEventCommand, Unit>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeactivateEventCommandHandler> _logger;

        public DeactivateEventCommandHandler(
            IEventRepository eventRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeactivateEventCommandHandler> logger)
        {
            _eventRepository = eventRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeactivateEventCommand request, CancellationToken cancellationToken)
        {
            var @event = await _eventRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Event with id {request.Id} was not found.");

            @event.Deactivate();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Event {EventId} '{Title}' deactivated",
                @event.Id, @event.Title);

            return Unit.Value;
        }
    }
}
