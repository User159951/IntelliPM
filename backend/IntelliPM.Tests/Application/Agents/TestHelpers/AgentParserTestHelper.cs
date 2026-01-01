using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Application.Agents.Validators;

namespace IntelliPM.Tests.Application.Agents.TestHelpers;

/// <summary>
/// Helper class for creating IAgentOutputParser instances in tests.
/// </summary>
public static class AgentParserTestHelper
{
    /// <summary>
    /// Creates a real AgentOutputParser instance for testing.
    /// </summary>
    public static IAgentOutputParser CreateRealParser()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        
        // Add validators from the validators assembly
        services.AddValidatorsFromAssemblyContaining<ProductAgentOutputValidator>();
        
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<AgentOutputParser>>();
        
        return new AgentOutputParser(logger, serviceProvider);
    }
}

