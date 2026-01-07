namespace IntelliPM.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a release deployment is attempted but blocking quality gates have not passed.
/// </summary>
public class QualityGateNotPassedException : ValidationException
{
    public QualityGateNotPassedException(string message) : base(message)
    {
    }

    public QualityGateNotPassedException(string message, IEnumerable<string> failedGateNames) 
        : base(message)
    {
        FailedGateNames = failedGateNames.ToList();
    }

    /// <summary>
    /// List of quality gate names that failed and are blocking deployment.
    /// </summary>
    public List<string> FailedGateNames { get; } = new();
}

