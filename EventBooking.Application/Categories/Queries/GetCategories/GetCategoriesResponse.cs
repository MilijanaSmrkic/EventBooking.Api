namespace EventBooking.Application.Categories.Queries.GetCategories
{
    public record GetCategoriesResponse
    {
        public int Id { get; init; }
        public string? Name { get; init; }
    }
}
