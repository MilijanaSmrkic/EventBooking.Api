using EventBooking.Domain.Entities;

namespace EventBooking.Domain.Tests.Entities;

public class EventTests
{
    [Fact]
    public void Create_ShouldReturnActiveEvent_WithCorrectProperties()
    {
        var eventDate = DateTime.UtcNow.AddDays(10);

        var @event = Event.Create("Rock Night", "Great show", 1, 2, eventDate, 49.99m, 200);

        Assert.NotEqual(Guid.Empty, @event.Id);
        Assert.Equal("Rock Night", @event.Title);
        Assert.Equal("Great show", @event.Description);
        Assert.Equal(1, @event.LocationId);
        Assert.Equal(2, @event.CategoryId);
        Assert.Equal(eventDate, @event.EventDate);
        Assert.Equal(49.99m, @event.BasePrice);
        Assert.Equal(200, @event.Capacity);
        Assert.True(@event.IsActive);
        Assert.NotNull(@event.CreatedAt);
    }

    [Fact]
    public void Create_ShouldAllowNullCategory()
    {
        var @event = Event.Create("Free Event", null, 1, null, null, 0m, 50);

        Assert.Null(@event.CategoryId);
        Assert.True(@event.IsActive);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var @event = Event.Create("To Deactivate", null, 1, null, null, 0m, 10);

        @event.Deactivate();

        Assert.False(@event.IsActive);
    }

    [Fact]
    public void Update_ShouldOverwriteAllFields()
    {
        var @event = Event.Create("Old Title", "Old Desc", 1, null, null, 10m, 50);
        var newDate = DateTime.UtcNow.AddDays(5);

        @event.Update("New Title", "New Desc", 2, 3, newDate, 99m, 300);

        Assert.Equal("New Title", @event.Title);
        Assert.Equal("New Desc", @event.Description);
        Assert.Equal(2, @event.LocationId);
        Assert.Equal(3, @event.CategoryId);
        Assert.Equal(newDate, @event.EventDate);
        Assert.Equal(99m, @event.BasePrice);
        Assert.Equal(300, @event.Capacity);
    }

    [Fact]
    public void Update_ShouldNotChangeIsActive()
    {
        var @event = Event.Create("Event", null, 1, null, null, 0m, 10);
        @event.Deactivate();

        @event.Update("New", null, 1, null, null, 0m, 10);

        Assert.False(@event.IsActive);
    }
}
