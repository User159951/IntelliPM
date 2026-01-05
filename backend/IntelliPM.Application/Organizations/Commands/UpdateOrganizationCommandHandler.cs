using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ApplicationException = IntelliPM.Application.Common.Exceptions.ApplicationException;

namespace IntelliPM.Application.Organizations.Commands;

/// <summary>
/// Handler for UpdateOrganizationCommand.
/// Updates an existing organization (SuperAdmin only).
/// </summary>
public class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand, UpdateOrganizationResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateOrganizationCommandHandler> _logger;

    public UpdateOrganizationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UpdateOrganizationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<UpdateOrganizationResponse> Handle(UpdateOrganizationCommand request, CancellationToken ct)
    {
        // Verify SuperAdmin permissions
        if (!_currentUserService.IsSuperAdmin())
        {
            throw new UnauthorizedException("Only SuperAdmin can update organizations");
        }

        var orgRepo = _unitOfWork.Repository<Organization>();
        var organization = await orgRepo.GetByIdAsync(request.OrganizationId, ct);

        if (organization == null)
        {
            throw new NotFoundException($"Organization {request.OrganizationId} not found");
        }

        // Check if Code is being changed and if new Code already exists
        if (organization.Code != request.Code)
        {
            var existingOrg = await orgRepo.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Code == request.Code && o.Id != request.OrganizationId, ct);

            if (existingOrg != null)
            {
                throw new ApplicationException($"Organization with code '{request.Code}' already exists");
            }
        }

        organization.Name = request.Name;
        organization.Code = request.Code;
        organization.UpdatedAt = DateTimeOffset.UtcNow;

        orgRepo.Update(organization);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Organization updated: {OrganizationId}, Name: {Name}, Code: {Code} by SuperAdmin {UserId}",
            organization.Id, organization.Name, organization.Code, _currentUserService.GetUserId());

        return new UpdateOrganizationResponse(
            organization.Id,
            organization.Name,
            organization.Code,
            organization.UpdatedAt ?? DateTimeOffset.UtcNow
        );
    }
}

