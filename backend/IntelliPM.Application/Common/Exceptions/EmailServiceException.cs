namespace IntelliPM.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when email service operations fail.
/// Used to provide user-friendly error messages for SMTP failures.
/// </summary>
public class EmailServiceException : Exception
{
    public EmailServiceException(string message) : base(message) { }
    
    public EmailServiceException(string message, Exception innerException) 
        : base(message, innerException) { }
}

