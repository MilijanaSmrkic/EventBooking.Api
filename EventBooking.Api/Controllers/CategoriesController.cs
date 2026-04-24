using EventBooking.Application.Categories.Queries.GetCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.Api.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly ISender _sender;

        public CategoriesController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>Returns all event categories.</summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<GetCategoriesResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetCategoriesQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
