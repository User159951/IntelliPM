using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Settings.Commands;

public class UpdateSettingCommandHandler : IRequestHandler<UpdateSettingCommand, UpdateSettingResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingsService _settingsService;

    public UpdateSettingCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPermissionService permissionService,
        ISettingsService settingsService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _permissionService = permissionService;
        _settingsService = settingsService;
    }

    public async Task<UpdateSettingResponse> Handle(UpdateSettingCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.GetUserId();

        // Admins have all permissions, so skip permission check for them
        if (!_currentUserService.IsAdmin())
        {
            // Check permission for non-admin users
            var hasPermission = await _permissionService.HasPermissionAsync(currentUserId, "admin.settings.update", cancellationToken);
            if (!hasPermission)
            {
                throw new UnauthorizedException("You don't have permission to update settings");
            }
        }

        var settingsRepo = _unitOfWork.Repository<GlobalSetting>();
        var existingSetting = await settingsRepo.Query()
            .FirstOrDefaultAsync(s => s.Key == request.Key, cancellationToken);

        if (existingSetting != null)
        {
            existingSetting.Value = request.Value;
            existingSetting.UpdatedAt = DateTimeOffset.UtcNow;
            existingSetting.UpdatedById = currentUserId;
            if (!string.IsNullOrEmpty(request.Category))
            {
                existingSetting.Category = request.Category;
            }
        }
        else
        {
            var newSetting = new GlobalSetting
            {
                Key = request.Key,
                Value = request.Value,
                Category = request.Category ?? "General",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedById = currentUserId,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            await settingsRepo.AddAsync(newSetting, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache for this setting
        _settingsService.InvalidateCache(request.Key);

        var finalSetting = await settingsRepo.Query()
            .FirstOrDefaultAsync(s => s.Key == request.Key, cancellationToken);

        return new UpdateSettingResponse(
            request.Key,
            request.Value,
            finalSetting?.Category ?? request.Category ?? "General");
    }
}

