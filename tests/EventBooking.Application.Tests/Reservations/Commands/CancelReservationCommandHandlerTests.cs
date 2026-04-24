using EventBooking.Application.Abstractions;
using EventBooking.Application.Abstractions.Repositories;
using EventBooking.Application.Reservations.Commands.CancelReservation;
using EventBooking.Application.Reservations.Events;
using EventBooking.Domain.Entities;
using MediatR;
using Moq;

namespace EventBooking.Application.Tests.Reservations.Commands;

public class CancelReservationCommandHandlerTests
{
    private readonly Mock<IReservationRepository> _reservationRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPublisher> _publisher = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly CancelReservationCommandHandler _handler;

    public CancelReservationCommandHandlerTests()
    {
        _currentUser.Setup(s => s.UserId).Returns(1);
        _currentUser.Setup(s => s.IsAdmin).Returns(false);

        _handler = new CancelReservationCommandHandler(
            _reservationRepo.Object, _uow.Object, _publisher.Object, _currentUser.Object);

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _publisher
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ShouldCancelReservationAndPublishEvent()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), 1, [1, 2]);
        _reservationRepo.Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        await _handler.Handle(new CancelReservationCommand(reservation.Id), CancellationToken.None);

        Assert.Equal(ReservationStatuses.Cancelled, reservation.StatusCode);
        Assert.Equal(1, reservation.CancelledByUserId);
        Assert.NotNull(reservation.CancelledAt);
        _publisher.Verify(p => p.Publish(
            It.Is<ReservationCancelledEvent>(e => e.FreedSeats == 2 && e.EventId == reservation.EventId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotPublishEvent_WhenCancellingWaitlistReservation()
    {
        var reservation = Reservation.CreateWaitlist(Guid.NewGuid(), 1, [1, 2]);
        _reservationRepo.Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        await _handler.Handle(new CancelReservationCommand(reservation.Id), CancellationToken.None);

        Assert.Equal(ReservationStatuses.Cancelled, reservation.StatusCode);
        _publisher.Verify(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserDoesNotOwnReservation()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), 99, [1]);
        _reservationRepo.Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(new CancelReservationCommand(reservation.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldAllow_WhenAdminCancelsAnyReservation()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), 99, [1]);
        _reservationRepo.Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        _currentUser.Setup(s => s.IsAdmin).Returns(true);

        await _handler.Handle(new CancelReservationCommand(reservation.Id), CancellationToken.None);

        Assert.Equal(ReservationStatuses.Cancelled, reservation.StatusCode);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenReservationNotFound()
    {
        _reservationRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new CancelReservationCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenAlreadyCancelled()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), 1, [1]);
        reservation.Cancel();
        _reservationRepo.Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new CancelReservationCommand(reservation.Id), CancellationToken.None));
    }
}
