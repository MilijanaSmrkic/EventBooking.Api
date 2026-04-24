using EventBooking.Domain.Entities;

namespace EventBooking.Domain.Tests.Entities;

public class ReservationTests
{
    private static readonly Guid EventId = Guid.NewGuid();
    private const int UserId = 42;

    [Fact]
    public void Create_ShouldReturnPendingReservation_WithCorrectSeatCount()
    {
        var seats = new List<int> { 1, 2, 3 };

        var reservation = Reservation.Create(EventId, UserId, seats);

        Assert.NotEqual(Guid.Empty, reservation.Id);
        Assert.Equal(EventId, reservation.EventId);
        Assert.Equal(UserId, reservation.UserId);
        Assert.Equal(3, reservation.SeatCount);
        Assert.Equal(seats, reservation.SeatNumbers);
        Assert.Equal(ReservationStatuses.Pending, reservation.StatusCode);
        Assert.True(reservation.ReservationDate <= DateTime.UtcNow);
    }

    [Fact]
    public void CreateWaitlist_ShouldReturnWaitlistStatus_WithCorrectSeatCount()
    {
        var seats = new List<int> { 5, 6 };

        var reservation = Reservation.CreateWaitlist(EventId, UserId, seats);

        Assert.Equal(ReservationStatuses.Waitlist, reservation.StatusCode);
        Assert.Equal(2, reservation.SeatCount);
    }

    [Fact]
    public void Approve_ShouldSetStatusToConfirmed()
    {
        var reservation = Reservation.Create(EventId, UserId, [1, 2]);

        reservation.Approve();

        Assert.Equal(ReservationStatuses.Confirmed, reservation.StatusCode);
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled()
    {
        var reservation = Reservation.Create(EventId, UserId, [1]);

        reservation.Cancel();

        Assert.Equal(ReservationStatuses.Cancelled, reservation.StatusCode);
    }

    [Fact]
    public void Update_ShouldReplaceSeatNumbersAndRecalculateSeatCount()
    {
        var reservation = Reservation.Create(EventId, UserId, [1, 2]);

        reservation.Update([10, 20, 30]);

        Assert.Equal(3, reservation.SeatCount);
        Assert.Equal([10, 20, 30], reservation.SeatNumbers);
    }
}
