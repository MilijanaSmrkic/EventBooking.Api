namespace EventBooking.Application.Models
{
    public record EmailMessage
    {
        public string To { get; init; } = string.Empty;
        public string Subject { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
    }
}
