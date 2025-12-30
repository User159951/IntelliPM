using MediatR;

namespace IntelliPM.Application.Queries.Metrics;

public class GetDefectsBySeverityQuery : IRequest<DefectsBySeverityResponse>
{
    public int? ProjectId { get; set; }
}

public class DefectsBySeverityResponse
{
    public List<DefectSeverityData> Defects { get; set; } = new();
}

public class DefectSeverityData
{
    public string Severity { get; set; } = string.Empty;
    public int Count { get; set; }
}
