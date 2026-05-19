using EventBooking.Application.Abstractions;
using EventBooking.Application.Abstractions.Repositories;
using EventBooking.Application.Events.Commands.Create;
using EventBooking.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EventBooking.Application.Tests.Events.Commands;

public class CreateEventCommandHandlerTests
{
    private readonly Mock<IEventRepository> _eventRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly CreateEventCommandHandler _handler;

    public CreateEventCommandHandlerTests()
    {
        _handler = new CreateEventCommandHandler(
            _eventRepo.Object,
            _uow.Object,
            NullLogger<CreateEventCommandHandler>.Instance);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_ShouldCreateEventAndReturnId_WhenLocationExists()
    {
        Event? added = null;
        _eventRepo.Setup(r => r.LocationExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _eventRepo.Setup(r => r.Add(It.IsAny<Event>())).Callback<Event>(e => added = e);

        var command = new CreateEventCommand("Rock Night", "Desc", 1, null, DateTime.UtcNow.AddDays(5), 50m, 100);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(added);
        Assert.Equal("Rock Night", added.Title);
        Assert.True(added.IsActive);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateEvent_WhenValidCategoryIdProvided()
    {
        _eventRepo.Setup(r => r.LocationExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _eventRepo.Setup(r => r.CategoryExistsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _eventRepo.Setup(r => r.Add(It.IsAny<Event>()));

        var command = new CreateEventCommand("Match", null, 1, 3, null, 20m, 500);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenLocationNotFound()
    {
        _eventRepo.Setup(r => r.LocationExistsAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var command = new CreateEventCommand("Title", null, 999, null, null, 0m, 10);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenCategoryIdProvidedButMissing()
    {
        _eventRepo.Setup(r => r.LocationExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _eventRepo.Setup(r => r.CategoryExistsAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var command = new CreateEventCommand("Title", null, 1, 99, null, 0m, 10);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
