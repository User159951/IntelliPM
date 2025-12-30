using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Domain.Entities;

namespace IntelliPM.Application.Defects.Commands;

public class UpdateDefectCommandHandler : IRequestHandler<UpdateDefectCommand, UpdateDefectResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public UpdateDefectCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<UpdateDefectResponse> Handle(UpdateDefectCommand request, CancellationToken cancellationToken)
    {
        var defectRepo = _unitOfWork.Repository<Defect>();
        var defect = await defectRepo.GetByIdAsync(request.DefectId, cancellationToken);

        if (defect == null)
            throw new InvalidOperationException($"Defect with ID {request.DefectId} not found");

        // Permission check - anyone who is a member can update defects
        var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(defect.ProjectId, request.UpdatedBy), cancellationToken);
        if (userRole == null)
            throw new UnauthorizedException("You are not a member of this project");
        // All members can update defects (no specific permission check needed)

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.Title))
            defect.Title = request.Title;

        if (!string.IsNullOrEmpty(request.Description))
            defect.Description = request.Description;

        if (!string.IsNullOrEmpty(request.Severity))
            defect.Severity = request.Severity;

        if (!string.IsNullOrEmpty(request.Status))
        {
            var oldStatus = defect.Status;
            defect.Status = request.Status;

            // Set ResolvedAt when status changes to Resolved or Closed
            if ((request.Status == "Resolved" || request.Status == "Closed") && !defect.ResolvedAt.HasValue)
            {
                defect.ResolvedAt = DateTimeOffset.UtcNow;
            }
            // Clear ResolvedAt if reopened
            else if (request.Status == "Open" || request.Status == "InProgress")
            {
                defect.ResolvedAt = null;
            }
        }

        if (request.AssignedToId.HasValue)
        {
            // Verify assignee exists
            var userRepo = _unitOfWork.Repository<User>();
            var assignee = await userRepo.GetByIdAsync(request.AssignedToId.Value, cancellationToken);
            if (assignee == null)
                throw new InvalidOperationException($"User with ID {request.AssignedToId.Value} not found");
            
            defect.AssignedToId = request.AssignedToId;
        }

        if (request.FoundInEnvironment != null)
            defect.FoundInEnvironment = request.FoundInEnvironment;

        if (request.StepsToReproduce != null)
            defect.StepsToReproduce = request.StepsToReproduce;

        if (request.Resolution != null)
            defect.Resolution = request.Resolution;

        defect.UpdatedAt = DateTimeOffset.UtcNow;

        defectRepo.Update(defect);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateDefectResponse(
            defect.Id,
            defect.Title,
            defect.Severity,
            defect.Status,
            defect.UpdatedAt
        );
    }
}
