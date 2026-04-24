using MediatR;

namespace EventBooking.Application.Categories.Queries.GetCategories
{
    public record GetCategoriesQuery : IRequest<List<GetCategoriesResponse>>;
}
