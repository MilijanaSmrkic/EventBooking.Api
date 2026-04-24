using EventBooking.Application.Abstractions;
using EventBooking.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventBooking.Application.Auth
{
    internal sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;

        public LoginCommandHandler(
            IApplicationDbContext context,
            ITokenService tokenService,
            IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken)
                ?? throw new UnauthorizedAccessException("Invalid username or password.");

            if (!_tokenService.VerifyPassword(request.Password, user.PasswordHash!))
                throw new UnauthorizedAccessException("Invalid username or password.");

            var token = _tokenService.GenerateToken(user);

            return new LoginResponse
            {
                Token = token,
                UserName = user.UserName!,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
            };
        }
    }
}
