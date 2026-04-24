namespace EventBooking.Domain.Entities
{
    public class Event
    {
        private Event() { }

        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public string? Title { get; private set; }
        public string? Description { get; private set; }
        public int LocationId { get; private set; }
        public int? CategoryId { get; private set; }
        public DateTime? EventDate { get; private set; }
        public decimal BasePrice { get; private set; }
        public int Capacity { get; private set; }
        public DateTime? CreatedAt { get; private set; }
        public bool IsActive { get; private set; }

        public Location? Location { get; private set; }
        public Category? Category { get; private set; }

        public static Event Create(
            string? title,
            string? description,
            int locationId,
            int? categoryId,
            DateTime? eventDate,
            decimal basePrice,
            int capacity)
        {
            return new Event
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                LocationId = locationId,
                CategoryId = categoryId,
                EventDate = eventDate,
                BasePrice = basePrice,
                Capacity = capacity,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }

        public void Update(
            string? title,
            string? description,
            int locationId,
            int? categoryId,
            DateTime? eventDate,
            decimal basePrice,
            int capacity)
        {
            Title = title;
            Description = description;
            LocationId = locationId;
            CategoryId = categoryId;
            EventDate = eventDate;
            BasePrice = basePrice;
            Capacity = capacity;
        }

        public void Deactivate() => IsActive = false;
    }
}
