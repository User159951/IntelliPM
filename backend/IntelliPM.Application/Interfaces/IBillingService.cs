namespace IntelliPM.Application.Interfaces;

/// <summary>
/// Service for integrating with external billing systems (Stripe, PayPal, etc.).
/// Handles subscription updates, cancellations, and invoice generation.
/// </summary>
public interface IBillingService
{
    /// <summary>
    /// Updates subscription tier for an organization.
    /// Triggers webhook to external billing system.
    /// </summary>
    Task<BillingResult> UpdateSubscriptionAsync(UpdateSubscriptionRequest request, CancellationToken ct);

    /// <summary>
    /// Cancels subscription for an organization.
    /// </summary>
    Task<BillingResult> CancelSubscriptionAsync(int organizationId, CancellationToken ct);

    /// <summary>
    /// Generates invoice for an organization for a specific period.
    /// </summary>
    Task<BillingInvoice> GenerateInvoiceAsync(int organizationId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken ct);
}

/// <summary>
/// Request to update subscription tier.
/// </summary>
public class UpdateSubscriptionRequest
{
    public int OrganizationId { get; set; }
    public string OldTier { get; set; } = string.Empty;
    public string NewTier { get; set; } = string.Empty;
    public bool ApplyImmediately { get; set; }
    public DateTimeOffset? ScheduledDate { get; set; }
}

/// <summary>
/// Result from billing system operation.
/// </summary>
public class BillingResult
{
    public bool Success { get; set; }
    public string ReferenceId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Invoice generated for billing period.
/// </summary>
public class BillingInvoice
{
    public string InvoiceId { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset PeriodStart { get; set; }
    public DateTimeOffset PeriodEnd { get; set; }
    public List<BillingLineItem> LineItems { get; set; } = new();
}

/// <summary>
/// Line item in an invoice.
/// </summary>
public class BillingLineItem
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

