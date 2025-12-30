using Microsoft.Extensions.Hosting;

namespace IntelliPM.Infrastructure.Services;

/// <summary>
/// Service for processing email templates with variable substitution
/// </summary>
public class EmailTemplateService
{
    private readonly string _templatesPath;

    public EmailTemplateService(IHostEnvironment environment)
    {
        // Get the templates directory path - check multiple locations
        var basePath = AppContext.BaseDirectory;
        _templatesPath = Path.Combine(basePath, "EmailTemplates");
        
        // For development, check if templates exist in project directory
        if (environment.IsDevelopment() && !Directory.Exists(_templatesPath))
        {
            var projectPath = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", ".."));
            var devTemplatesPath = Path.Combine(projectPath, "IntelliPM.Infrastructure", "EmailTemplates");
            if (Directory.Exists(devTemplatesPath))
            {
                _templatesPath = devTemplatesPath;
            }
        }
    }

    /// <summary>
    /// Processes an email template by replacing variables with actual values
    /// </summary>
    /// <param name="templateName">Name of the template file (without extension)</param>
    /// <param name="variables">Dictionary of variable names and their values</param>
    /// <returns>Processed HTML content</returns>
    public string ProcessTemplate(string templateName, Dictionary<string, string> variables)
    {
        var templatePath = Path.Combine(_templatesPath, $"{templateName}.html");
        
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Email template not found: {templatePath}");
        }

        var template = File.ReadAllText(templatePath);
        
        foreach (var variable in variables)
        {
            // Replace {{VariableName}} with actual value
            template = template.Replace($"{{{{{variable.Key}}}}}", variable.Value ?? string.Empty);
        }

        return template;
    }

    /// <summary>
    /// Checks if a template exists
    /// </summary>
    public bool TemplateExists(string templateName)
    {
        var templatePath = Path.Combine(_templatesPath, $"{templateName}.html");
        return File.Exists(templatePath);
    }
}

