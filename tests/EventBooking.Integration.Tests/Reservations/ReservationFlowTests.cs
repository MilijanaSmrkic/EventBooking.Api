using EventBooking.Application.Abstractions;
using EventBooking.Application.Reservations.Commands.CancelReservation;
using EventBooking.Application.Reservations.Commands.CreateReservation;
using EventBooking.Application.Reservations.Events;
using EventBooking.Domain.Entities;
using EventBooking.Infrastructure.Persistence;
using EventBooking.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EventBooking.Integration.Tests.Reservations;

public class ReservationFlowTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly EventRepository _eventRepo;
    private readonly ReservationRepository _reservationRepo;
    private readonly Mock<IPublisher> _publisher;
    private readonly Mock<ICurrentUserService> _currentUser;

    public ReservationFlowTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _eventRepo = new EventRepository(_context);
        _reservationRepo = new ReservationRepository(_context);

        _publisher = new Mock<IPublisher>();
        _publisher
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _currentUser = new Mock<ICurrentUserService>();
        _currentUser.Setup(s => s.UserId).Returns(1);
        _currentUser.Setup(s => s.IsAdmin).Returns(false);
    }

    public void Dispose() => _context.Dispose();

    private async Task<(Event @event, User user)> SeedAsync(int capacity = 10)
    {
        var location = new Location { Id = 1, City = "Beograd", PostalCode = "11000" };
        var user = new User { Id = 1, UserName = "korisnik", Email = "k@test.com", PasswordHash = "x", Role = "User" };
        _context.Locations.Add(location);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var @event = Event.Create("Test Event", null, 1, null, DateTime.UtcNow.AddDays(30), 50m, capacity);
        _context.Events.Add(@event);
        await _context.SaveChangesAsync();

        return (@event, user);
    }

    private CreateReservationCommandHandler CreateHandler() =>
        new(_eventRepo, _reservationRepo, _context, _context,
            NullLogger<CreateReservationCommandHandler>.Instance);

    private CancelReservationCommandHandler CancelHandler() =>
        new(_reservationRepo, _context, _publisher.Object, _currentUser.Object,
            NullLogger<CancelReservationCommandHandler>.Instance);

    [Fact]
    public async Task CreateReservation_ShouldBePending_WhenCapacityAvailable()
    {
        var (@event, user) = await SeedAsync(capacity: 10);

        var handler = CreateHandler();
        var id = await handler.Handle(
            new CreateReservationCommand(@event.Id, user.Id, [1, 2, 3]),
            CancellationToken.None);

        var saved = await _context.Reservations.FindAsync(id);
        Assert.NotNull(saved);
        Assert.Equal(ReservationStatuses.Pending, saved.StatusCode);
        Assert.Equal(3, saved.SeatCount);
    }

    [Fact]
    public async Task CreateReservation_ShouldBeWaitlist_WhenCapacityExceeded()
    {
        var (@event, user) = await SeedAsync(capacity: 2);

        var handler = CreateHandler();
        await handler.Handle(new CreateReservationCommand(@event.Id, user.Id, [1, 2]), CancellationToken.None);

        var id = await handler.Handle(
            new CreateReservationCommand(@event.Id, user.Id, [3, 4]),
            CancellationToken.None);

        var saved = await _context.Reservations.FindAsync(id);
        Assert.Equal(ReservationStatuses.Waitlist, saved!.StatusCode);
    }

    [Fact]
    public async Task CreateReservation_ShouldThrow_WhenSeatAlreadyReserved()
    {
        var (@event, user) = await SeedAsync(capacity: 10);
        var handler = CreateHandler();

        await handler.Handle(new CreateReservationCommand(@event.Id, user.Id, [1, 2]), CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CreateReservationCommand(@event.Id, user.Id, [2, 3]), CancellationToken.None));
    }

    [Fact]
    public async Task CancelPendingReservation_ShouldPromoteWaitlistAndPublishEvent()
    {
        var (@event, user) = await SeedAsync(capacity: 2);
        var createHandler = CreateHandler();

        var pendingId = await createHandler.Handle(
            new CreateReservationCommand(@event.Id, user.Id, [1, 2]),
            CancellationToken.None);

        await createHandler.Handle(
            new CreateReservationCommand(@event.Id, user.Id, [3, 4]),
            CancellationToken.None);

        var cancelHandler = CancelHandler();
        await cancelHandler.Handle(new CancelReservationCommand(pendingId), CancellationToken.None);

        _publisher.Verify(p => p.Publish(
            It.Is<ReservationCancelledEvent>(e => e.FreedSeats == 2 && e.EventId == @event.Id),
            It.IsAny<CancellationToken>()), Times.Once);

        var pending = await _context.Reservations.FindAsync(pendingId);
        Assert.Equal(ReservationStatuses.Cancelled, pending!.StatusCode);
        Assert.Equal(1, pending.CancelledByUserId);
        Assert.NotNull(pending.CancelledAt);
    }

    [Fact]
    public async Task CancelWaitlistReservation_ShouldNotPublishEvent()
    {
        var (@event, user) = await SeedAsync(capacity: 1);
        var createHandler = CreateHandler();

        await createHandler.Handle(new CreateReservationCommand(@event.Id, user.Id, [1]), CancellationToken.None);

        var waitlistId = await createHandler.Handle(
            new CreateReservationCommand(@event.Id, user.Id, [2]),
            CancellationToken.None);

        await CancelHandler().Handle(new CancelReservationCommand(waitlistId), CancellationToken.None);

        _publisher.Verify(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WaitlistPromotion_ShouldAutoApprove_WhenCapacityFreed()
    {
        var (@event, user) = await SeedAsync(capacity: 2);

        var user2 = new User { Id = 2, UserName = "korisnik2", Email = "k2@test.com", PasswordHash = "x", Role = "User" };
        _context.Users.Add(user2);
        await _context.SaveChangesAsync();

        var createHandler = CreateHandler();

        var pendingId = await createHandler.Handle(
            new CreateReservationCommand(@event.Id, user.Id, [1, 2]),
            CancellationToken.None);

        var waitlistId = await createHandler.Handle(
            new CreateReservationCommand(@event.Id, user2.Id, [3, 4]),
            CancellationToken.None);

        var cancelHandler = CancelHandler();
        await cancelHandler.Handle(new CancelReservationCommand(pendingId), CancellationToken.None);

        var waitlistReservation = await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Event)
            .FirstAsync(r => r.Id == waitlistId);

        var promotionHandler = new ReservationCancelledEventHandler(
            _reservationRepo, _context, _publisher.Object,
            NullLogger<ReservationCancelledEventHandler>.Instance);

        await promotionHandler.Handle(
            new ReservationCancelledEvent(pendingId, @event.Id, 2),
            CancellationToken.None);

        var promoted = await _context.Reservations.FindAsync(waitlistId);
        Assert.Equal(ReservationStatuses.Confirmed, promoted!.StatusCode);
    }
}
