using EventBooking.Application.Abstractions;
using EventBooking.Application.Abstractions.Repositories;
using EventBooking.Application.Reservations.Commands.CreateReservation;
using EventBooking.Application.Tests.Helpers;
using EventBooking.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EventBooking.Application.Tests.Reservations.Commands;

public class CreateReservationCommandHandlerTests
{
    private readonly Mock<IEventRepository> _eventRepo = new();
    private readonly Mock<IReservationRepository> _reservationRepo = new();
    private readonly Mock<IApplicationDbContext> _ctx = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly CreateReservationCommandHandler _handler;

    public CreateReservationCommandHandlerTests()
    {
        _handler = new CreateReservationCommandHandler(
            _eventRepo.Object,
            _reservationRepo.Object,
            _ctx.Object,
            _uow.Object,
            NullLogger<CreateReservationCommandHandler>.Instance);

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_ShouldCreatePendingReservation_WhenCapacityAvailable()
    {
        var @event = Event.Create("Concert", null, 1, null, null, 50m, 100);
        var user = new User { Id = 1, Email = "user@test.com" };
        Reservation? added = null;

        _eventRepo.Setup(r => r.GetByIdAsync(@event.Id, It.IsAny<CancellationToken>())).ReturnsAsync(@event);
        _ctx.Setup(c => c.Users).Returns(DbSetMockHelper.BuildMock([user]).Object);
        _reservationRepo.Setup(r => r.GetUsedSeatsAsync(@event.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _reservationRepo.Setup(r => r.GetOccupiedSeatNumbersAsync(@event.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new List<int>());
        _reservationRepo.Setup(r => r.Add(It.IsAny<Reservation>())).Callback<Reservation>(r => added = r);

        var result = await _handler.Handle(
            new CreateReservationCommand(@event.Id, user.Id, [1, 2, 3]),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(added);
        Assert.Equal(ReservationStatuses.Pending, added.StatusCode);
        Assert.Equal(3, added.SeatCount);
    }

    [Fact]
    public async Task Handle_ShouldCreateWaitlistReservation_WhenCapacityExceeded()
    {
        // Capacity = 3, all seats taken → next request goes to waitlist
        var @event = Event.Create("Full Event", null, 1, null, null, 50m, 3);
        var user = new User { Id = 1, Email = "user@test.com" };
        Reservation? added = null;

        _eventRepo.Setup(r => r.GetByIdAsync(@event.Id, It.IsAny<CancellationToken>())).ReturnsAsync(@event);
        _ctx.Setup(c => c.Users).Returns(DbSetMockHelper.BuildMock([user]).Object);
        _reservationRepo.Setup(r => r.GetUsedSeatsAsync(@event.Id, It.IsAny<CancellationToken>())).ReturnsAsync(3);
        _reservationRepo.Setup(r => r.Add(It.IsAny<Reservation>())).Callback<Reservation>(r => added = r);

        await _handler.Handle(
            new CreateReservationCommand(@event.Id, user.Id, [5, 6]),
            CancellationToken.None);

        Assert.NotNull(added);
        Assert.Equal(ReservationStatuses.Waitlist, added.StatusCode);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenEventNotFound()
    {
        var eventId = Guid.NewGuid();
        _eventRepo.Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync((Event?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new CreateReservationCommand(eventId, 1, [1]), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenUserNotFound()
    {
        var @event = Event.Create("Concert", null, 1, null, null, 50m, 100);

        _eventRepo.Setup(r => r.GetByIdAsync(@event.Id, It.IsAny<CancellationToken>())).ReturnsAsync(@event);
        _ctx.Setup(c => c.Users).Returns(DbSetMockHelper.BuildMock<User>([]).Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new CreateReservationCommand(@event.Id, 999, [1]), CancellationToken.None));
    }
}
