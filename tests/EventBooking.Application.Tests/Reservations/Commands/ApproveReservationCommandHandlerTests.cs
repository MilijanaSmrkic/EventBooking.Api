using EventBooking.Application.Abstractions;
using EventBooking.Application.Abstractions.Repositories;
using EventBooking.Application.Reservations.Commands.ApproveReservation;
using EventBooking.Application.Reservations.Events;
using EventBooking.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Reflection;

namespace EventBooking.Application.Tests.Reservations.Commands;

public class ApproveReservationCommandHandlerTests
{
    private readonly Mock<IReservationRepository> _reservationRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPublisher> _publisher = new();
    private readonly ApproveReservationCommandHandler _handler;

    public ApproveReservationCommandHandlerTests()
    {
        _handler = new ApproveReservationCommandHandler(
            _reservationRepo.Object,
            _uow.Object,
            _publisher.Object,
            NullLogger<ApproveReservationCommandHandler>.Instance);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _publisher
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static void SetNav(object target, string property, object? value) =>
        target.GetType()
            .GetProperty(property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(target, value);

    [Fact]
    public async Task Handle_ShouldApproveReservationAndPublishEmailEvent()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), 1, [3, 7]);
        SetNav(reservation, "User", new User { Id = 1, Email = "fan@test.com" });
        SetNav(reservation, "Event", Event.Create("Jazz Night", null, 1, null, null, 30m, 50));

        _reservationRepo.Setup(r => r.GetByIdWithNavigationsAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        await _handler.Handle(new ApproveReservationCommand(reservation.Id), CancellationToken.None);

        Assert.Equal(ReservationStatuses.Confirmed, reservation.StatusCode);
        _publisher.Verify(p => p.Publish(
            It.Is<ReservationApprovedEvent>(e =>
                e.RecipientEmail == "fan@test.com" &&
                e.SeatNumbers.SequenceEqual(new[] { 3, 7 })),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenReservationNotPending()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), 1, [1]);
        reservation.Approve();
        SetNav(reservation, "User", new User { Id = 1, Email = "fan@test.com" });

        _reservationRepo.Setup(r => r.GetByIdWithNavigationsAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new ApproveReservationCommand(reservation.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenUserHasNoEmail()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), 1, [1]);
        SetNav(reservation, "User", new User { Id = 1, Email = null });

        _reservationRepo.Setup(r => r.GetByIdWithNavigationsAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new ApproveReservationCommand(reservation.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenReservationNotFound()
    {
        _reservationRepo.Setup(r => r.GetByIdWithNavigationsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new ApproveReservationCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
