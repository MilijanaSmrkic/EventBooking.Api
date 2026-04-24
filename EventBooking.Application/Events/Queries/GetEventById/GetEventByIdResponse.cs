namespace EventBooking.Application.Events.Queries.GetEventById
{
    public record GetEventByIdResponse
    {
        public Guid Id { get; init; }
        public string? Title { get; init; }
        public string? Description { get; init; }
        public int LocationId { get; init; }
        public string? City { get; init; }
        public string? PostalCode { get; init; }
        public int? CategoryId { get; init; }
        public string? CategoryName { get; init; }
        public DateTime? EventDate { get; init; }
        public decimal BasePrice { get; init; }
        public int Capacity { get; init; }
        public DateTime? CreatedAt { get; init; }
        public bool IsActive { get; init; }
    }
}
