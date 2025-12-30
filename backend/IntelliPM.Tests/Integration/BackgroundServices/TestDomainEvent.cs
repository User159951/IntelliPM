using IntelliPM.Domain.Events;
using MediatR;

namespace IntelliPM.Tests.Integration.BackgroundServices;

/// <summary>
/// Test domain event for OutboxProcessor integration tests.
/// </summary>
public class TestDomainEvent : IDomainEvent, INotification
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public string TestProperty { get; }

    public TestDomainEvent(string testProperty)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        TestProperty = testProperty;
    }
}

