namespace EventBooking.Application.Abstractions;

public interface ICurrentUserService
{
    int UserId { get; }
    bool IsAdmin { get; }
}
