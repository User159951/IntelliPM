using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Search.Queries;
using System.Security.Claims;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class SearchController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<SearchController> _logger;

    public SearchController(IMediator mediator, ILogger<SearchController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Global search across projects, tasks, and users
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new SearchResponse(new List<SearchResultDto>()));
            }

            // Filter out special characters that could cause issues
            var sanitizedQuery = q.Trim();

            var query = new SearchQuery(q.Trim(), limit);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing search");
            return Problem(
                title: "Error performing search",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
