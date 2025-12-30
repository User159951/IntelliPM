using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using IntelliPM.Infrastructure.Persistence;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AlertsController : BaseApiController
{
    private readonly AppDbContext _dbContext;

    public AlertsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Get alerts for the current user
    /// </summary>
    /// <param name="unreadOnly">Filter to show only unread alerts (default: true)</param>
    /// <param name="limit">Maximum number of alerts to return (default: 10, max: 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of alerts</returns>
    /// <response code="200">Alerts retrieved successfully</response>
    /// <response code="500">Error retrieving alerts</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] bool unreadOnly = true,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0 || limit > 50)
        {
            limit = 10;
        }

        var query = _dbContext.Alerts
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .AsQueryable();

        if (unreadOnly)
        {
            query = query.Where(a => !a.IsRead);
        }

        var alerts = await query
            .Take(limit)
            .Select(a => new
            {
                a.Id,
                a.ProjectId,
                a.Type,
                a.Severity,
                a.Title,
                a.Message,
                a.IsRead,
                a.IsResolved,
                a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(alerts);
    }

    /// <summary>
    /// Mark an alert as read
    /// </summary>
    /// <param name="id">Alert ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">Alert marked as read successfully</response>
    /// <response code="404">Alert not found</response>
    /// <response code="500">Error updating alert</response>
    [HttpPost("{id:int}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken = default)
    {
        var alert = await _dbContext.Alerts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (alert == null)
        {
            return NotFound();
        }

        if (!alert.IsRead)
        {
            alert.IsRead = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return NoContent();
    }
}

