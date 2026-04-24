namespace EventBooking.Application.Events.Commands.UpdateEvent
{
    public class UpdateEventRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int LocationId { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? EventDate { get; set; }
        public decimal BasePrice { get; set; }
        public int Capacity { get; set; }
    }
}
