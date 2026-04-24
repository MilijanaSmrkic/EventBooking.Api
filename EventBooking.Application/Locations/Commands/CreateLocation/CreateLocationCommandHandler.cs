using EventBooking.Application.Abstractions;
using EventBooking.Application.Abstractions.Repositories;
using EventBooking.Domain.Entities;
using MediatR;

namespace EventBooking.Application.Locations.Commands.CreateLocation
{
    internal sealed class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, int>
    {
        private readonly ILocationRepository _locationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateLocationCommandHandler(ILocationRepository locationRepository, IUnitOfWork unitOfWork)
        {
            _locationRepository = locationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
        {
            var location = new Location
            {
                City = request.City,
                PostalCode = request.PostalCode
            };

            _locationRepository.Add(location);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return location.Id;
        }
    }
}
