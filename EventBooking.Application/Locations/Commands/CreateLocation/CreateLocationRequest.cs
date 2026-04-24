namespace EventBooking.Application.Locations.Commands.CreateLocation
{
    public class CreateLocationRequest
    {
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
    }
}
