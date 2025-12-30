import { apiClient } from './client';
import type { AgentResponse, ImproveTaskRequest } from '@/types';

export const agentsApi = {
  // Project-scoped agents (RAG-style)
  runProductAgent: (projectId: number): Promise<AgentResponse> =>
    apiClient.post(`/api/projects/${projectId}/agents/run-product`),

  runDeliveryAgent: (projectId: number): Promise<AgentResponse> =>
    apiClient.post(`/api/projects/${projectId}/agents/run-delivery`),

  runManagerAgent: (projectId: number): Promise<AgentResponse> =>
    apiClient.post(`/api/projects/${projectId}/agents/run-manager`),

  runQAAgent: (projectId: number): Promise<AgentResponse> =>
    apiClient.post(`/api/projects/${projectId}/agents/run-qa`),

  runBusinessAgent: (projectId: number): Promise<AgentResponse> =>
    apiClient.post(`/api/projects/${projectId}/agents/run-business`),

  // Semantic Kernel-powered task improver
  improveTask: (data: ImproveTaskRequest): Promise<AgentResponse> =>
    apiClient.post('/api/Agent/improve-task', data),

  // Semantic Kernel-powered project risk analysis (existing endpoint)
  analyzeRisks: (projectId: number): Promise<AgentResponse> =>
    apiClient.get(`/api/Agent/analyze-risks/${projectId}`),

  // New: Project Insight Agent
  analyzeProject: (projectId: number): Promise<AgentResponse> =>
    apiClient.post(`/api/Agent/analyze-project/${projectId}`),

  // New: Risk Detection Agent (Semantic Kernel tools)
  detectRisks: (projectId: number): Promise<AgentResponse> =>
    apiClient.post(`/api/Agent/detect-risks/${projectId}`),

  // New: Sprint Planning Agent
  planSprint: (sprintId: number): Promise<AgentResponse> =>
    apiClient.post(`/api/Agent/plan-sprint/${sprintId}`),
};
