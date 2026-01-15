using System.Text.RegularExpressions;

namespace IntelliPM.Infrastructure.Utilities;

/// <summary>
/// Utility class for redacting Personally Identifiable Information (PII) from logs and data.
/// Prevents accidental logging of sensitive data like passwords, tokens, credit cards, and email addresses.
/// </summary>
public static class PiiRedactor
{
    private const string RedactedPassword = "[REDACTED_PASSWORD]";
    private const string RedactedToken = "[REDACTED_TOKEN]";
    private const string RedactedEmail = "[REDACTED_EMAIL]";
    private const string RedactedCreditCard = "[REDACTED_CC]";
    private const string RedactedSsn = "[REDACTED_SSN]";
    private const string RedactedValue = "[REDACTED]";

    // Patterns for detecting sensitive data
    private static readonly Regex EmailPattern = new(
        @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex CreditCardPattern = new(
        @"\b(?:\d{4}[-\s]?){3}\d{4}\b",
        RegexOptions.Compiled);

    private static readonly Regex SsnPattern = new(
        @"\b\d{3}-\d{2}-\d{4}\b",
        RegexOptions.Compiled);

    // Keywords that indicate sensitive fields
    private static readonly HashSet<string> SensitiveKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "passwd", "pwd", "secret", "token", "access_token", "refresh_token",
        "api_key", "apikey", "auth_token", "authorization", "bearer", "credential",
        "credit_card", "creditcard", "card_number", "cardnumber", "cvv", "cvc",
        "ssn", "social_security", "socialsecurity", "ssn_number",
        "account_number", "accountnumber", "routing_number", "routingnumber"
    };

    /// <summary>
    /// Redacts sensitive information from a string value.
    /// </summary>
    /// <param name="value">The value to redact</param>
    /// <param name="fieldName">Optional field name to check against sensitive keywords</param>
    /// <returns>Redacted value if sensitive data detected, otherwise original value</returns>
    public static string Redact(string? value, string? fieldName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value ?? string.Empty;
        }

        // Check if field name indicates sensitive data
        if (!string.IsNullOrWhiteSpace(fieldName) && IsSensitiveField(fieldName))
        {
            return RedactedValue;
        }

        // Check for email addresses
        if (EmailPattern.IsMatch(value))
        {
            return EmailPattern.Replace(value, RedactedEmail);
        }

        // Check for credit card numbers
        if (CreditCardPattern.IsMatch(value))
        {
            return CreditCardPattern.Replace(value, RedactedCreditCard);
        }

        // Check for SSN
        if (SsnPattern.IsMatch(value))
        {
            return SsnPattern.Replace(value, RedactedSsn);
        }

        // Check if value looks like a token (long alphanumeric string)
        if (IsTokenLike(value))
        {
            return RedactedToken;
        }

        return value;
    }

    /// <summary>
    /// Redacts sensitive information from an object by creating a sanitized copy.
    /// Useful for logging DTOs or request/response objects.
    /// </summary>
    /// <param name="obj">The object to sanitize</param>
    /// <returns>A dictionary representation with sensitive fields redacted</returns>
    public static Dictionary<string, object?> SanitizeObject(object? obj)
    {
        if (obj == null)
        {
            return new Dictionary<string, object?>();
        }

        var result = new Dictionary<string, object?>();
        var properties = obj.GetType().GetProperties();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(obj);
            var propName = prop.Name;

            // Check for [Sensitive] attribute
            var isSensitive = prop.GetCustomAttributes(typeof(SensitiveAttribute), inherit: true).Any();

            if (isSensitive || IsSensitiveField(propName))
            {
                result[propName] = RedactedValue;
            }
            else if (value is string strValue)
            {
                result[propName] = Redact(strValue, propName);
            }
            else if (value != null && value.GetType().IsClass && value.GetType() != typeof(string))
            {
                // Recursively sanitize nested objects
                result[propName] = SanitizeObject(value);
            }
            else
            {
                result[propName] = value;
            }
        }

        return result;
    }

    /// <summary>
    /// Redacts sensitive information from a dictionary of key-value pairs.
    /// </summary>
    public static Dictionary<string, object?> SanitizeDictionary(IDictionary<string, object?>? dictionary)
    {
        if (dictionary == null)
        {
            return new Dictionary<string, object?>();
        }

        var result = new Dictionary<string, object?>();

        foreach (var kvp in dictionary)
        {
            if (IsSensitiveField(kvp.Key))
            {
                result[kvp.Key] = RedactedValue;
            }
            else if (kvp.Value is string strValue)
            {
                result[kvp.Key] = Redact(strValue, kvp.Key);
            }
            else if (kvp.Value != null && kvp.Value.GetType().IsClass && kvp.Value.GetType() != typeof(string))
            {
                result[kvp.Key] = SanitizeObject(kvp.Value);
            }
            else
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if a field name indicates sensitive data.
    /// </summary>
    public static bool IsSensitiveField(string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return false;
        }

        return SensitiveKeywords.Any(keyword => 
            fieldName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a value looks like a token (long alphanumeric string).
    /// </summary>
    private static bool IsTokenLike(string value)
    {
        // Tokens are typically long alphanumeric strings (20+ characters)
        if (value.Length < 20)
        {
            return false;
        }

        // Check if it's mostly alphanumeric with possible dashes/underscores
        var alphanumericCount = value.Count(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.');
        return alphanumericCount >= value.Length * 0.9; // 90% alphanumeric
    }
}

/// <summary>
/// Attribute to mark properties or fields as containing sensitive data.
/// Properties marked with this attribute will be automatically redacted when using SanitizeObject.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SensitiveAttribute : Attribute
{
}
