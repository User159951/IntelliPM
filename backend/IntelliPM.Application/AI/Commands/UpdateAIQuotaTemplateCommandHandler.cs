using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.AI.DTOs;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Handler for updating an existing AI quota template.
/// </summary>
public class UpdateAIQuotaTemplateCommandHandler : IRequestHandler<UpdateAIQuotaTemplateCommand, AIQuotaTemplateDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateAIQuotaTemplateCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<AIQuotaTemplateDto> Handle(UpdateAIQuotaTemplateCommand request, CancellationToken ct)
    {
        // Verify admin permissions
        if (!_currentUserService.IsSuperAdmin())
        {
            throw new UnauthorizedException("Only SuperAdmin can update quota templates");
        }

        var template = await _unitOfWork.Repository<AIQuotaTemplate>()
            .GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Quota template with ID {request.Id} not found");

        if (template.DeletedAt != null)
        {
            throw new NotFoundException($"Quota template with ID {request.Id} has been deleted");
        }

        // Update fields if provided
        if (request.Description != null)
            template.Description = request.Description;

        if (request.IsActive.HasValue)
            template.IsActive = request.IsActive.Value;

        if (request.MaxTokensPerPeriod.HasValue)
            template.MaxTokensPerPeriod = request.MaxTokensPerPeriod.Value;

        if (request.MaxRequestsPerPeriod.HasValue)
            template.MaxRequestsPerPeriod = request.MaxRequestsPerPeriod.Value;

        if (request.MaxDecisionsPerPeriod.HasValue)
            template.MaxDecisionsPerPeriod = request.MaxDecisionsPerPeriod.Value;

        if (request.MaxCostPerPeriod.HasValue)
            template.MaxCostPerPeriod = request.MaxCostPerPeriod.Value;

        if (request.AllowOverage.HasValue)
            template.AllowOverage = request.AllowOverage.Value;

        if (request.OverageRate.HasValue)
            template.OverageRate = request.OverageRate.Value;

        if (request.DefaultAlertThresholdPercentage.HasValue)
            template.DefaultAlertThresholdPercentage = request.DefaultAlertThresholdPercentage.Value;

        if (request.DisplayOrder.HasValue)
            template.DisplayOrder = request.DisplayOrder.Value;

        template.UpdatedAt = DateTimeOffset.UtcNow;

        _unitOfWork.Repository<AIQuotaTemplate>().Update(template);
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

