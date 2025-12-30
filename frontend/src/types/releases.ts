export enum ReleaseStatus {
  Planned = 0,
  InProgress = 1,
  Testing = 2,
  ReadyForDeployment = 3,
  Deployed = 4,
  Failed = 5,
  Cancelled = 6,
}

export enum ReleaseType {
  Major = 0,
  Minor = 1,
  Patch = 2,
  Hotfix = 3,
}

export enum QualityGateStatus {
  Passed = 0,
  Warning = 1,
  Failed = 2,
  Pending = 3,
  Skipped = 4,
}

export enum QualityGateType {
  CodeCoverage = 0,
  AllTasksCompleted = 1,
  NoOpenBugs = 2,
  CodeReviewApproval = 3,
  SecurityScan = 4,
  PerformanceTests = 5,
  DocumentationComplete = 6,
  ManualApproval = 7,
}

export interface ReleaseDto {
  id: number;
  projectId: number;
  name: string;
  version: string;
  description: string | null;
  type: string; // "Major" | "Minor" | "Patch" | "Hotfix"
  status: string; // ReleaseStatus as string
  plannedDate: string;
  actualReleaseDate: string | null;
  releaseNotes: string | null;
  changeLog: string | null;
  isPreRelease: boolean;
  tagName: string | null;
  sprintCount: number;
  completedTasksCount: number;
  totalTasksCount: number;
  overallQualityStatus: string | null; // QualityGateStatus as string
  createdAt: string;
  createdByName: string;
  releasedByName: string | null;
  sprints?: ReleaseSprintDto[];
  qualityGates?: QualityGateDto[];
}

export interface CreateReleaseRequest {
  name: string;
  version: string;
  description?: string;
  type: string;
  plannedDate: string;
  isPreRelease?: boolean;
  tagName?: string;
  sprintIds?: number[];
}

export interface UpdateReleaseRequest {
  name: string;
  version: string;
  description?: string;
  plannedDate: string;
  status: string;
}

export interface ReleaseSprintDto {
  id: number;
  name: string;
  startDate: string;
  endDate: string;
  status: string;
  completedTasksCount: number;
  totalTasksCount: number;
  completionPercentage: number;
}

export interface QualityGateDto {
  id: number;
  releaseId: number;
  type: string; // QualityGateType as string
  status: string; // QualityGateStatus as string
  isRequired: boolean;
  threshold: number | null;
  actualValue: number | null;
  message: string;
  details: string | null;
  checkedAt: string | null;
  checkedByName: string | null;
}

export interface ReleaseStatistics {
  totalReleases: number;
  deployedReleases: number;
  plannedReleases: number;
  failedReleases: number;
  averageLeadTime: number; // days
}

