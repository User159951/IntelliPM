using FluentAssertions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using IntelliPM.Infrastructure.BackgroundServices;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Tests.Integration.BackgroundServices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.Text.Json;
using Task = System.Threading.Tasks.Task;
using Xunit;

namespace IntelliPM.Tests.Integration.Infrastructure;

/// <summary>
/// FAST integration tests for OutboxProcessor using InMemoryDatabase.
/// Tests retry logic, idempotency, and Dead Letter Queue.
/// Expected runtime: < 10 seconds
/// </summary>
public class OutboxProcessorFastTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IDomainEventDispatcher> _mockDispatcher;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Mock<ILogger<OutboxProcessor>> _mockLogger;
    private const int MaxRetryAttempts = 3;
    private readonly string _dbName;

    public OutboxProcessorFastTests()
    {
        // Use unique database name per test for isolation
        _dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _mockDispatcher = new Mock<IDomainEventDispatcher>();
        _mockLogger = new Mock<ILogger<OutboxProcessor>>();

        // Setup service scope factory
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_dbName));
        services.AddScoped(_ => _mockDispatcher.Object);
        var serviceProvider = services.BuildServiceProvider();
        _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    }

    private async Task ProcessOutboxMessagesAsync(OutboxProcessor processor)
    {
        // Use reflection to call the private ProcessOutboxMessagesAsync method
        var method = typeof(OutboxProcessor).GetMethod("ProcessOutboxMessagesAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (method == null)
        {
            throw new InvalidOperationException("Could not find ProcessOutboxMessagesAsync method");
        }

        var task = (Task)method.Invoke(processor, new object[] { CancellationToken.None })!;
        await task;
    }

    [Fact]
    public async Task OutboxProcessor_Should_Process_Successful_Messages()
    {
        // Arrange
        var testEvent = new TestDomainEvent("Test Property Value");
        var eventType = typeof(TestDomainEvent).AssemblyQualifiedName!;
        var payload = JsonSerializer.Serialize(testEvent);

        var outboxMessage = OutboxMessage.Create(eventType, payload);
        _context.OutboxMessages.Add(outboxMessage);
        await _context.SaveChangesAsync();

        _mockDispatcher
            .Setup(d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var processor = new OutboxProcessor(_serviceScopeFactory, _mockLogger.Object);
        await ProcessOutboxMessagesAsync(processor);

        // Assert
        var processedMessage = await _context.OutboxMessages.FindAsync(outboxMessage.Id);
        processedMessage.Should().NotBeNull();
        processedMessage!.ProcessedAt.Should().NotBeNull();
        processedMessage.RetryCount.Should().Be(0);
        processedMessage.Error.Should().BeNull();

        _mockDispatcher.Verify(
            d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task OutboxProcessor_Should_Retry_Failed_Messages_And_Move_To_DLQ()
    {
        // Arrange
        var testEvent = new TestDomainEvent("Test Property Value");
        var eventType = typeof(TestDomainEvent).AssemblyQualifiedName!;
        var payload = JsonSerializer.Serialize(testEvent);

        var outboxMessage = OutboxMessage.Create(eventType, payload);
        _context.OutboxMessages.Add(outboxMessage);
        await _context.SaveChangesAsync();

        _mockDispatcher
            .Setup(d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Network error"));

        var processor = new OutboxProcessor(_serviceScopeFactory, _mockLogger.Object);

        // Act - First attempt (RetryCount 0 -> 1)
        await ProcessOutboxMessagesAsync(processor);

        var message1 = await _context.OutboxMessages.FindAsync(outboxMessage.Id);
        message1!.RetryCount.Should().Be(1);
        message1.Error.Should().Contain("Network error");
        message1.NextRetryAt.Should().NotBeNull();

        // Simulate time passing for second retry
        message1 = await _context.OutboxMessages.FindAsync(outboxMessage.Id);
        _context.Entry(message1!).Property("NextRetryAt").CurrentValue = DateTime.UtcNow.AddMinutes(-1);
        await _context.SaveChangesAsync();

        // Second attempt (RetryCount 1 -> 2)
        await ProcessOutboxMessagesAsync(processor);

        var message2 = await _context.OutboxMessages.FindAsync(outboxMessage.Id);
        message2!.RetryCount.Should().Be(2);

        // Simulate time passing for third retry
        message2 = await _context.OutboxMessages.FindAsync(outboxMessage.Id);
        _context.Entry(message2!).Property("NextRetryAt").CurrentValue = DateTime.UtcNow.AddMinutes(-1);
        await _context.SaveChangesAsync();

        // Third attempt (RetryCount 2 -> 3)
        await ProcessOutboxMessagesAsync(processor);

        var message3 = await _context.OutboxMessages.FindAsync(outboxMessage.Id);
        message3!.RetryCount.Should().Be(3);

        // After 3 retries, message should be moved to DLQ
        var dlqMessage = await _context.DeadLetterMessages
            .FirstOrDefaultAsync(d => d.OriginalMessageId == outboxMessage.Id);
        
        dlqMessage.Should().NotBeNull();
        dlqMessage!.TotalRetryAttempts.Should().Be(MaxRetryAttempts);
        dlqMessage.LastError.Should().Contain("Network error");

        // Original message should be removed from Outbox
        var originalMessage = await _context.OutboxMessages.FindAsync(outboxMessage.Id);
        originalMessage.Should().BeNull();

        _mockDispatcher.Verify(
            d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task OutboxProcessor_Should_Check_Idempotency_Before_Processing()
    {
        // Arrange
        var idempotencyKey = "test-idempotency-key-123";
        var testEvent = new TestDomainEvent("Test Property Value");
        var eventType = typeof(TestDomainEvent).AssemblyQualifiedName!;
        var payload = JsonSerializer.Serialize(testEvent);

        // First message - already processed
        var processedMessage = OutboxMessage.Create(eventType, payload, idempotencyKey);
        processedMessage.MarkAsProcessed();
        _context.OutboxMessages.Add(processedMessage);
        await _context.SaveChangesAsync();

        // Second message - duplicate with same idempotency key
        var duplicateMessage = OutboxMessage.Create(eventType, payload, idempotencyKey);
        _context.OutboxMessages.Add(duplicateMessage);
        await _context.SaveChangesAsync();

        var dispatchCallCount = 0;
        _mockDispatcher
            .Setup(d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => dispatchCallCount++)
            .Returns(Task.CompletedTask);

        // Act
        var processor = new OutboxProcessor(_serviceScopeFactory, _mockLogger.Object);
        await ProcessOutboxMessagesAsync(processor);

        // Assert
        var duplicate = await _context.OutboxMessages.FindAsync(duplicateMessage.Id);
        duplicate!.ProcessedAt.Should().NotBeNull(); // Marked as processed due to idempotency check
        dispatchCallCount.Should().Be(0); // No new dispatch calls since duplicate was already processed
    }

    [Fact]
    public async Task OutboxProcessor_Should_Calculate_Exponential_Backoff_Correctly()
    {
        // Arrange
        var testEvent = new TestDomainEvent("Test Property Value");
        var eventType = typeof(TestDomainEvent).AssemblyQualifiedName!;
        var payload = JsonSerializer.Serialize(testEvent);

        var outboxMessage = OutboxMessage.Create(eventType, payload);
        _context.OutboxMessages.Add(outboxMessage);
        await _context.SaveChangesAsync();

        _mockDispatcher
            .Setup(d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error"));

        var processor = new OutboxProcessor(_serviceScopeFactory, _mockLogger.Object);

        // Act - First failure (RetryCount 0 -> 1)
        await ProcessOutboxMessagesAsync(processor);

        var message1 = await _context.OutboxMessages.FindAsync(outboxMessage.Id);
        message1!.NextRetryAt.Should().NotBeNull();
        var expectedBackoff1 = DateTime.UtcNow.AddMinutes(2); // 2^1 = 2 minutes
        message1.NextRetryAt!.Value.Should().BeCloseTo(expectedBackoff1, TimeSpan.FromMinutes(1));

        // Simulate time passing and retry
        message1 = await _context.OutboxMessages.FindAsync(outboxMessage.Id);
        _context.Entry(message1!).Property("NextRetryAt").CurrentValue = DateTime.UtcNow.AddMinutes(-1);
        await _context.SaveChangesAsync();

        // Second failure (RetryCount 1 -> 2)
        await ProcessOutboxMessagesAsync(processor);

        var message2 = await _context.OutboxMessages.FindAsync(outboxMessage.Id);
        message2!.NextRetryAt.Should().NotBeNull();
        var expectedBackoff2 = DateTime.UtcNow.AddMinutes(4); // 2^2 = 4 minutes
        message2.NextRetryAt!.Value.Should().BeCloseTo(expectedBackoff2, TimeSpan.FromMinutes(1));

        // Simulate time passing and retry
        message2 = await _context.OutboxMessages.FindAsync(outboxMessage.Id);
        _context.Entry(message2!).Property("NextRetryAt").CurrentValue = DateTime.UtcNow.AddMinutes(-1);
        await _context.SaveChangesAsync();

        // Third failure (RetryCount 2 -> 3)
        await ProcessOutboxMessagesAsync(processor);

        var message3 = await _context.OutboxMessages.FindAsync(outboxMessage.Id);
        message3!.NextRetryAt.Should().NotBeNull();
        var expectedBackoff3 = DateTime.UtcNow.AddMinutes(8); // 2^3 = 8 minutes
        message3.NextRetryAt!.Value.Should().BeCloseTo(expectedBackoff3, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task OutboxProcessor_Should_Not_Process_Messages_With_NextRetryAt_In_Future()
    {
        // Arrange
        var testEvent = new TestDomainEvent("Test Property Value");
        var eventType = typeof(TestDomainEvent).AssemblyQualifiedName!;
        var payload = JsonSerializer.Serialize(testEvent);

        var outboxMessage = OutboxMessage.Create(eventType, payload);
        outboxMessage.RecordFailure("Test error");
        // Set NextRetryAt to future
        _context.Entry(outboxMessage).Property("NextRetryAt").CurrentValue = DateTime.UtcNow.AddHours(1);
        _context.OutboxMessages.Add(outboxMessage);
        await _context.SaveChangesAsync();

        _mockDispatcher
            .Setup(d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var processor = new OutboxProcessor(_serviceScopeFactory, _mockLogger.Object);
        await ProcessOutboxMessagesAsync(processor);

        // Assert
        var message = await _context.OutboxMessages.FindAsync(outboxMessage.Id);
        message!.ProcessedAt.Should().BeNull(); // Not processed yet
        _mockDispatcher.Verify(
            d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OutboxProcessor_Should_Process_Messages_In_CreatedAt_Order()
    {
        // Arrange
        var messages = new List<OutboxMessage>();
        for (int i = 0; i < 5; i++)
        {
            var testEvent = new TestDomainEvent($"Test Property Value {i}");
            var eventType = typeof(TestDomainEvent).AssemblyQualifiedName!;
            var payload = JsonSerializer.Serialize(testEvent);
            var message = OutboxMessage.Create(eventType, payload);
            
            // Manually set CreatedAt to ensure ordering
            var createdAt = DateTime.UtcNow.AddMinutes(-5 + i);
            _context.Entry(message).Property("CreatedAt").CurrentValue = createdAt;
            
            messages.Add(message);
            _context.OutboxMessages.Add(message);
        }

        await _context.SaveChangesAsync();

        var processedOrder = new List<int>();
        _mockDispatcher
            .Setup(d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns<IDomainEvent, CancellationToken>((evt, ct) =>
            {
                if (evt is TestDomainEvent testEvt)
                {
                    var index = int.Parse(testEvt.TestProperty.Split(' ').Last());
                    processedOrder.Add(index);
                }
                return Task.CompletedTask;
            });

        // Act
        var processor = new OutboxProcessor(_serviceScopeFactory, _mockLogger.Object);
        await ProcessOutboxMessagesAsync(processor);

        // Assert
        processedOrder.Should().HaveCount(5);
        processedOrder.Should().BeInAscendingOrder();

        var allMessages = await _context.OutboxMessages.ToListAsync();
        allMessages.Should().OnlyContain(m => m.ProcessedAt != null);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}

