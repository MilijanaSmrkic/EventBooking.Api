using EventBooking.Domain.Entities;

namespace EventBooking.Application.Abstractions
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}
