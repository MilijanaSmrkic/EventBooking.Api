using EventBooking.Application.Abstractions;
using EventBooking.Application.Abstractions.Repositories;
using EventBooking.Domain.Entities;
using MediatR;

namespace EventBooking.Application.Events.Commands.Create
{
    internal sealed class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateEventCommandHandler(IEventRepository eventRepository, IUnitOfWork unitOfWork)
        {
            _eventRepository = eventRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            if (!await _eventRepository.LocationExistsAsync(request.LocationId, cancellationToken))
                throw new KeyNotFoundException($"Location with id {request.LocationId} was not found.");

            if (request.CategoryId.HasValue &&
                !await _eventRepository.CategoryExistsAsync(request.CategoryId.Value, cancellationToken))
                throw new KeyNotFoundException($"Category with id {request.CategoryId} was not found.");

            var @event = Event.Create(
                request.Title,
                request.Description,
                request.LocationId,
                request.CategoryId,
                request.EventDate,
                request.BasePrice,
                request.Capacity);

            _eventRepository.Add(@event);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return @event.Id;
        }
    }
}
