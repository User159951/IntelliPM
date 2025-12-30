namespace IntelliPM.Domain.Constants;

/// <summary>
/// Constants for AI decision logging and governance.
/// </summary>
public static class AIDecisionConstants
{
    /// <summary>
    /// Types of AI decisions that can be logged.
    /// </summary>
    public static class DecisionTypes
    {
        public const string RiskDetection = "RiskDetection";
        public const string SprintPlanning = "SprintPlanning";
        public const string TaskPrioritization = "TaskPrioritization";
        public const string ResourceAllocation = "ResourceAllocation";
        public const string DeadlineEstimation = "DeadlineEstimation";
        public const string CodeReview = "CodeReview";
        public const string TestCoverage = "TestCoverage";
        public const string PerformanceAnalysis = "PerformanceAnalysis";
    }

    /// <summary>
    /// Types of AI agents that make decisions.
    /// </summary>
    public static class AgentTypes
    {
        public const string ProductAgent = "ProductAgent";
        public const string DeliveryAgent = "DeliveryAgent";
        public const string ManagerAgent = "ManagerAgent";
        public const string QAAgent = "QAAgent";
        public const string BusinessAgent = "BusinessAgent";
    }

    /// <summary>
    /// Status values for AI decisions.
    /// </summary>
    public static class Statuses
    {
        public const string Pending = "Pending";
        public const string Applied = "Applied";
        public const string Rejected = "Rejected";
        public const string Overridden = "Overridden";
        public const string PendingApproval = "PendingApproval";
    }

    /// <summary>
    /// Minimum confidence score for auto-applying decisions without human approval.
    /// </summary>
    public const decimal MinConfidenceScore = 0.7m;

    /// <summary>
    /// Maximum length for reasoning text.
    /// </summary>
    public const int MaxReasoningLength = 10000;
}

