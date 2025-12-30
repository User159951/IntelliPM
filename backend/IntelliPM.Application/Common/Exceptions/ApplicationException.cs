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

public class ValidationException : ApplicationException
{
    public ValidationException(string message) : base(message) { }
    public Dictionary<string, string[]> Errors { get; set; } = new();
}

public class ConcurrencyException : ApplicationException
{
    public ConcurrencyException(string message) : base(message) { }
}

