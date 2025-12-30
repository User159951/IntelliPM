using Microsoft.Extensions.DependencyInjection;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Application.Common.Behaviors;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Services;
using IntelliPM.Application.Services;
using IntelliPM.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace IntelliPM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Behaviors (Order matters!)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Agents
        services.AddScoped<ProductAgent>();
        services.AddScoped<DeliveryAgent>();
        services.AddScoped<ManagerAgent>();
        services.AddScoped<QAAgent>();
        services.AddScoped<BusinessAgent>();

        // Services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IMentionParser, MentionParser>();
        services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IAIAvailabilityService, AIAvailabilityService>();
        services.AddScoped<ITaskDependencyValidator, TaskDependencyValidator>();
        services.AddScoped<IMilestoneValidator, MilestoneValidator>();
        services.AddScoped<IReleaseNotesGenerator, ReleaseNotesGenerator>();
        services.AddScoped<IQualityGateChecker, QualityGateChecker>();

        return services;
    }
}

