namespace EventBooking.Application.Models
{
    public record ReservationApprovedEmailOptions
    {
        public const string SectionName = "EmailTemplates:ReservationApproved";

        public string Subject { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
    }
}
