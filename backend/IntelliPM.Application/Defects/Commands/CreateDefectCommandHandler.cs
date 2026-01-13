using MediatR;
using Microsoft.EntityFrameworkCore;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Domain.Entities;

namespace IntelliPM.Application.Defects.Commands;

public class CreateDefectCommandHandler : IRequestHandler<CreateDefectCommand, CreateDefectResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public CreateDefectCommandHandler(IUnitOfWork unitOfWork, IMediator mediator, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public async Task<CreateDefectResponse> Handle(CreateDefectCommand request, CancellationToken cancellationToken)
    {
        // Permission check - anyone who is a member can create defects
        var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(request.ProjectId, request.ReportedById), cancellationToken);
        if (userRole == null)
            throw new UnauthorizedException("You are not a member of this project");
        // All members can create defects (no specific permission check needed)

        // Verify assignee exists and belongs to the same organization if provided
        if (request.AssignedToId.HasValue)
        {
            var organizationId = _currentUserService.GetOrganizationId();
            var userRepo = _unitOfWork.Repository<User>();
            var assignee = await userRepo.Query()
                .FirstOrDefaultAsync(u => u.Id == request.AssignedToId.Value, cancellationToken);
            
            if (assignee == null)
                throw new ValidationException($"User with ID {request.AssignedToId.Value} not found");
            
            if (assignee.OrganizationId != organizationId)
                throw new ValidationException($"User with ID {request.AssignedToId.Value} does not belong to your organization");
        }

        var defect = new Defect
        {
            ProjectId = request.ProjectId,
            UserStoryId = request.UserStoryId,
            SprintId = request.SprintId,
            Title = request.Title,
            Description = request.Description,
            Severity = request.Severity,
            ReportedById = request.ReportedById,
            AssignedToId = request.AssignedToId,
            FoundInEnvironment = request.FoundInEnvironment,
            StepsToReproduce = request.StepsToReproduce,
            ReportedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var repo = _unitOfWork.Repository<Defect>();
        await repo.AddAsync(defect, cancellationToken);

        // Create alert for high/critical defects
        if (defect.Severity is "High" or "Critical")
        {
            var alertRepo = _unitOfWork.Repository<Alert>();
            await alertRepo.AddAsync(new Alert
            {
                ProjectId = defect.ProjectId,
                Type = "CriticalDefect",
                Severity = defect.Severity == "Critical" ? "Critical" : "Error",
                Title = "High severity defect reported",
                Message = $"Defect '{defect.Title}' reported with severity {defect.Severity}.",
                CreatedAt = DateTimeOffset.UtcNow
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateDefectResponse(defect.Id, defect.Title, defect.Severity, defect.Status);
    }
}

