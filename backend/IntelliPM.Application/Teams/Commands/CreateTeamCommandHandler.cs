using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;

namespace IntelliPM.Application.Teams.Commands;

public class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, CreateTeamResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateTeamCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateTeamResponse> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = new Team
        {
            Name = request.Name,
            Capacity = request.Capacity,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var teamRepo = _unitOfWork.Repository<Team>();
        await teamRepo.AddAsync(team, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateTeamResponse(
            team.Id,
            team.Name,
            team.Capacity,
            team.CreatedAt
        );
    }
}

