using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.AI.DTOs;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Handler for creating a new AI quota template.
/// </summary>
public class CreateAIQuotaTemplateCommandHandler : IRequestHandler<CreateAIQuotaTemplateCommand, AIQuotaTemplateDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateAIQuotaTemplateCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<AIQuotaTemplateDto> Handle(CreateAIQuotaTemplateCommand request, CancellationToken ct)
    {
        // Verify admin permissions
        if (!_currentUserService.IsSuperAdmin())
        {
            throw new UnauthorizedException("Only SuperAdmin can create quota templates");
        }

        // Check if template with same name already exists
        var existingTemplate = await _unitOfWork.Repository<AIQuotaTemplate>()
            .Query()
            .FirstOrDefaultAsync(t => t.TierName == request.TierName && t.DeletedAt == null, ct);

        if (existingTemplate != null)
        {
            throw new ValidationException($"A template with tier name '{request.TierName}' already exists");
        }

        var now = DateTimeOffset.UtcNow;
        var template = new AIQuotaTemplate
        {
            TierName = request.TierName,
            Description = request.Description,
            IsActive = true,
            IsSystemTemplate = false, // Only seeded templates are system templates
            MaxTokensPerPeriod = request.MaxTokensPerPeriod,
            MaxRequestsPerPeriod = request.MaxRequestsPerPeriod,
            MaxDecisionsPerPeriod = request.MaxDecisionsPerPeriod,
            MaxCostPerPeriod = request.MaxCostPerPeriod,
            AllowOverage = request.AllowOverage,
            OverageRate = request.OverageRate,
            DefaultAlertThresholdPercentage = request.DefaultAlertThresholdPercentage,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _unitOfWork.Repository<AIQuotaTemplate>().AddAsync(template, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new AIQuotaTemplateDto(
            template.Id,
            template.TierName,
            template.Description,
            template.IsActive,
            template.IsSystemTemplate,
            template.MaxTokensPerPeriod,
            template.MaxRequestsPerPeriod,
            template.MaxDecisionsPerPeriod,
            template.MaxCostPerPeriod,
            template.AllowOverage,
            template.OverageRate,
            template.DefaultAlertThresholdPercentage,
            template.DisplayOrder,
            template.CreatedAt,
            template.UpdatedAt
        );
    }
}

