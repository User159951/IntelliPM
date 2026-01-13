using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace IntelliPM.API.Controllers;

/// <summary>
/// Controller for providing lookup/reference data endpoints.
/// Returns static reference data like project types, task statuses, priorities, etc.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class LookupsController : BaseApiController
{
    /// <summary>
    /// Get all project types with metadata
    /// </summary>
    /// <returns>List of project types</returns>
    [HttpGet("project-types")]
    [ProducesResponseType(typeof(LookupResponse), StatusCodes.Status200OK)]
    public IActionResult GetProjectTypes()
    {
        var items = new[]
        {
            new LookupItem
            {
                Value = "Scrum",
                Label = "Scrum",
                DisplayOrder = 1,
                Metadata = new LookupMetadata
                {
                    Color = "blue",
                    Icon = "calendar"
                }
            },
            new LookupItem
            {
                Value = "Kanban",
                Label = "Kanban",
                DisplayOrder = 2,
                Metadata = new LookupMetadata
                {
                    Color = "green",
                    Icon = "columns"
                }
            },
            new LookupItem
            {
                Value = "Waterfall",
                Label = "Waterfall",
                DisplayOrder = 3,
                Metadata = new LookupMetadata
                {
                    Color = "purple",
                    Icon = "water"
                }
            }
        };

        return Ok(new LookupResponse { Items = items });
    }

    /// <summary>
    /// Get all task statuses with metadata
    /// </summary>
    /// <returns>List of task statuses</returns>
    [HttpGet("task-statuses")]
    [ProducesResponseType(typeof(LookupResponse), StatusCodes.Status200OK)]
    public IActionResult GetTaskStatuses()
    {
        var items = new[]
        {
            new LookupItem
            {
                Value = "Todo",
                Label = "Todo",
                DisplayOrder = 1,
                Metadata = new LookupMetadata
                {
                    Color = "gray",
                    BgColor = "bg-muted",
                    TextColor = "text-muted-foreground"
                }
            },
            new LookupItem
            {
                Value = "InProgress",
                Label = "In Progress",
                DisplayOrder = 2,
                Metadata = new LookupMetadata
                {
                    Color = "blue",
                    BgColor = "bg-blue-500/10",
                    TextColor = "text-blue-500"
                }
            },
            new LookupItem
            {
                Value = "Blocked",
                Label = "Blocked",
                DisplayOrder = 3,
                Metadata = new LookupMetadata
                {
                    Color = "red",
                    BgColor = "bg-red-500/10",
                    TextColor = "text-red-500"
                }
            },
            new LookupItem
            {
                Value = "Done",
                Label = "Done",
                DisplayOrder = 4,
                Metadata = new LookupMetadata
                {
                    Color = "green",
                    BgColor = "bg-green-500/10",
                    TextColor = "text-green-500"
                }
            }
        };

        return Ok(new LookupResponse { Items = items });
    }

    /// <summary>
    /// Get all task priorities with metadata
    /// </summary>
    /// <returns>List of task priorities</returns>
    [HttpGet("task-priorities")]
    [ProducesResponseType(typeof(LookupResponse), StatusCodes.Status200OK)]
    public IActionResult GetTaskPriorities()
    {
        var items = new[]
        {
            new LookupItem
            {
                Value = "Low",
                Label = "Low",
                DisplayOrder = 1,
                Metadata = new LookupMetadata
                {
                    Color = "slate",
                    BgColor = "bg-slate-500/10",
                    TextColor = "text-slate-500"
                }
            },
            new LookupItem
            {
                Value = "Medium",
                Label = "Medium",
                DisplayOrder = 2,
                Metadata = new LookupMetadata
                {
                    Color = "blue",
                    BgColor = "bg-blue-500/10",
                    TextColor = "text-blue-500"
                }
            },
            new LookupItem
            {
                Value = "High",
                Label = "High",
                DisplayOrder = 3,
                Metadata = new LookupMetadata
                {
                    Color = "orange",
                    BgColor = "bg-orange-500/10",
                    TextColor = "text-orange-500"
                }
            },
            new LookupItem
            {
                Value = "Critical",
                Label = "Critical",
                DisplayOrder = 4,
                Metadata = new LookupMetadata
                {
                    Color = "red",
                    BgColor = "bg-red-500/10",
                    TextColor = "text-red-500"
                }
            }
        };

        return Ok(new LookupResponse { Items = items });
    }
}

/// <summary>
/// Lookup item with value, label, and optional metadata
/// </summary>
public class LookupItem
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int? DisplayOrder { get; set; }
    public LookupMetadata? Metadata { get; set; }
}

/// <summary>
/// Lookup metadata for styling and display
/// </summary>
public class LookupMetadata
{
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public string? BgColor { get; set; }
    public string? TextColor { get; set; }
    public string? BorderColor { get; set; }
    public string? DotColor { get; set; }
}

/// <summary>
/// Response containing lookup items
/// </summary>
public class LookupResponse
{
    public LookupItem[] Items { get; set; } = Array.Empty<LookupItem>();
}
