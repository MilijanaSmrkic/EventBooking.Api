namespace EventBooking.Application.Events.Queries.GetEvents
{
    public record GetEventsResponse
    {
        public Guid Id { get; init; }
        public string? Title { get; init; }
        public int LocationId { get; init; }
        public string? City { get; init; }
        public string? PostalCode { get; init; }
        public int? CategoryId { get; init; }
        public string? CategoryName { get; init; }
        public DateTime? EventDate { get; init; }
        public decimal BasePrice { get; init; }
        public int Capacity { get; init; }
        public bool IsActive { get; init; }
    }
}
