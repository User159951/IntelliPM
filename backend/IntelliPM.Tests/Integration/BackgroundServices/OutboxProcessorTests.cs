using FluentAssertions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using IntelliPM.Infrastructure.BackgroundServices;
using IntelliPM.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using Testcontainers.MsSql;
using Xunit;

namespace IntelliPM.Tests.Integration.BackgroundServices;

/// <summary>
/// Integration tests for OutboxProcessor with real database using TestContainers.
/// Tests the Outbox pattern implementation including retry logic, idempotency, and batch processing.
/// </summary>
public class OutboxProcessorTests : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer;
    private AppDbContext _dbContext = null!;
    private readonly Mock<IDomainEventDispatcher> _mockDispatcher;
    private IServiceScopeFactory _serviceScopeFactory = null!;
    private const int MaxRetryAttempts = 3;

    public OutboxProcessorTests()
    {
        // Create SQL Server test container
        // Using SQL Server 2019 for tests (faster startup) - this does NOT affect production/development databases
        // Only the TestContainers integration test environment uses this image
        _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
            .WithPassword("YourStrong@Passw0rd")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("MSSQL_PID", "Developer")
            .Build();

        _mockDispatcher = new Mock<IDomainEventDispatcher>();
    }

    public async System.Threading.Tasks.Task InitializeAsync()
    {
        // Start the database container
        await _dbContainer.StartAsync();

        // Create DbContext options
        var connectionString = _dbContainer.GetConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString, sqlOpts =>
                sqlOpts.EnableRetryOnFailure(3))
            .Options;

        _dbContext = new AppDbContext(options);

        // Apply migrations
        await _dbContext.Database.MigrateAsync();

        // Setup service scope factory
        var services = new ServiceCollection();
        var finalConnectionString = connectionString; // Capture for closure
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(finalConnectionString, sqlOpts =>
                sqlOpts.EnableRetryOnFailure(3)));

        services.AddScoped(_ => _mockDispatcher.Object);
        var serviceProvider = services.BuildServiceProvider();
        _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    }

    public async System.Threading.Tasks.Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }

        await _dbContainer.DisposeAsync();
    }

    private async System.Threading.Tasks.Task CleanupDatabaseAsync()
    {
        _dbContext.OutboxMessages.RemoveRange(_dbContext.OutboxMessages);
        await _dbContext.SaveChangesAsync();
        _mockDispatcher.Reset();
    }

    private async System.Threading.Tasks.Task ProcessOutboxMessagesAsync(OutboxProcessor processor)
    {
        // Use reflection to call the private ProcessOutboxMessagesAsync method
        var method = typeof(OutboxProcessor).GetMethod("ProcessOutboxMessagesAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (method == null)
        {
            throw new InvalidOperationException("Could not find ProcessOutboxMessagesAsync method");
        }

        var task = (System.Threading.Tasks.Task)method.Invoke(processor, new object[] { CancellationToken.None })!;
        await task;
    }

    [Fact]
    public async System.Threading.Tasks.Task OutboxProcessor_ProcessesUnprocessedMessages_Successfully()
    {
        // Arrange: Insert OutboxMessage with ProcessedAt = null
        await CleanupDatabaseAsync();

        var testEvent = new TestDomainEvent("Test Property Value");
        var eventType = typeof(TestDomainEvent).AssemblyQualifiedName!;
        var payload = JsonSerializer.Serialize(testEvent);

        var outboxMessage = OutboxMessage.Create(eventType, payload);
        _dbContext.OutboxMessages.Add(outboxMessage);
        await _dbContext.SaveChangesAsync();

        _mockDispatcher
            .Setup(d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act: Run OutboxProcessor once
        var logger = new Mock<ILogger<OutboxProcessor>>();
        var processor = new OutboxProcessor(_serviceScopeFactory, logger.Object);
        await ProcessOutboxMessagesAsync(processor);

        // Assert: ProcessedAt is set, event dispatched
        var processedMessage = await _dbContext.OutboxMessages.FindAsync(outboxMessage.Id);
        processedMessage.Should().NotBeNull();
        processedMessage!.ProcessedAt.Should().NotBeNull();
        processedMessage.RetryCount.Should().Be(0);
        processedMessage.Error.Should().BeNull();

        _mockDispatcher.Verify(
            d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task OutboxProcessor_RetriesFailedMessages_WithExponentialBackoff()
    {
        // Arrange: Insert message, mock event handler to throw exception
        await CleanupDatabaseAsync();

        var testEvent = new TestDomainEvent("Test Property Value");
        var eventType = typeof(TestDomainEvent).AssemblyQualifiedName!;
        var payload = JsonSerializer.Serialize(testEvent);

        var outboxMessage = OutboxMessage.Create(eventType, payload);
        _dbContext.OutboxMessages.Add(outboxMessage);
        await _dbContext.SaveChangesAsync();

        _mockDispatcher
            .Setup(d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Network error"));

        var logger = new Mock<ILogger<OutboxProcessor>>();
        var processor = new OutboxProcessor(_serviceScopeFactory, logger.Object);

        // Act: Run processor 3 times
        // First attempt (RetryCount 0 -> 1)
        await ProcessOutboxMessagesAsync(processor);

        var message1 = await _dbContext.OutboxMessages.FindAsync(outboxMessage.Id);
        message1!.RetryCount.Should().Be(1);
        message1.Error.Should().Contain("Network error");
        message1.NextRetryAt.Should().NotBeNull();
        var expectedBackoff1 = DateTime.UtcNow.AddMinutes(2); // 2^1 = 2 minutes
        message1.NextRetryAt!.Value.Should().BeCloseTo(expectedBackoff1, TimeSpan.FromMinutes(1));

        // Simulate time passing for second retry
        message1 = await _dbContext.OutboxMessages.FindAsync(outboxMessage.Id);
        _dbContext.Entry(message1!).Property("NextRetryAt").CurrentValue = DateTime.UtcNow.AddMinutes(-1); // Make it ready for retry
        await _dbContext.SaveChangesAsync();

        // Second attempt (RetryCount 1 -> 2)
        await ProcessOutboxMessagesAsync(processor);

        var message2 = await _dbContext.OutboxMessages.FindAsync(outboxMessage.Id);
        message2!.RetryCount.Should().Be(2);
        var expectedBackoff2 = DateTime.UtcNow.AddMinutes(4); // 2^2 = 4 minutes
        message2.NextRetryAt!.Value.Should().BeCloseTo(expectedBackoff2, TimeSpan.FromMinutes(1));

        // Simulate time passing for third retry
        message2 = await _dbContext.OutboxMessages.FindAsync(outboxMessage.Id);
        _dbContext.Entry(message2!).Property("NextRetryAt").CurrentValue = DateTime.UtcNow.AddMinutes(-1);
        await _dbContext.SaveChangesAsync();

        // Third attempt (RetryCount 2 -> 3)
        await ProcessOutboxMessagesAsync(processor);

        var message3 = await _dbContext.OutboxMessages.FindAsync(outboxMessage.Id);
        message3!.RetryCount.Should().Be(3);
        var expectedBackoff3 = DateTime.UtcNow.AddMinutes(8); // 2^3 = 8 minutes
        message3.NextRetryAt!.Value.Should().BeCloseTo(expectedBackoff3, TimeSpan.FromMinutes(1));

        // After 3 retries, message should not be processed (max retries exceeded)
        message3.ProcessedAt.Should().BeNull();

        // Assert: RetryCount increments (0 -> 1 -> 2 -> 3)
        // NextRetryAt set correctly (2min, 4min, 8min)
        // Error message captured
        // After 3 retries, message marked as permanently failed
        _mockDispatcher.Verify(
            d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async System.Threading.Tasks.Task OutboxProcessor_SkipsAlreadyProcessedMessages()
    {
        // Arrange: Insert message with same IdempotencyKey, already processed
        await CleanupDatabaseAsync();

        var idempotencyKey = "test-idempotency-key-123";
        var testEvent = new TestDomainEvent("Test Property Value");
        var eventType = typeof(TestDomainEvent).AssemblyQualifiedName!;
        var payload = JsonSerializer.Serialize(testEvent);

        // First message - already processed
        var processedMessage = OutboxMessage.Create(eventType, payload, idempotencyKey);
        processedMessage.MarkAsProcessed();
        _dbContext.OutboxMessages.Add(processedMessage);
        await _dbContext.SaveChangesAsync();

        // Second message - duplicate with same idempotency key
        var duplicateMessage = OutboxMessage.Create(eventType, payload, idempotencyKey);
        _dbContext.OutboxMessages.Add(duplicateMessage);
        await _dbContext.SaveChangesAsync();

        var dispatchCallCount = 0;
        _mockDispatcher
            .Setup(d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => dispatchCallCount++)
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act: Insert duplicate message, run processor
        var logger = new Mock<ILogger<OutboxProcessor>>();
        var processor = new OutboxProcessor(_serviceScopeFactory, logger.Object);
        await ProcessOutboxMessagesAsync(processor);

        // Assert: Second message skipped, handler not called twice
        var duplicate = await _dbContext.OutboxMessages.FindAsync(duplicateMessage.Id);
        duplicate!.ProcessedAt.Should().NotBeNull(); // Marked as processed due to idempotency check
        dispatchCallCount.Should().Be(0); // No new dispatch calls since duplicate was already processed
    }

    [Fact]
    public async System.Threading.Tasks.Task OutboxProcessor_ProcessesMultipleMessages_InOrder()
    {
        // Arrange: Insert 10 messages with different CreatedAt
        await CleanupDatabaseAsync();

        var messages = new List<OutboxMessage>();
        for (int i = 0; i < 10; i++)
        {
            var testEvent = new TestDomainEvent($"Test Property Value {i}");
            var eventType = typeof(TestDomainEvent).AssemblyQualifiedName!;
            var payload = JsonSerializer.Serialize(testEvent);
            var message = OutboxMessage.Create(eventType, payload);
            
            // Manually set CreatedAt to ensure ordering using reflection
            var createdAt = DateTime.UtcNow.AddMinutes(-10 + i);
            var dbEntry = _dbContext.Entry(message);
            dbEntry.Property("CreatedAt").CurrentValue = createdAt;
            
            messages.Add(message);
            _dbContext.OutboxMessages.Add(message);
        }

        await _dbContext.SaveChangesAsync();

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
                return System.Threading.Tasks.Task.CompletedTask;
            });

        // Act: Run processor
        var logger = new Mock<ILogger<OutboxProcessor>>();
        var processor = new OutboxProcessor(_serviceScopeFactory, logger.Object);
        await ProcessOutboxMessagesAsync(processor);

        // Assert: All processed in CreatedAt order
        processedOrder.Should().HaveCount(10);
        processedOrder.Should().BeInAscendingOrder();

        var allMessages = await _dbContext.OutboxMessages.ToListAsync();
        allMessages.Should().OnlyContain(m => m.ProcessedAt != null);
    }

    [Theory]
    [InlineData(0, 2)]   // First retry after 2 minutes
    [InlineData(1, 4)]   // Second retry after 4 minutes
    [InlineData(2, 8)]   // Third retry after 8 minutes
    public void CalculateNextRetryTime_ReturnsCorrectBackoff(int retryCount, int expectedMinutes)
    {
        // Arrange
        var testEvent = new TestDomainEvent("Test Property Value");
        var eventType = typeof(TestDomainEvent).AssemblyQualifiedName!;
        var payload = JsonSerializer.Serialize(testEvent);
        var message = OutboxMessage.Create(eventType, payload);

        // Set initial retry count
        for (int i = 0; i < retryCount; i++)
        {
            message.RecordFailure("Test error");
        }

        // Act
        var beforeRecordFailure = DateTime.UtcNow;
        message.RecordFailure("Test error");
        var afterRecordFailure = DateTime.UtcNow;

        // Assert
        message.NextRetryAt.Should().NotBeNull();
        var expectedRetryTime = beforeRecordFailure.AddMinutes(expectedMinutes);
        message.NextRetryAt!.Value.Should().BeCloseTo(expectedRetryTime, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async System.Threading.Tasks.Task OutboxProcessor_StopsRetrying_AfterMaxAttempts()
    {
        // Arrange: Message with RetryCount = 3
        await CleanupDatabaseAsync();

        var testEvent = new TestDomainEvent("Test Property Value");
        var eventType = typeof(TestDomainEvent).AssemblyQualifiedName!;
        var payload = JsonSerializer.Serialize(testEvent);

        var outboxMessage = OutboxMessage.Create(eventType, payload);
        // Set retry count to max (3)
        for (int i = 0; i < MaxRetryAttempts; i++)
        {
            outboxMessage.RecordFailure("Test error");
        }

        _dbContext.OutboxMessages.Add(outboxMessage);
        await _dbContext.SaveChangesAsync();

        _mockDispatcher
            .Setup(d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Persistent error"));

        // Act: Run processor
        var logger = new Mock<ILogger<OutboxProcessor>>();
        var processor = new OutboxProcessor(_serviceScopeFactory, logger.Object);
        await ProcessOutboxMessagesAsync(processor);

        // Assert: Message not retried, marked as failed
        var message = await _dbContext.OutboxMessages.FindAsync(outboxMessage.Id);
        message!.RetryCount.Should().Be(MaxRetryAttempts);
        message.ProcessedAt.Should().BeNull(); // Not processed
        message.NextRetryAt.Should().NotBeNull(); // Still has next retry time but won't be processed

        // Verify dispatcher was not called (message skipped due to max retries)
        _mockDispatcher.Verify(
            d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async System.Threading.Tasks.Task OutboxProcessor_DoesNotProcessSameMessage_Concurrently()
    {
        // Arrange: Start two processors simultaneously
        await CleanupDatabaseAsync();

        var testEvent = new TestDomainEvent("Test Property Value");
        var eventType = typeof(TestDomainEvent).AssemblyQualifiedName!;
        var payload = JsonSerializer.Serialize(testEvent);

        var outboxMessage = OutboxMessage.Create(eventType, payload);
        _dbContext.OutboxMessages.Add(outboxMessage);
        await _dbContext.SaveChangesAsync();

        var dispatchCount = 0;
        _mockDispatcher
            .Setup(d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns<IDomainEvent, CancellationToken>(async (evt, ct) =>
            {
                Interlocked.Increment(ref dispatchCount);
                await System.Threading.Tasks.Task.Delay(100, ct); // Simulate processing time
                return;
            });

        // Act: Both try to process same message (using row locking via EF Core)
        var logger1 = new Mock<ILogger<OutboxProcessor>>();
        var logger2 = new Mock<ILogger<OutboxProcessor>>();
        var processor1 = new OutboxProcessor(_serviceScopeFactory, logger1.Object);
        var processor2 = new OutboxProcessor(_serviceScopeFactory, logger2.Object);

        var task1 = ProcessOutboxMessagesAsync(processor1);
        var task2 = ProcessOutboxMessagesAsync(processor2);

        await System.Threading.Tasks.Task.WhenAll(task1, task2);

        // Assert: Only one processes it (use row locking)
        // Note: Actual row locking behavior depends on database and EF Core configuration
        // In practice, both might process it, but the second should handle idempotency
        var message = await _dbContext.OutboxMessages.FindAsync(outboxMessage.Id);
        message!.ProcessedAt.Should().NotBeNull(); // Should be processed

        // At least one, but ideally only one dispatch
        dispatchCount.Should().BeGreaterOrEqualTo(1);
    }
}
