using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ApplicationException = IntelliPM.Application.Common.Exceptions.ApplicationException;

namespace IntelliPM.Application.Organizations.Commands;

/// <summary>
/// Handler for DeleteOrganizationCommand.
/// Deletes an organization (SuperAdmin only).
/// Prevents deletion if organization has users (due to FK constraint).
/// </summary>
public class DeleteOrganizationCommandHandler : IRequestHandler<DeleteOrganizationCommand, DeleteOrganizationResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteOrganizationCommandHandler> _logger;

    public DeleteOrganizationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeleteOrganizationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<DeleteOrganizationResponse> Handle(DeleteOrganizationCommand request, CancellationToken ct)
    {
        // Verify SuperAdmin permissions
        if (!_currentUserService.IsSuperAdmin())
        {
            throw new UnauthorizedException("Only SuperAdmin can delete organizations");
        }

        var orgRepo = _unitOfWork.Repository<Organization>();
        var organization = await orgRepo.GetByIdAsync(request.OrganizationId, ct);

        if (organization == null)
        {
            throw new NotFoundException($"Organization {request.OrganizationId} not found");
        }

        // Check if organization has users (FK constraint will prevent deletion, but we check for better error message)
        var userCount = await _unitOfWork.Repository<User>()
            .Query()
            .AsNoTracking()
            .CountAsync(u => u.OrganizationId == request.OrganizationId, ct);

        if (userCount > 0)
        {
            throw new ApplicationException(
                $"Cannot delete organization {request.OrganizationId} because it has {userCount} user(s). " +
                "Please remove or reassign all users before deleting the organization.");
        }

        orgRepo.Delete(organization);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Organization deleted: {OrganizationId}, Name: {Name} by SuperAdmin {UserId}",
            organization.Id, organization.Name, _currentUserService.GetUserId());

        return new DeleteOrganizationResponse(
            request.OrganizationId,
            true,
            "Organization deleted successfully"
        );
    }
}

