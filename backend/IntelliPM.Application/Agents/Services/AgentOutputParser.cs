using System.Text.Json;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using IntelliPM.Application.Agents.DTOs;
using IntelliPM.Application.Agents.Validators;

namespace IntelliPM.Application.Agents.Services;

/// <summary>
/// Service for parsing JSON output from LLM agents with validation.
/// Handles JSON extraction from text, parsing, and FluentValidation.
/// </summary>
public class AgentOutputParser : IAgentOutputParser
{
    private readonly ILogger<AgentOutputParser> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IServiceProvider _serviceProvider;

    public AgentOutputParser(ILogger<AgentOutputParser> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
    }

    /// <summary>
    /// Attempts to parse JSON string into the specified type with validation.
    /// </summary>
    public bool TryParse<T>(string json, out T? result, out List<string> errors)
    {
        result = default(T);
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(json))
        {
            errors.Add("JSON input is null or empty");
            return false;
        }

        try
        {
            // Extract JSON from text if wrapped in markdown or other text
            var jsonString = ExtractJson(json);
            
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                errors.Add("No valid JSON found in input");
                _logger.LogWarning("Failed to extract JSON from LLM output. Raw output: {RawOutput}", json);
                return false;
            }

            // Parse JSON
            try
            {
                result = JsonSerializer.Deserialize<T>(jsonString, _jsonOptions);
            }
            catch (JsonException ex)
            {
                errors.Add($"JSON parsing error: {ex.Message}");
                _logger.LogWarning(ex, "JSON parsing failed. JSON string: {JsonString}", jsonString);
                return false;
            }

            if (result == null)
            {
                errors.Add("Deserialized result is null");
                return false;
            }

            // Validate using FluentValidation
            var validator = GetValidator<T>();
            if (validator != null)
            {
                var validationResult = validator.Validate(result);
                if (!validationResult.IsValid)
                {
                    errors.AddRange(validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                    _logger.LogWarning("Validation failed for {TypeName}. Errors: {Errors}. JSON: {Json}", 
                        typeof(T).Name, string.Join("; ", errors), jsonString);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            errors.Add($"Unexpected error during parsing: {ex.Message}");
            _logger.LogError(ex, "Unexpected error parsing JSON for type {TypeName}. JSON: {Json}", 
                typeof(T).Name, json);
            return false;
        }
    }

    /// <summary>
    /// Extracts JSON from text that may contain markdown code blocks or other text.
    /// </summary>
    private string ExtractJson(string input)
    {
        // Try to find JSON in markdown code block (```json ... ```)
        var markdownJsonMatch = Regex.Match(input, @"```(?:json)?\s*(\{.*?\})\s*```", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (markdownJsonMatch.Success)
        {
            return markdownJsonMatch.Groups[1].Value;
        }

        // Try to find JSON object directly
        var jsonObjectMatch = Regex.Match(input, @"\{.*\}", RegexOptions.Singleline);
        if (jsonObjectMatch.Success)
        {
            return jsonObjectMatch.Value;
        }

        // If no JSON found, return the original string (let deserializer handle it)
        return input.Trim();
    }

    /// <summary>
    /// Gets the FluentValidation validator for the specified type.
    /// </summary>
    private IValidator<T>? GetValidator<T>()
    {
        // Map types to their validators
        var type = typeof(T);
        
        if (type == typeof(ProductAgentOutputDto))
        {
            return _serviceProvider.GetService<IValidator<ProductAgentOutputDto>>() as IValidator<T>;
        }
        else if (type == typeof(DeliveryAgentOutputDto))
        {
            return _serviceProvider.GetService<IValidator<DeliveryAgentOutputDto>>() as IValidator<T>;
        }
        else if (type == typeof(ManagerAgentOutputDto))
        {
            return _serviceProvider.GetService<IValidator<ManagerAgentOutputDto>>() as IValidator<T>;
        }
        else if (type == typeof(QAAgentOutputDto))
        {
            return _serviceProvider.GetService<IValidator<QAAgentOutputDto>>() as IValidator<T>;
        }
        else if (type == typeof(BusinessAgentOutputDto))
        {
            return _serviceProvider.GetService<IValidator<BusinessAgentOutputDto>>() as IValidator<T>;
        }

        return null;
    }
}

