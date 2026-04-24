namespace EventBooking.Domain.Entities
{
    public class Location
    {
        public int Id { get; set; }
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
    }
}
