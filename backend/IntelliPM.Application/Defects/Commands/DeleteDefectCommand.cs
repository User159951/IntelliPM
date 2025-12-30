using MediatR;

namespace IntelliPM.Application.Defects.Commands;

public record DeleteDefectCommand(int DefectId, int DeletedBy) : IRequest;
