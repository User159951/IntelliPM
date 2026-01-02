import { apiClient } from './client';
import type { AgentResponse, ImproveTaskRequest } from '@/types';
import type {
  AgentMetrics,
  AgentAuditLogsResponse,
  SprintRetrospective,
} from '@/types/agents';

/**
 * API client for AI Agent endpoints
 * Provides methods to interact with AI agents for project analysis, task improvement, and more.
 */
export const agentsApi = {
  /**
   * Run Product Agent for project analysis
   * Analyzes the project from a product perspective, providing insights on features, user stories, and product strategy.
   * @param projectId - Project ID to analyze
   * @returns Agent execution result with product analysis
   */
  runProductAgent: (projectId: number): Promise<AgentResponse> =>
    apiClient.post(`/projects/${projectId}/agents/run-product`),

  /**
   * Run Delivery Agent for project analysis
   * Analyzes the project from a delivery perspective, focusing on sprint planning, velocity, and delivery timelines.
   * @param projectId - Project ID to analyze
   * @returns Agent execution result with delivery analysis
   */
  runDeliveryAgent: (projectId: number): Promise<AgentResponse> =>
    apiClient.post(`/projects/${projectId}/agents/run-delivery`),

  /**
   * Run Manager Agent for project analysis
   * Analyzes the project from a management perspective, providing insights on resource allocation, risks, and key decisions.
   * @param projectId - Project ID to analyze
   * @returns Agent execution result with management analysis
   */
  runManagerAgent: (projectId: number): Promise<AgentResponse> =>
    apiClient.post(`/projects/${projectId}/agents/run-manager`),

  /**
   * Run QA Agent for project analysis
   * Analyzes the project from a quality assurance perspective, identifying defects, test coverage, and quality metrics.
   * @param projectId - Project ID to analyze
   * @returns Agent execution result with QA analysis
   */
  runQAAgent: (projectId: number): Promise<AgentResponse> =>
    apiClient.post(`/projects/${projectId}/agents/run-qa`),

  /**
   * Run Business Agent for project analysis
   * Analyzes the project from a business perspective, providing insights on ROI, business value, and strategic alignment.
   * @param projectId - Project ID to analyze
   * @returns Agent execution result with business analysis
   */
  runBusinessAgent: (projectId: number): Promise<AgentResponse> =>
    apiClient.post(`/projects/${projectId}/agents/run-business`),

  /**
   * Improves a messy task description using AI with automatic function calling
   * The AI will improve the description to be more detailed and professional.
   * @param data - Task description to improve
   * @returns Improved task description with suggestions
   */
  improveTask: (data: ImproveTaskRequest): Promise<AgentResponse> =>
    apiClient.post('/Agent/improve-task', data),

  /**
   * Analyzes project risks using AI (GET endpoint)
   * @param projectId - Project ID to analyze for risks
   * @returns Agent execution result with detected risks
   */
  analyzeRisks: (projectId: number): Promise<AgentResponse> =>
    apiClient.get(`/Agent/analyze-risks/${projectId}`),

  /**
   * Analyze project using AI agent
   * Provides comprehensive project analysis including insights, risks, and recommendations.
   * @param projectId - Project ID to analyze
   * @returns Agent execution result with comprehensive analysis
   */
  analyzeProject: (projectId: number): Promise<AgentResponse> =>
    apiClient.post(`/Agent/analyze-project/${projectId}`),

  /**
   * Detect risks in a project using AI agent
   * Identifies potential risks with severity and mitigation recommendations.
   * @param projectId - Project ID to analyze for risks
   * @returns Agent execution result with detected risks
   */
  detectRisks: (projectId: number): Promise<AgentResponse> =>
    apiClient.post(`/Agent/detect-risks/${projectId}`),

  /**
   * Plan sprint using AI agent
   * Generates a sprint plan with suggested tasks, capacity allocation, and recommendations.
   * @param sprintId - Sprint ID to plan
   * @returns Agent execution result with sprint plan
   */
  planSprint: (sprintId: number): Promise<AgentResponse> =>
    apiClient.post(`/Agent/plan-sprint/${sprintId}`),

  /**
   * Analyze task dependencies for a project using AI agent
   * Analyzes task dependencies and identifies circular dependencies and critical paths.
   * @param projectId - Project ID to analyze dependencies
   * @returns Agent execution result with dependency analysis
   */
  analyzeDependencies: (projectId: number): Promise<AgentResponse> =>
    apiClient.post(`/Agent/analyze-dependencies/${projectId}`),

  /**
   * Gets agent execution statistics and metrics
   * Returns metrics including total executions, success rate, average execution time, and cost.
   * @returns Agent execution metrics
   */
  getMetrics: (): Promise<AgentMetrics> =>
    apiClient.get<AgentMetrics>('/Agent/metrics'),

  /**
   * Gets paginated audit log of all agent executions
   * @param params - Query parameters for filtering and pagination
   * @param params.page - Page number (default: 1, minimum: 1)
   * @param params.pageSize - Number of items per page (default: 50, minimum: 1, maximum: 100)
   * @param params.agentId - Optional filter by agent ID
   * @param params.userId - Optional filter by user ID
   * @param params.status - Optional filter by status (Pending, Success, Error)
   * @returns Paginated list of agent execution logs
   */
  getAuditLog: (params?: {
    page?: number;
    pageSize?: number;
    agentId?: string;
    userId?: string;
    status?: string;
  }): Promise<AgentAuditLogsResponse> => {
    const queryParams = new URLSearchParams();
    if (params?.page) queryParams.append('page', params.page.toString());
    if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString());
    if (params?.agentId) queryParams.append('agentId', params.agentId);
    if (params?.userId) queryParams.append('userId', params.userId);
    if (params?.status) queryParams.append('status', params.status);

    const query = queryParams.toString();
    return apiClient.get<AgentAuditLogsResponse>(
      `/Agent/audit-log${query ? `?${query}` : ''}`
    );
  },

  /**
   * Store a note for an AI agent
   * Stores contextual notes that agents can use for future analysis.
   * @param projectId - Project ID
   * @param data - Note type and content
   * @returns Confirmation of storage
   */
  storeNote: (
    projectId: number,
    data: { type: string; content: string }
  ): Promise<{ success: boolean; noteId?: number }> =>
    apiClient.post<{ success: boolean; noteId?: number }>(
      `/projects/${projectId}/agents/notes`,
      data
    ),

  /**
   * Génère une rétrospective de sprint avec l'IA
   * Analyse le sprint terminé et génère une rétrospective complète avec ce qui a bien fonctionné,
   * ce qui peut être amélioré, les actions à prendre et les recommandations.
   * @param sprintId - ID du sprint
   * @returns Rétrospective générée (ce qui a bien fonctionné, ce qui peut être amélioré, actions)
   */
  generateRetrospective: async (sprintId: number): Promise<SprintRetrospective> => {
    const response = await apiClient.post<AgentResponse>(
      `/Agent/generate-retrospective/${sprintId}`
    );
    
    // Parse the JSON content from AgentResponse
    try {
      const parsed = JSON.parse(response.content);
      return parsed as SprintRetrospective;
    } catch {
      throw new Error('Failed to parse retrospective data from agent response');
    }
  },
};
