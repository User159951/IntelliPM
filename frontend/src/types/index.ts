// Auth types
export type GlobalRole = 'User' | 'Admin';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  // Tokens are now in httpOnly cookies, not in response body
  // Response contains user info only
  userId: number;
  username: string;
  email: string;
  roles?: string[];
  message?: string;
  // Legacy fields for backward compatibility (deprecated)
  token?: string;
  accessToken?: string;
  refreshToken?: string;
}

export interface User {
  userId: number;
  username: string;
  email: string;
  firstName?: string;
  lastName?: string;
  globalRole: GlobalRole;
  organizationId: number;
  permissions: string[];
  // Legacy field for backward compatibility (deprecated)
  roles?: string[];
}

// Project types
export type ProjectStatus = 'Active' | 'OnHold' | 'Completed' | 'Archived';
export type ProjectType = 'Scrum' | 'Kanban' | 'Waterfall';
export type ProjectRole = 'ProductOwner' | 'ScrumMaster' | 'Developer' | 'Tester' | 'Viewer';

export interface ProjectMember {
  id: number;
  userId: number;
  userName: string;
  firstName?: string;
  lastName?: string;
  email: string;
  avatar?: string | null;
  role?: ProjectRole | 'Owner' | 'Admin' | 'Member'; // Support both old and new role formats
  invitedAt?: string;
  invitedByName?: string;
  currentWorkload?: {
    taskCount: number;
    storyPoints: number;
  };
  status?: 'Available' | 'Busy' | 'Off';
}

export interface Project {
  id: number;
  name: string;
  description: string;
  type: ProjectType;
  status: ProjectStatus;
  sprintDurationDays: number;
  ownerId: number;
  ownerName?: string;
  createdAt: string;
  openTasksCount?: number;
  activeSprintId?: number;
  members?: ProjectMember[];
  startDate?: string; // ISO date string
  endDate?: string; // ISO date string
}

export interface CreateProjectRequest {
  name: string;
  description: string;
  type: ProjectType;
  sprintDurationDays: number;
  status?: ProjectStatus;
  startDate?: string; // ISO date string
  memberIds?: number[];
}

export interface UpdateProjectRequest {
  name?: string;
  description?: string;
  status?: ProjectStatus;
  type?: ProjectType;
  sprintDurationDays?: number;
}

// Sprint types
export type SprintStatus = 'Planned' | 'Active' | 'Completed';

export interface Sprint {
  id: number;
  name: string;
  projectId: number;
  startDate: string;
  endDate: string;
  status: SprintStatus;
  capacity: number;
  goal?: string;
  tasks?: Task[];
}

export interface CreateSprintRequest {
  name: string;
  projectId: number;
  startDate: string;
  endDate: string;
  capacity: number;
  goal?: string;
}

// Task types
export type TaskStatus = 'Todo' | 'InProgress' | 'Blocked' | 'Done';
export type TaskPriority = 'Low' | 'Medium' | 'High' | 'Critical';

export interface Task {
  id: number;
  title: string;
  description: string;
  projectId: number;
  sprintId?: number;
  status: TaskStatus;
  priority: TaskPriority;
  storyPoints?: number;
  assigneeId?: number;
  assigneeName?: string;
  createdAt: string;
  updatedAt?: string;
  dueDate?: string;
  type?: 'Bug' | 'Feature' | 'Task' | 'Epic';
}

export interface CreateTaskRequest {
  title: string;
  description: string;
  projectId: number;
  priority: TaskPriority;
  storyPoints?: number;
  assigneeId?: number;
  type?: 'Bug' | 'Feature' | 'Task' | 'Epic';
  sprintId?: number;
  dueDate?: string;
  tags?: string[];
  acceptanceCriteria?: string[];
}

export interface UpdateTaskRequest {
  title?: string;
  description?: string;
  priority?: TaskPriority;
  storyPoints?: number;
  assigneeId?: number;
  sprintId?: number;
  dueDate?: string;
  type?: 'Bug' | 'Feature' | 'Task';
  tags?: string[];
  acceptanceCriteria?: string[];
}

export interface TaskComment {
  id: number;
  taskId: number;
  userId: number;
  userName: string;
  userAvatar?: string;
  content: string;
  createdAt: string;
  updatedAt?: string;
}

export interface TaskAttachment {
  id: number;
  taskId: number;
  fileName: string;
  fileSize: number;
  fileType: string;
  fileUrl: string;
  uploadedBy: number;
  uploadedByName: string;
  uploadedAt: string;
}

export interface TaskActivity {
  id: number;
  taskId: number;
  userId: number;
  userName: string;
  action: string; // 'created', 'updated', 'status_changed', 'assigned', etc.
  field?: string; // 'status', 'priority', etc.
  oldValue?: string;
  newValue?: string;
  createdAt: string;
}

// Backlog types
export interface Epic {
  id: number;
  title: string;
  description: string;
  projectId: number;
  features?: Feature[];
}

export interface Feature {
  id: number;
  title: string;
  description: string;
  storyPoints?: number;
  domainTag?: string;
  epicId: number;
  stories?: Story[];
}

export interface Story {
  id: number;
  title: string;
  description: string;
  storyPoints?: number;
  domainTag?: string;
  featureId: number;
  acceptanceCriteria?: string;
}

export interface CreateEpicRequest {
  title: string;
  description: string;
}

export interface CreateFeatureRequest {
  title: string;
  description: string;
  storyPoints?: number;
  domainTag?: string;
  epicId: number;
}

export interface CreateStoryRequest {
  title: string;
  description: string;
  storyPoints?: number;
  domainTag?: string;
  featureId: number;
  acceptanceCriteria?: string;
}

// Defect types
export type DefectSeverity = 'Low' | 'Medium' | 'High' | 'Critical';
export type DefectStatus = 'Open' | 'InProgress' | 'Resolved' | 'Closed';

export interface Defect {
  id: number;
  title: string;
  description: string;
  severity: DefectSeverity;
  status: DefectStatus;
  projectId: number;
  userStoryId?: number;
  sprintId?: number;
  assignedToId?: number;
  assignedToName?: string;
  reportedById?: number;
  reportedByName?: string;
  foundInEnvironment?: string;
  stepsToReproduce?: string;
  createdAt: string;
  updatedAt?: string;
  createdBy?: string;
  resolvedAt?: string;
}

export interface CreateDefectRequest {
  title: string;
  description: string;
  severity: DefectSeverity;
  userStoryId?: number;
  sprintId?: number;
  foundInEnvironment?: string;
  stepsToReproduce?: string;
  assignedToId?: number;
}

export interface UpdateDefectRequest {
  title?: string;
  description?: string;
  severity?: DefectSeverity;
  status?: DefectStatus;
  assignedToId?: number;
  foundInEnvironment?: string;
  stepsToReproduce?: string;
  resolution?: string;
}

// Team types
export interface Team {
  id: number;
  name: string;
  totalCapacity: number;
  members: TeamMember[];
}

export interface TeamMember {
  id: number;
  name: string;
  email?: string;
  role?: string;
}

export interface TeamCapacity {
  teamId: number;
  totalCapacity: number;
  utilizedCapacity: number;
  availableCapacity: number;
}

export interface RegisterTeamRequest {
  name: string;
  memberIds: number[];
  totalCapacity: number;
}

// Metrics types
export interface MetricsSummary {
  totalProjects?: number;
  activeSprints?: number;
  openTasks?: number;
  blockedTasks?: number;
  defectsCount?: number;
  velocity?: number;
  throughput?: number;
  defectRate?: number;
  sprintHealth?: string;
  deliveryPredictability?: number;
}

// Insight types
export interface Insight {
  id: number;
  title: string;
  description: string;
  type: string;
  agentType?: string;
  status: string;
  projectId: number;
  createdAt: string;
}

// Agent types
export interface AgentResponse {
  content: string;
  status: 'Success' | 'Error' | 'ProposalReady';
  requiresApproval?: boolean;
  executionCostUsd: number;
  executionTimeMs: number;
  toolsCalled: string[];
  timestamp: string;
  errorMessage?: string;
  metadata?: Record<string, unknown>;
}

export interface ImproveTaskRequest {
  description: string;
  projectId?: number;
  title?: string;
  context?: string;
}

export interface ImprovedTaskData {
  title: string;
  description: string;
  acceptanceCriteria: string[];
  definitionOfDone?: string[];
  suggestedPriority: TaskPriority;
  suggestedStoryPoints: number;
  type: 'Bug' | 'Feature' | 'Task';
}
