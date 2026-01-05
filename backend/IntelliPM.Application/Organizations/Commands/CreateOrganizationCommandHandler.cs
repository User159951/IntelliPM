using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ApplicationException = IntelliPM.Application.Common.Exceptions.ApplicationException;

namespace IntelliPM.Application.Organizations.Commands;

/// <summary>
/// Handler for CreateOrganizationCommand.
/// Creates a new organization (SuperAdmin only).
/// </summary>
public class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, CreateOrganizationResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateOrganizationCommandHandler> _logger;

    public CreateOrganizationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateOrganizationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<CreateOrganizationResponse> Handle(CreateOrganizationCommand request, CancellationToken ct)
    {
        // Verify SuperAdmin permissions
        if (!_currentUserService.IsSuperAdmin())
        {
            throw new UnauthorizedException("Only SuperAdmin can create organizations");
        }

        // Check if organization with same Code already exists
        var existingOrg = await _unitOfWork.Repository<Organization>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Code == request.Code, ct);

        if (existingOrg != null)
        {
            throw new ApplicationException($"Organization with code '{request.Code}' already exists");
        }

        var organization = new Organization
        {
            Name = request.Name,
            Code = request.Code,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var orgRepo = _unitOfWork.Repository<Organization>();
        await orgRepo.AddAsync(organization, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Organization created: {OrganizationId}, Name: {Name}, Code: {Code} by SuperAdmin {UserId}",
            organization.Id, organization.Name, organization.Code, _currentUserService.GetUserId());

        return new CreateOrganizationResponse(
            organization.Id,
            organization.Name,
            organization.Code,
            organization.CreatedAt
        );
    }
}

