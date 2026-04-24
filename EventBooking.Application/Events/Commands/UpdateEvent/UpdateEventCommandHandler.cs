using EventBooking.Application.Abstractions;
using EventBooking.Application.Abstractions.Repositories;
using MediatR;

namespace EventBooking.Application.Events.Commands.UpdateEvent
{
    internal sealed class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, Unit>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateEventCommandHandler(IEventRepository eventRepository, IUnitOfWork unitOfWork)
        {
            _eventRepository = eventRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            var @event = await _eventRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Event with id {request.Id} was not found.");

            if (!await _eventRepository.LocationExistsAsync(request.LocationId, cancellationToken))
                throw new KeyNotFoundException($"Location with id {request.LocationId} was not found.");

            if (request.CategoryId.HasValue &&
                !await _eventRepository.CategoryExistsAsync(request.CategoryId.Value, cancellationToken))
                throw new KeyNotFoundException($"Category with id {request.CategoryId} was not found.");

            @event.Update(
                request.Title,
                request.Description,
                request.LocationId,
                request.CategoryId,
                request.EventDate,
                request.BasePrice,
                request.Capacity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
