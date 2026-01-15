using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Handler for toggling global AI kill switch.
/// Updates the GlobalSetting "AI.Enabled" to enable/disable AI system-wide.
/// </summary>
public class ToggleGlobalAICommandHandler : IRequestHandler<ToggleGlobalAICommand, ToggleGlobalAIResponse>
{
    private const string GlobalAIEnabledKey = "AI.Enabled";
    private const string CacheKey = "ai_global_enabled";

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ToggleGlobalAICommandHandler> _logger;

    public ToggleGlobalAICommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMemoryCache cache,
        ILogger<ToggleGlobalAICommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ToggleGlobalAIResponse> Handle(ToggleGlobalAICommand request, CancellationToken ct)
    {
        // Verify SuperAdmin permissions
        if (!_currentUserService.IsSuperAdmin())
        {
            throw new UnauthorizedException("Only SuperAdmin can toggle global AI kill switch");
        }

        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == 0)
        {
            throw new UnauthorizedException("User not authenticated");
        }

        _logger.LogWarning(
            "TOGGLING GLOBAL AI KILL SWITCH: Enabled={Enabled}, Reason={Reason}, UserId={UserId}",
            request.Enabled, request.Reason, currentUserId);

        var globalSettingRepo = _unitOfWork.Repository<GlobalSetting>();
        var globalSetting = await globalSettingRepo.Query()
            .FirstOrDefaultAsync(gs => gs.Key == GlobalAIEnabledKey, ct);

        if (globalSetting == null)
        {
            // Create new setting if it doesn't exist
            globalSetting = new GlobalSetting
            {
                Key = GlobalAIEnabledKey,
                Value = request.Enabled.ToString(),
                Description = "Global AI kill switch. When disabled, all AI features are disabled system-wide.",
                Category = "FeatureFlags",
                UpdatedById = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            await globalSettingRepo.AddAsync(globalSetting, ct);
        }
        else
        {
            // Update existing setting
            globalSetting.Value = request.Enabled.ToString();
            globalSetting.UpdatedById = currentUserId;
            globalSetting.UpdatedAt = DateTimeOffset.UtcNow;
            globalSettingRepo.Update(globalSetting);
        }

        // Create audit log
        var auditLog = new AuditLog
        {
            UserId = currentUserId,
            Action = "ToggleGlobalAI",
            EntityType = "GlobalSetting",
            EntityId = globalSetting.Id,
            EntityName = GlobalAIEnabledKey,
            Changes = JsonSerializer.Serialize(new
            {
                Key = GlobalAIEnabledKey,
                Enabled = request.Enabled,
                Reason = request.Reason,
                ToggledAt = DateTimeOffset.UtcNow
            }),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog, ct);

        // Save changes
        await _unitOfWork.SaveChangesAsync(ct);

        // Clear cache to force refresh
        _cache.Remove(CacheKey);

        _logger.LogInformation(
            "Global AI kill switch toggled: Enabled={Enabled}, Reason={Reason}, UserId={UserId}",
            request.Enabled, request.Reason, currentUserId);

        return new ToggleGlobalAIResponse(
            Enabled: request.Enabled,
            ToggledAt: DateTimeOffset.UtcNow,
            Reason: request.Reason,
            UpdatedById: currentUserId
        );
    }
}
