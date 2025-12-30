namespace IntelliPM.Application.Common;

/// <summary>
/// Represents the result of a validation operation.
/// Contains a boolean indicating validity and a list of error messages.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Indicates whether the validation passed.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// List of validation error messages.
    /// Empty if validation passed.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with the specified error messages.
    /// </summary>
    /// <param name="errors">Error messages describing validation failures.</param>
    public static ValidationResult Failure(params string[] errors) => new() 
    { 
        IsValid = false, 
        Errors = errors.ToList() 
    };
}

