namespace EventBooking.Application.Locations.Queries.GetLocations
{
    public record GetLocationsResponse
    {
        public int Id { get; init; }
        public string City { get; init; } = string.Empty;
        public string PostalCode { get; init; } = string.Empty;
    }
}
