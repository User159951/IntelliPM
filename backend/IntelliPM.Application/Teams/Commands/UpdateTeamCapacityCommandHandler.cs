using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;

namespace IntelliPM.Application.Teams.Commands;

public class UpdateTeamCapacityCommandHandler : IRequestHandler<UpdateTeamCapacityCommand, UpdateTeamCapacityResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTeamCapacityCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateTeamCapacityResponse> Handle(UpdateTeamCapacityCommand request, CancellationToken cancellationToken)
    {
        var teamRepo = _unitOfWork.Repository<Team>();
        var team = await teamRepo.GetByIdAsync(request.TeamId, cancellationToken);
        
        if (team == null)
            throw new InvalidOperationException($"Team with ID {request.TeamId} not found");

        team.Capacity = request.NewCapacity;
        team.UpdatedAt = DateTimeOffset.UtcNow;

        teamRepo.Update(team);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateTeamCapacityResponse(
            team.Id,
            team.Capacity,
            team.UpdatedAt
        );
    }
}

