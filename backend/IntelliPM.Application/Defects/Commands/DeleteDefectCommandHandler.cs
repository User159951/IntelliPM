using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Domain.Entities;
using System.Threading.Tasks;

namespace IntelliPM.Application.Defects.Commands;

public class DeleteDefectCommandHandler : IRequestHandler<DeleteDefectCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public DeleteDefectCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async System.Threading.Tasks.Task Handle(DeleteDefectCommand request, CancellationToken cancellationToken)
    {
        var defectRepo = _unitOfWork.Repository<Defect>();
        var defect = await defectRepo.GetByIdAsync(request.DefectId, cancellationToken);

        if (defect == null)
            throw new InvalidOperationException($"Defect with ID {request.DefectId} not found");

        // Permission check - only ProductOwner or ScrumMaster can delete defects
        var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(defect.ProjectId, request.DeletedBy), cancellationToken);
        if (userRole == null)
            throw new UnauthorizedException("You are not a member of this project");
        if (!ProjectPermissions.CanDeleteTasks(userRole.Value)) // Using CanDeleteTasks as similar permission
            throw new UnauthorizedException("You don't have permission to delete defects in this project");

        defectRepo.Delete(defect);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
