using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Tasks.Commands;

/// <summary>
/// Handler for removing a task dependency.
/// Verifies the dependency exists and belongs to the user's organization (multi-tenancy).
/// </summary>
public class RemoveTaskDependencyCommandHandler : IRequestHandler<RemoveTaskDependencyCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RemoveTaskDependencyCommandHandler> _logger;

    public RemoveTaskDependencyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<RemoveTaskDependencyCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(RemoveTaskDependencyCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            throw new UnauthorizedException("Organization ID not found");
        }

        // Get dependency and verify it exists and belongs to user's organization
        var dependency = await _unitOfWork.Repository<TaskDependency>()
            .Query()
            .FirstOrDefaultAsync(
                d => d.Id == request.DependencyId && d.OrganizationId == organizationId,
                cancellationToken);

        if (dependency == null)
        {
            throw new NotFoundException($"Dependency with ID {request.DependencyId} not found");
        }

        // Hard delete
        _unitOfWork.Repository<TaskDependency>().Delete(dependency);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Task dependency {DependencyId} removed (Task {SourceTaskId} -> Task {DependentTaskId})",
            dependency.Id, dependency.SourceTaskId, dependency.DependentTaskId);

        return Unit.Value;
    }
}

