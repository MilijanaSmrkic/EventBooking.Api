namespace EventBooking.Application.Auth
{
    public record LoginResponse
    {
        public string Token { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public DateTime ExpiresAt { get; init; }
    }
}
