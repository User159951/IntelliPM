using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Handler for exporting AI decisions to CSV format.
/// </summary>
public class ExportAIDecisionsQueryHandler : IRequestHandler<ExportAIDecisionsQuery, string>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExportAIDecisionsQueryHandler> _logger;

    public ExportAIDecisionsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<ExportAIDecisionsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<string> Handle(ExportAIDecisionsQuery request, CancellationToken ct)
    {
        var query = _unitOfWork.Repository<AIDecisionLog>()
            .Query()
            .AsNoTracking()
            .Where(d => d.CreatedAt >= request.StartDate && d.CreatedAt <= request.EndDate);

        if (request.OrganizationId.HasValue)
        {
            query = query.Where(d => d.OrganizationId == request.OrganizationId.Value);
        }

        var decisions = await query
            .OrderBy(d => d.CreatedAt)
            .ToListAsync(ct);

        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("DecisionId,OrganizationId,DecisionType,AgentType,EntityType,EntityId,EntityName,Question,Decision,ConfidenceScore,Status,RequiresApproval,ApprovedByHuman,TokensUsed,CreatedAt");

        // Rows
        foreach (var decision in decisions)
        {
            csv.AppendLine($"{decision.DecisionId}," +
                          $"{decision.OrganizationId}," +
                          $"{EscapeCsv(decision.DecisionType)}," +
                          $"{EscapeCsv(decision.AgentType)}," +
                          $"{EscapeCsv(decision.EntityType)}," +
                          $"{decision.EntityId}," +
                          $"{EscapeCsv(decision.EntityName)}," +
                          $"{EscapeCsv(decision.Question)}," +
                          $"{EscapeCsv(decision.Decision)}," +
                          $"{decision.ConfidenceScore}," +
                          $"{EscapeCsv(decision.Status)}," +
                          $"{decision.RequiresHumanApproval}," +
                          $"{decision.ApprovedByHuman}," +
                          $"{decision.TokensUsed}," +
                          $"{decision.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        return csv.ToString();
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // If value contains comma, quote, or newline, wrap in quotes and escape quotes
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}

