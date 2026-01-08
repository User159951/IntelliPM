// Enum types now imported from generated types
export type { ReleaseStatus, ReleaseType, QualityGateStatus, QualityGateType } from './generated/enums';

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

