namespace IntelliPM.Application.Common.Exceptions;

public class ApplicationException : Exception
{
    public ApplicationException(string message) : base(message) { }
}

public class NotFoundException : ApplicationException
{
    public NotFoundException(string message) : base(message) { }
}

public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message) : base(message) { }
}

public class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message) : base(message) { }
    
    public ForbiddenException(string message, string? permission, int? organizationId = null) : base(message)
    {
        Permission = permission;
        OrganizationId = organizationId;
    }
    
    public string? Permission { get; }
    public int? OrganizationId { get; }
}

public class ValidationException : ApplicationException
{
    public ValidationException(string message) : base(message) { }
    public Dictionary<string, string[]> Errors { get; set; } = new();
}

public class ConcurrencyException : ApplicationException
{
    public ConcurrencyException(string message) : base(message) { }
}

public class FeatureFlagNotFoundException : NotFoundException
{
    public FeatureFlagNotFoundException(string featureName, int? organizationId = null)
        : base(organizationId.HasValue
            ? $"Feature flag '{featureName}' not found for organization ID {organizationId.Value} (checked organization-specific and global flags)."
            : $"Feature flag '{featureName}' not found (checked global flags).")
    {
        FeatureName = featureName;
        OrganizationId = organizationId;
    }

    public string FeatureName { get; }
    public int? OrganizationId { get; }
}

