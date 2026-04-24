using EventBooking.Application.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EventBooking.Infrastructure.Services;

internal sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public int UserId =>
        int.TryParse(
            _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var id) ? id : 0;

    public bool IsAdmin =>
        _httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;
}
