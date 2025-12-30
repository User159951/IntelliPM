using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Tasks.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Tasks.Commands;

/// <summary>
/// Handler for adding a dependency between two tasks.
/// Validates tasks exist, belong to same organization, checks for cycles, and enforces limits.
/// </summary>
public class AddTaskDependencyCommandHandler : IRequestHandler<AddTaskDependencyCommand, TaskDependencyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITaskDependencyValidator _validator;
    private readonly ILogger<AddTaskDependencyCommandHandler> _logger;

    private const int MaxDependenciesPerTask = 20;

    public AddTaskDependencyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ITaskDependencyValidator validator,
        ILogger<AddTaskDependencyCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<TaskDependencyDto> Handle(AddTaskDependencyCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();

        if (userId == 0 || organizationId == 0)
        {
            throw new UnauthorizedException("User must be authenticated");
        }

        // Validate both tasks exist and belong to the same organization
        var sourceTask = await _unitOfWork.Repository<ProjectTask>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.SourceTaskId && t.OrganizationId == organizationId, cancellationToken);

        if (sourceTask == null)
        {
            throw new NotFoundException($"Source task with ID {request.SourceTaskId} not found");
        }

        var dependentTask = await _unitOfWork.Repository<ProjectTask>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.DependentTaskId && t.OrganizationId == organizationId, cancellationToken);

        if (dependentTask == null)
        {
            throw new NotFoundException($"Dependent task with ID {request.DependentTaskId} not found");
        }

        // Check if dependency already exists
        var existingDependency = await _unitOfWork.Repository<TaskDependency>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                d => d.SourceTaskId == request.SourceTaskId &&
                     d.DependentTaskId == request.DependentTaskId &&
                     d.DependencyType == request.DependencyType &&
                     d.OrganizationId == organizationId,
                cancellationToken);

        if (existingDependency != null)
        {
            throw new ValidationException(
                $"A dependency of type {request.DependencyType} already exists from task {request.SourceTaskId} to task {request.DependentTaskId}");
        }

        // Check for cycles using the validator
        if (await _validator.WouldCreateCycleAsync(request.SourceTaskId, request.DependentTaskId, cancellationToken))
        {
            throw new ValidationException(
                "Adding this dependency would create a cycle in the dependency graph");
        }

        // Validate dependency type
        if (!await _validator.ValidateDependencyTypeAsync(request.SourceTaskId, request.DependentTaskId, request.DependencyType, cancellationToken))
        {
            throw new ValidationException(
                $"Invalid dependency type {request.DependencyType} for the given tasks");
        }

        // Check max dependencies per task (limit on source task)
        var sourceTaskDependencyCount = await _unitOfWork.Repository<TaskDependency>()
            .Query()
            .AsNoTracking()
            .CountAsync(
                d => d.SourceTaskId == request.SourceTaskId && d.OrganizationId == organizationId,
                cancellationToken);

        if (sourceTaskDependencyCount >= MaxDependenciesPerTask)
        {
            throw new ValidationException(
                $"Task {request.SourceTaskId} already has the maximum number of dependencies ({MaxDependenciesPerTask})");
        }

        // Create the dependency
        var dependency = new TaskDependency
        {
            SourceTaskId = request.SourceTaskId,
            DependentTaskId = request.DependentTaskId,
            DependencyType = request.DependencyType,
            OrganizationId = organizationId,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedById = userId
        };

        await _unitOfWork.Repository<TaskDependency>().AddAsync(dependency, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Task dependency {DependencyId} created: Task {SourceTaskId} -> Task {DependentTaskId} (Type: {DependencyType}) by user {UserId}",
            dependency.Id, request.SourceTaskId, request.DependentTaskId, request.DependencyType, userId);

        // Get created by user info
        var createdBy = await _unitOfWork.Repository<User>()
            .GetByIdAsync(userId, cancellationToken);

        if (createdBy == null)
        {
            throw new NotFoundException($"User with ID {userId} not found");
        }

        var createdByName = $"{createdBy.FirstName} {createdBy.LastName}".Trim();
        if (string.IsNullOrEmpty(createdByName))
        {
            createdByName = createdBy.Username;
        }

        // Map to DTO
        return new TaskDependencyDto(
            dependency.Id,
            dependency.SourceTaskId,
            sourceTask.Title,
            dependency.DependentTaskId,
            dependentTask.Title,
            dependency.DependencyType.ToString(),
            dependency.CreatedAt,
            createdByName
        );
    }
}

