# IntelliPM Frontend Documentation

**Version:** 2.25.0  
**Last Updated:** January 15, 2026 (Complete Codebase Scan - Comprehensive Update)  
**Technology Stack:** React 18, TypeScript (Strict Mode), Vite, Tailwind CSS, shadcn/ui, TanStack Query

**Codebase Statistics:**
- **Total TypeScript/TSX Files:** 332 files (252 TSX + 80 TS, excluding test files)
- **Pages:** 44 page files (excluding test files)
- **Components:** 171 component files (excluding test files)
- **API Clients:** 36 API client files (excluding test files)
- **Hooks:** 16 custom hooks
- **Contexts:** 8 React context files
- **Test Files:** 31 test files (.test.tsx/.test.ts)

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Project Structure](#project-structure)
4. [Technology Stack](#technology-stack)
5. [State Management](#state-management)
6. [Routing](#routing)
7. [API Integration](#api-integration)
8. [Components](#components)
9. [Pages](#pages)
10. [Styling](#styling)
11. [Internationalization (i18n)](#11-internationalization-i18n)
12. [Forms & Validation](#forms--validation)
13. [Testing](#testing)
14. [Development Setup](#development-setup)
15. [Build & Deployment](#build--deployment)
16. [Best Practices](#best-practices)
17. [Troubleshooting](#troubleshooting)
18. [TypeScript](#typescript)
19. [API Integration Patterns](#api-integration-patterns)
20. [Feature Flags](#feature-flags)
21. [SweetAlert2 Integration](#sweetalert2-integration)
22. [Custom Hooks](#custom-hooks)
23. [Accessibility](#accessibility)
24. [Performance Optimization](#performance-optimization)
25. [Security](#security)
26. [Monitoring & Analytics](#monitoring--analytics)
27. [Future Improvements](#future-improvements)
28. [Contributing](#contributing)
29. [Missing Features](#missing-features)
30. [API Integration Status](#api-integration-status)

---

## 1. Overview

### 1.1 Introduction

IntelliPM Frontend is a modern, responsive React application built with TypeScript that provides an intuitive interface for project management with AI-powered features. The application follows modern React patterns and best practices for maintainability and scalability.

### 1.2 Key Features

- **Modern UI/UX**: Built with shadcn/ui components and Tailwind CSS
- **Responsive Design**: Mobile-first approach with adaptive layouts
- **Dark Mode**: System-aware theme switching
- **Real-time Updates**: TanStack Query for efficient data fetching and caching
- **Type Safety**: Full TypeScript coverage with strict mode enabled
- **AI Integration**: Interactive AI agents for project insights
- **Role-Based Access**: Dynamic UI based on user permissions
- **Feature Flags**: Runtime feature toggling with context-based caching
- **Permission System**: Comprehensive permission checking with usePermissions hook
- **Global Search**: Quick navigation with keyboard shortcuts (Ctrl/Cmd+K)
- **Error Handling**: Comprehensive error boundaries, global error toasts, and user-friendly error messages
- **Comment System**: Threaded comments with @username mentions
- **File Attachments**: Drag-and-drop file uploads with validation
- **Notification Bell**: Real-time notification badge and dropdown
- **AI Governance**: AI decision logging, quota management, and usage tracking
- **Milestones Management**: Complete milestone tracking with statistics and timeline views
- **Release Management**: Release planning, quality gates, and release notes generation
- **Task Dependencies**: Visual dependency graphs and dependency management
- **Internationalization (i18n)**: Multi-language support (English, French) with dynamic language switching and backend sync
- **Lookup Data API**: Reference data endpoints for project types, task statuses, and priorities with metadata

### 1.3 Design Principles

- **Component-Based Architecture**: Reusable, composable components
- **Separation of Concerns**: Clear separation between UI, logic, and data
- **Type Safety**: TypeScript strict mode for comprehensive compile-time error detection
- **Performance**: Code splitting, lazy loading, and efficient re-renders
- **Accessibility**: WCAG-compliant components from Radix UI
- **Developer Experience**: Hot module replacement, fast builds, comprehensive tooling

---

## 2. Architecture

### 2.1 Application Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│  (Pages, Components, UI Components)                     │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                  Application Layer                        │
│  (Hooks, Contexts, Business Logic)                      │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                    Data Layer                             │
│  (API Clients, TanStack Query, State Management)        │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                  Backend API                              │
│  (REST API, Authentication, WebSockets)                 │
└─────────────────────────────────────────────────────────┘
```

### 2.2 Design Patterns

- **Container/Presenter Pattern**: Separation of logic and presentation
- **Custom Hooks**: Reusable business logic
- **Context API**: Global state management (Auth, Theme)
- **Render Props**: Flexible component composition
- **Compound Components**: Related components working together

### 2.3 Data Flow

1. **User Action** → Component Event Handler
2. **Event Handler** → API Call (via API client)
3. **API Client** → Backend API
4. **Response** → TanStack Query Cache
5. **Cache Update** → Component Re-render
6. **UI Update** → User Feedback

---

## 3. Project Structure

### 3.1 Directory Structure

```
frontend/
├── public/                    # Static assets
│   ├── favicon.ico
│   └── placeholder.svg
├── src/
│   ├── api/                   # API client modules (37 API clients, 3 test files = 40 files total) ✅ Verified
│   │   ├── client.ts          # Base API client with token refresh
│   │   ├── auth.ts            # Authentication API
│   │   ├── projects.ts        # Projects API
│   │   ├── tasks.ts           # Tasks API
│   │   ├── adminAiQuota.ts    # Admin member AI quota management
│   │   ├── adminAIQuotas.ts   # Admin organization AI quotas
│   │   ├── superAdminAIQuota.ts # SuperAdmin organization AI quota
│   │   ├── organizations.ts   # Organization management (Admin)
│   │   ├── organizationPermissionPolicy.ts # Permission policy (SuperAdmin)
│   │   ├── memberPermissions.ts # Member permissions (Admin)
│   │   ├── milestones.ts      # Milestone management
│   │   ├── releases.ts        # Release management
│   │   ├── dependencies.ts    # Task dependency management
│   │   ├── sprints.ts         # Sprints API
│   │   ├── defects.ts         # Defects API
│   │   ├── teams.ts           # Teams API
│   │   ├── metrics.ts         # Metrics API
│   │   ├── agents.ts          # AI agents API
│   │   ├── notifications.ts   # Notifications API
│   │   ├── search.ts          # Global search API
│   │   ├── settings.ts        # Settings API
│   │   ├── permissions.ts     # Permissions API
│   │   ├── users.ts           # Users API
│   │   ├── activity.ts        # Activity feed API
│   │   ├── alerts.ts          # Alerts API
│   │   ├── insights.ts        # Project insights API
│   │   ├── backlog.ts         # Backlog API
│   │   ├── memberService.ts   # Project member operations
│   │   ├── admin.ts           # Admin API
│   │   ├── auditLogs.ts       # Audit logs API
│   │   ├── comments.ts        # Comments API
│   │   ├── attachments.ts     # Attachments API
│   │   ├── aiGovernance.ts    # AI governance API
│   │   ├── lookups.ts         # Lookup/reference data API (project types, task statuses, priorities) with metadata
│   │   ├── language.ts        # Language preference API (getUserLanguage, updateUserLanguage)
│   │   └── adminAIQuotas.ts   # Admin member AI quotas (new model with OrganizationAIQuota)
│   ├── components/            # React components (163 files, excluding test files) ✅ Verified
│   │   ├── ui/                # shadcn/ui components (51 components)
│   │   ├── layout/            # Layout components
│   │   ├── auth/              # Authentication components (4 components)
│   │   │   ├── Logo.tsx       # IntelliPM logo with variants
│   │   │   ├── GeometricShapes.tsx # Animated decorative shapes
│   │   │   ├── LoginForm.tsx  # Standalone login form
│   │   │   └── PasswordStrengthIndicator.tsx # Password strength indicator
│   │   ├── admin/             # Admin-specific components
│   │   ├── agents/            # AI agent components
│   │   ├── projects/          # Project-related components
│   │   ├── tasks/             # Task-related components
│   │   ├── sprints/           # Sprint-related components
│   │   ├── defects/           # Defect-related components
│   │   ├── notifications/     # Notification components
│   │   ├── comments/          # Comment components
│   │   ├── attachments/       # Attachment components
│   │   ├── search/            # Search components
│   │   ├── guards/            # Route guards
│   │   └── dashboard/         # Dashboard components
│   ├── contexts/              # React contexts (9 files: 7 main .tsx + 2 test .tsx files) ✅ Verified
│   │   ├── AuthContext.tsx    # Authentication context
│   │   ├── ThemeContext.tsx   # Theme context
│   │   ├── ProjectContext.tsx # Project context
│   │   ├── FeatureFlagsContext.tsx # Feature flags context
│   │   ├── LanguageContext.tsx # Language/i18n context
│   │   └── PermissionContext.tsx # Permission context
│   ├── hooks/                 # Custom hooks (16 hooks: 15 .ts + 1 .tsx) ✅ Verified
│   │   ├── use-debounce.ts    # Debounce hook
│   │   ├── use-mobile.tsx     # Mobile detection hook
│   │   ├── useProjectPermissions.ts # Project-level permission hook
│   │   ├── usePermissions.ts  # Global permission checking hook
│   │   ├── useFeatureFlag.ts  # Feature flag checking hook
│   │   ├── useReadModels.ts   # Read model hooks (task board, sprint summary, project overview)
│   │   ├── use-toast.ts       # Toast notification hook (legacy)
│   │   ├── use-debounce.ts    # Debounce hook
│   │   ├── use-debounce.test.ts # Debounce hook tests
│   │   ├── useAIErrorHandler.ts # AI error handling hook (quota exceeded, AI disabled)
│   │   ├── useQuotaNotifications.ts # Quota notification hook (80% warning, 100% error toasts)
│   │   ├── useTaskDependencies.ts # Task dependencies hook
│   │   ├── useProjectTaskDependencies.ts # Project task dependencies hook
│   │   ├── useLookups.ts      # Lookup data hook
│   │   ├── usePermissions.ts  # Permission checking hook
│   │   ├── useProjectPermissions.ts # Project permission checking hook
│   │   ├── useFeatureFlag.ts  # Feature flag checking hook
│   │   ├── useUserRole.ts     # User role hook
│   │   ├── useDebouncedCallback.ts # Debounced callback hook
│   │   ├── useRequestDeduplication.ts # Request deduplication hook
│   │   ├── useReadModels.ts   # Read models hook
│   │   └── useTranslation.ts # i18n translation hook with safeT helper
│   ├── pages/                 # Page components (51 pages, excluding test files) ✅ Verified
│   │   ├── auth/              # Authentication pages (5 pages: Login, Register, ForgotPassword, ResetPassword, AcceptInvite)
│   │   │   ├── Login.tsx
│   │   │   ├── Register.tsx
│   │   │   ├── ForgotPassword.tsx
│   │   │   ├── ResetPassword.tsx
│   │   │   └── AcceptInvite.tsx
│   │   ├── admin/             # Admin pages (19 pages including components)
│   │   │   ├── AdminDashboard.tsx
│   │   │   ├── AdminUsers.tsx
│   │   │   ├── AdminPermissions.tsx
│   │   │   ├── AdminSettings.tsx
│   │   │   ├── AdminSystemHealth.tsx
│   │   │   ├── AdminAuditLogs.tsx
│   │   │   ├── AIGovernance.tsx
│   │   │   ├── AdminAIQuota.tsx
│   │   │   ├── AdminOrganizations.tsx
│   │   │   ├── AdminOrganizationDetail.tsx
│   │   │   ├── AdminMyOrganization.tsx
│   │   │   ├── AdminOrganizationMembers.tsx
│   │   │   ├── AdminMemberAIQuotas.tsx
│   │   │   └── AdminMemberPermissions.tsx
│   │   ├── superadmin/        # SuperAdmin pages (2 pages)
│   │   │   ├── SuperAdminOrganizationAIQuota.tsx
│   │   │   └── SuperAdminOrganizationPermissions.tsx
│   │   │   └── SuperAdminOrganizationPermissions.tsx
│   │   ├── ReleaseDetailPage.tsx # Release detail page
│   │   ├── ReleaseHealthDashboard.tsx # Release health dashboard
│   ├── dev/                   # Development tools (excluded from production builds)
│   │   ├── ReleaseApiTest.tsx # Release API connectivity test page (dev only)
│   │   └── README.md          # Documentation for dev tools
│   │   └── [feature].tsx      # Feature pages (Dashboard, Projects, Tasks, etc.)
│   ├── lib/                   # Utility functions
│   │   ├── utils.ts           # Common utilities
│   │   └── sweetalert.ts      # SweetAlert2 wrapper utilities
│   ├── services/              # Service layer
│   │   ├── featureFlagService.ts # Feature flag service with caching
│   │   └── readModelService.ts   # Read model service for CQRS read models
│   ├── types/                 # TypeScript types
│   │   ├── index.ts           # Type definitions
│   │   ├── featureFlags.ts    # Feature flag types and enums
│   │   ├── agents.ts          # AI agent output types (PrioritizedItem, DefectPattern, ValueMetric, etc.)
│   │   └── aiGovernance.ts    # AI governance types (QuotaStatus, QuotaDetails, etc.)
│   ├── utils/                 # Utility functions
│   │   ├── featureFlags.ts    # Feature flag utility functions
│   │   └── testReleaseApiConnectivity.ts # API connectivity test utility for Release endpoints (dev only)
│   ├── mocks/                 # Mock data for testing
│   │   ├── data.ts
│   │   └── server.ts          # MSW mock server
│   ├── test/                  # Test utilities
│   │   ├── setup.ts
│   │   └── test-utils.tsx
│   ├── App.tsx                # Root component
│   ├── main.tsx               # Application entry point
│   └── index.css              # Global styles
├── .eslintrc.js               # ESLint configuration
├── components.json            # shadcn/ui configuration
├── index.html                 # HTML template
├── package.json               # Dependencies
├── postcss.config.js          # PostCSS configuration
├── tailwind.config.ts         # Tailwind CSS configuration
├── tsconfig.json              # TypeScript configuration
├── tsconfig.app.json          # App-specific TS config
├── tsconfig.node.json         # Node-specific TS config
├── vite.config.ts             # Vite configuration
└── vitest.config.ts           # Vitest configuration
```

### 3.2 Key Directories

#### 3.2.1 `/src/api/`

API client modules organized by feature:
- `client.ts`: Base API client with error handling
- `auth.ts`: Authentication endpoints
- `projects.ts`: Project management (CRUD, members, teams, dependency graph)
- `permissions.ts`: Permissions API (global permissions, project permissions via getProjectPermissions)
- `tasks.ts`: Task management
- `sprints.ts`: Sprint management
- `defects.ts`: Defect tracking
- `teams.ts`: Team management
- `metrics.ts`: Metrics and analytics
- `agents.ts`: AI agent interactions
- `notifications.ts`: Notification management
- `search.ts`: Global search
- `settings.ts`: Settings management (getAll, update, sendTestEmail)
- `permissions.ts`: Permission management
- `users.ts`: User management (getAllPaginated, getUserProjects, getUserActivity, update, delete, bulkUpdateStatus)
- `activity.ts`: Activity feed
- `alerts.ts`: Alert management
- `insights.ts`: Project insights
- `backlog.ts`: Backlog management
- `memberService.ts`: Project member operations
- `admin.ts`: Admin operations (getDashboardStats, getSystemHealth)
- `auditLogs.ts`: Audit logs
- `comments.ts`: Comment management (add, get, update, delete)
- `attachments.ts`: File attachment operations (upload, get, delete, list)
- `aiGovernance.ts`: AI governance API
  - User endpoints: getQuotaStatus (mocked, ready for backend), getDecisions, getUsageStatistics
  - Admin endpoints: getAllDecisions, getAllQuotas, updateQuota, disableAI, enableAI, getOverviewStats
  - Note: getQuotaStatus and getQuotaDetails currently use mock data, awaiting backend endpoint implementation
- `notifications.ts`: Notification management (updated with preferences)
- `milestones.ts`: Milestone management (get, create, update, delete, complete, statistics)
- `releases.ts`: Release management (get, create, update, delete, deploy, quality gates, release notes)
- `dependencies.ts`: Task dependency management (get, add, remove, graph)
- `featureFlags.ts`: Feature flags API (if separate from settings)
- `organizationPermissionPolicy.ts`: Organization permission policy API (SuperAdmin only)
  - getOrganizationPermissionPolicy: Get permission policy for an organization
  - upsertOrganizationPermissionPolicy: Create or update permission policy
- `memberPermissions.ts`: Member permissions management API (Admin only)
  - getMemberPermissions: Get paginated list of members with permissions
  - updateMemberPermission: Update member role and/or permissions
- `lookups.ts`: Lookup/reference data API (project types, task statuses, priorities with metadata)
- `language.ts`: Language preference API (getUserLanguage, updateUserLanguage with backend sync)
  - getProjectTypes: Get project types with metadata (Scrum, Kanban, Waterfall)
  - getTaskStatuses: Get task statuses with metadata (Todo, InProgress, Blocked, Done)
  - getTaskPriorities: Get task priorities with metadata (Low, Medium, High, Critical)
- `language.ts`: Language preference API (backend sync)
  - getUserLanguage: Get user's language preference from backend (via Settings API)
  - updateUserLanguage: Update user's language preference on backend (via Settings API)
  - Note: Falls back to 'en' if backend fails, allows local language change even if backend update fails

#### 3.2.2 `/src/components/`

Component library organized by feature:

- **`ui/`**: shadcn/ui base components (51 components)
- **`layout/`**: Layout components (MainLayout, AdminLayout, Sidebar, Header)
- **`guards/`**: Route protection components (RequireAdminGuard, PermissionGuard)
- **`admin/`**: Admin-specific components (4 components)
  - InviteUserDialog
  - EditUserDialog
  - DeleteUserDialog
  - UserDetailDialog
- **`agents/`**: AI agent UI components
  - ProjectInsightPanel.tsx
  - RiskDetectionPanel.tsx
  - SprintPlanningAssistant.tsx
  - ProjectAnalysisPanel.tsx (new - comprehensive project analysis)
  - RiskDetectionDashboard.tsx (new - interactive risk detection)
  - **`results/`**: Agent result display components (6 components)
    - AgentResultsDisplay.tsx (wrapper component)
    - ProductAgentResults.tsx (prioritized items table)
    - QAAgentResults.tsx (defect patterns with charts)
    - BusinessAgentResults.tsx (value metrics with trends)
    - ManagerAgentResults.tsx (executive summary)
    - DeliveryAgentResults.tsx (milestones, risks, action items)
- **`ai-governance/`**: AI governance and quota management components (4 components)
  - QuotaStatusWidget.tsx (quota display widget)
  - QuotaAlertBanner.tsx (quota threshold alerts)
  - QuotaExceededAlert.tsx (quota exceeded alert)
  - AIDisabledAlert.tsx (AI disabled alert)
- **`projects/`**: Project-related components
- **`tasks/`**: Task-related components
  - TaskBoard.tsx (Kanban board with drag-and-drop)
  - StatusBadge.tsx (Reusable status badge component)
  - TaskImproverDialog.tsx (new - AI task improvement dialog)
  - DependencyAnalyzerPanel.tsx (new - task dependency analysis)
- **`users/`**: User-related components
  - UserCard.tsx (User card display component)
  - RoleBadge.tsx (Role badge component for users)
- **`teams/`**: Team-related components
  - EditTeamDialog.tsx (Edit team dialog with member management)
- **`sprints/`**: Sprint-related components
  - SprintPlanningAI.tsx (new - intelligent sprint planning)
- **`defects/`**: Defect-related components
- **`notifications/`**: Notification components
  - NotificationBell.tsx (Notification bell with badge count)
  - NotificationDropdown.tsx (Notification dropdown menu)
- **`comments/`**: Comment components
  - CommentSection.tsx (Comment thread display)
- **`milestones/`**: Milestone-related components (8 components)
  - CreateMilestoneDialog.tsx
  - EditMilestoneDialog.tsx
  - CompleteMilestoneDialog.tsx
  - MilestoneCard.tsx
  - MilestonesList.tsx
  - MilestoneStatistics.tsx
  - MilestoneTimeline.tsx
  - NextMilestone.tsx
- **`releases/`**: Release-related components (18 components)
  - CreateReleaseDialog.tsx
  - EditReleaseDialog.tsx
  - DeployReleaseDialog.tsx
  - ReleaseCard.tsx
  - ReleasesList.tsx
  - ReleaseStatistics.tsx
  - ReleaseTimeline.tsx
  - ReleaseNotesEditor.tsx
  - ReleaseNotesViewer.tsx
  - QualityGatesPanel.tsx
  - QualityGateWidget.tsx
  - QualityTrendChart.tsx
  - ReleaseHealthDashboard.tsx
  - BlockedReleasesWidget.tsx
  - NextReleaseWidget.tsx
  - PendingApprovalsWidget.tsx
  - DeploymentFrequencyChart.tsx
  - SprintSelectorDialog.tsx
- **`tasks/`**: Task-related components (includes dependency management)
  - TaskDependenciesList.tsx
  - AddDependencyDialog.tsx
  - DependencyGraph.tsx
  - BlockedBadge.tsx
  - CommentForm.tsx (Comment input with mention autocomplete)
  - CommentItem.tsx (Individual comment display)
- **`attachments/`**: Attachment components
  - AttachmentUpload.tsx (Drag-and-drop file upload)
  - AttachmentList.tsx (List of attachments with download/delete)
- **`search/`**: Search components
- **`dashboard/`**: Dashboard widgets
- **`FeatureFlag/`**: Feature flag conditional rendering components

#### 3.2.3 `/src/pages/`

Page components (route-level components):

- **`auth/`**: Login, Register, AcceptInvite, ForgotPassword, ResetPassword (5 pages)
- **`admin/`**: AdminDashboard, AdminUsers, AdminPermissions, AdminSettings, AdminAuditLogs, AdminSystemHealth, AIGovernance, AdminAIQuota, AdminOrganizations, AdminOrganizationDetail, AdminMyOrganization, AdminOrganizationMembers, AdminMemberAIQuotas, AdminMemberPermissions (14 pages)
- **`superadmin/`**: SuperAdminOrganizationAIQuota, SuperAdminOrganizationPermissions (2 pages)
- **Feature Pages**: Dashboard, Projects, ProjectDetail, ProjectMembers, Tasks, Sprints, Teams, Metrics, Insights, Agents, Backlog, Defects, Profile, Users, QuotaDetails, ReleaseDetailPage, ReleaseHealthDashboard, Terms, NotFound, Index (20 pages)
- **Total**: 41 pages (excluding test files)

#### 3.2.4 `/src/contexts/`

React Context providers:

- **`AuthContext`**: Authentication state and methods
- **`ThemeContext`**: Theme (light/dark) management
- **`ProjectContext`**: Current project context
- **`FeatureFlagsContext`**: Feature flags state and management
- **`LanguageContext`**: Language/i18n state and language switching
- **`PermissionContext`**: Permission checking and management

---

## 4. Technology Stack

### 4.1 Core Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| **React** | 18.3.1 | UI library |
| **TypeScript** | 5.8.3 | Type safety (strict mode enabled) |
| **Vite** | 5.4.19 | Build tool and dev server |
| **React Router** | 6.30.1 | Client-side routing |
| **TanStack Query** | 5.83.0 | Data fetching and caching |

### 4.2 UI Framework

| Library | Purpose |
|---------|---------|
| **shadcn/ui** | Component library (Radix UI primitives) |
| **Tailwind CSS** | Utility-first CSS framework |
| **Radix UI** | Accessible component primitives |
| **Lucide React** | Icon library |
| **Recharts** | Chart library for metrics |

### 4.3 Form Management

| Library | Purpose |
|---------|---------|
| **React Hook Form** | Form state management |
| **Zod** | Schema validation |
| **@hookform/resolvers** | Zod integration for RHF |

### 4.4 Development Tools

| Tool | Purpose |
|------|---------|
| **Vitest** | Unit testing framework |
| **Testing Library** | React component testing |
| **MSW** | API mocking for tests |
| **ESLint** | Code linting |
| **TypeScript ESLint** | TypeScript-specific linting |

### 4.5 Monitoring & Error Tracking

| Tool | Purpose |
|------|---------|
| **Sentry** | Error tracking and performance monitoring |
| **Sentry Replay** | Session replay for debugging |

### 4.6 Additional Libraries

- **date-fns**: Date manipulation and formatting
- **sweetalert2**: Beautiful, responsive alert dialogs and toast notifications
- **sweetalert2-react-content**: React integration for SweetAlert2
- **next-themes**: Theme management
- **cmdk**: Command palette component
- **embla-carousel-react**: Carousel component
- **react-resizable-panels**: Resizable panel layouts
- **react-beautiful-dnd**: Drag-and-drop functionality for Kanban boards

---

## 5. State Management

### 5.1 Context API

#### 5.1.1 AuthContext

Manages authentication state and user information:

```typescript
interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isAdmin: boolean;
  login: (data: LoginRequest) => Promise<User>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
}
```

**Usage:**
```typescript
const { user, isAuthenticated, login, logout } = useAuth();
```

**Features:**
- Automatic user fetch on mount
- Cookie-based authentication (httpOnly cookies)
- Sentry user context integration
- Role-based access helpers

#### 5.1.2 ThemeContext

Manages theme (light/dark mode):

```typescript
interface ThemeContextType {
  theme: 'light' | 'dark' | 'system';
  setTheme: (theme: 'light' | 'dark' | 'system') => void;
  resolvedTheme: 'light' | 'dark';
}
```

**Usage:**
```typescript
const { theme, setTheme, resolvedTheme } = useTheme();
```

**Features:**
- System theme detection
- Persistent theme preference (localStorage)
- Smooth theme transitions

#### 5.1.3 ProjectContext

Manages current project context (if needed for cross-component state).

#### 5.1.4 FeatureFlagsContext

Manages feature flags state and provides global access to feature flag values:

```typescript
interface FeatureFlagsContextType {
  flags: Record<string, boolean>;
  isLoading: boolean;
  error: Error | null;
  refresh: () => Promise<void>;
  isEnabled: (flagName: string) => boolean;
}
```

**Usage:**
```typescript
const { flags, isEnabled, isLoading, refresh } = useFeatureFlags();

if (isEnabled('EnableAIInsights')) {
  // Show AI features
}
```

**Features:**
- Fetches all flags on mount for current organization
- Auto-refreshes every 5 minutes
- Provides flags via context for global access
- Handles loading and error states
- Supports organization-specific flags

### 5.2 TanStack Query (React Query)

#### 5.2.1 Configuration

```typescript
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // 5 minutes
      retry: 1,
    },
  },
});
```

#### 5.2.2 Query Patterns

**Basic Query:**
```typescript
const { data, isLoading, error } = useQuery({
  queryKey: ['projects', projectId],
  queryFn: () => projectsApi.getById(projectId),
  enabled: !!projectId,
});
```

**Authentication-Aware Query (Recommended Pattern):**
```typescript
const { isAuthenticated, isLoading: isAuthLoading } = useAuth();

const { data } = useQuery({
  queryKey: ['notifications'],
  queryFn: () => notificationsApi.getAll(),
  enabled: isAuthenticated && !isAuthLoading, // Only fetch when authenticated
  refetchOnWindowFocus: isAuthenticated && !isAuthLoading, // Prevent 401 on window focus
  retry: (failureCount, error) => {
    // Don't retry on 401 errors - API client handles token refresh
    if (error instanceof Error && (error.message.includes('Unauthorized') || error.message.includes('401'))) {
      return false;
    }
    return failureCount < 3;
  },
});
```

**Mutation:**
```typescript
import { showToast } from '@/lib/sweetalert';

const mutation = useMutation({
  mutationFn: (data: CreateProjectRequest) => projectsApi.create(data),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['projects'] });
    showToast('Project created successfully', 'success');
  },
});
```

**Best Practices:**
- Always use `enabled` option for authentication-dependent queries
- Set `refetchOnWindowFocus` conditionally based on authentication status to prevent 401 errors
- Implement proper retry logic that excludes 401 errors (API client handles token refresh)
- Queries automatically disable when `isAuthenticated` becomes `false` (via `auth:failed` event)

#### 5.2.3 Query Keys

Organized by feature:
- `['projects']`: All projects
- `['projects', id]`: Single project
- `['tasks', projectId]`: Tasks by project
- `['user-role', projectId]`: User role in project
- `['notifications']`: User notifications
- `['metrics', projectId]`: Project metrics

### 5.3 Local State

For component-specific state, use React hooks:
- `useState`: Simple state
- `useReducer`: Complex state logic
- `useRef`: Mutable references
- `useMemo`: Computed values
- `useCallback`: Memoized callbacks

---

## 6. Routing

### 6.1 Route Structure

```typescript
<Routes>
  {/* Public routes */}
  <Route path="/login" element={<Login />} />
  <Route path="/register" element={<Register />} />
  <Route path="/forgot-password" element={<ForgotPassword />} />
  <Route path="/reset-password/:token" element={<ResetPassword />} />
  <Route path="/invite/accept/:token" element={<AcceptInvite />} />
  <Route path="/terms" element={<Terms />} />

  {/* Protected routes */}
  <Route element={<MainLayout />}>
    <Route path="/" element={<Navigate to="/dashboard" replace />} />
    <Route path="/dashboard" element={<Dashboard />} />
    <Route path="/projects" element={<Projects />} />
    <Route path="/projects/:projectId/releases/:releaseId" element={<ReleaseDetailPage />} />
    <Route path="/projects/:projectId/releases/health" element={<ReleaseHealthDashboard />} />
    <Route path="/projects/:id" element={<ProjectDetail />} />
    <Route path="/projects/:id/members" element={<ProjectMembers />} />
    <Route path="/tasks" element={<Tasks />} />
    <Route path="/sprints" element={<Sprints />} />
    <Route path="/backlog" element={<Backlog />} />
    <Route path="/defects" element={<Defects />} />
    <Route path="/profile" element={<Profile />} />
    <Route path="/teams" element={<Teams />} />
    <Route path="/users" element={<Users />} />
    <Route path="/metrics" element={<Metrics />} />
    <Route path="/insights" element={<Insights />} />
    <Route path="/agents" element={<Agents />} />
    <Route path="/settings/ai-quota" element={<QuotaDetails />} />
  </Route>

  {/* Admin routes */}
  <Route path="/admin" element={<RequireAdminGuard><AdminLayout /></RequireAdminGuard>}>
    <Route index element={<Navigate to="/admin/dashboard" replace />} />
    <Route path="dashboard" element={<AdminDashboard />} />
    <Route path="users" element={<AdminUsers />} />
    <Route path="permissions" element={<AdminPermissions />} />
    <Route path="settings" element={<AdminSettings />} />
    <Route path="audit-logs" element={<AdminAuditLogs />} />
    <Route path="system-health" element={<AdminSystemHealth />} />
    <Route path="ai-governance" element={<AIGovernance />} />
    {/* SuperAdmin only routes */}
    <Route path="organizations/:orgId/permissions" element={<RequireSuperAdminGuard><SuperAdminOrganizationPermissions /></RequireSuperAdminGuard>} />
    {/* Admin own-org routes */}
    <Route path="permissions/members" element={<AdminMemberPermissions />} />
  </Route>

  {/* 404 */}
  <Route path="*" element={<NotFound />} />
</Routes>
```

### 6.2 Route Guards

#### 6.2.1 MainLayout

- Checks authentication status
- Redirects to `/login` if not authenticated
- Shows loading skeleton during auth check
- Provides sidebar and header layout

#### 6.2.2 RequireAdminGuard

- Checks if user has Admin role
- Redirects to dashboard if not admin
- Used for admin-only routes

### 6.3 Navigation

**Programmatic Navigation:**
```typescript
import { useNavigate } from 'react-router-dom';

const navigate = useNavigate();
navigate('/projects/123');
```

**Link Component:**
```typescript
import { Link } from 'react-router-dom';

<Link to="/projects/123">View Project</Link>
```

**NavLink Component:**
Custom `NavLink` component with active state styling.

---

## 7. API Integration

### 7.1 API Client

#### 7.1.1 Base Client (`client.ts`)

```typescript
class ApiClient {
  private async request<T>(endpoint: string, options: RequestInit): Promise<T> {
    // Handles:
    // - Cookie-based authentication (credentials: 'include')
    // - API versioning (/api/v1)
    // - Error handling (401, 429, etc.)
    // - Response parsing
  }
  
  get<T>(endpoint: string): Promise<T>
  post<T>(endpoint: string, data?: unknown): Promise<T>
  put<T>(endpoint: string, data?: unknown): Promise<T>
  patch<T>(endpoint: string, data?: unknown): Promise<T>
  delete<T>(endpoint: string): Promise<T>
}
```

**Features:**
- Automatic API versioning (`/api/v1/...` for standard routes)
- Admin routes excluded from versioning (`/api/admin/...` remain unchanged)
- Cookie-based authentication (httpOnly cookies)
- Automatic token refresh on 401 errors
- Automatic redirect to login if refresh fails
- Rate limit handling (429 errors with retry-after parsing)
- Comprehensive error parsing (field-level validation errors)
- User-friendly error messages based on HTTP status codes
- Global error toast notifications (Sonner)
- Sentry error logging for server errors (5xx)
- CORS credentials support
- ETag caching support for GET requests (304 Not Modified)
- Request/response normalization (handles both PascalCase and camelCase)

#### 7.1.2 API Modules

Each feature has its own API module:

**Example: `projects.ts`**
```typescript
export const projectsApi = {
  getAll: (page = 1, pageSize = 20) => 
    apiClient.get<PagedResponse<Project>>(`/Projects?page=${page}&pageSize=${pageSize}`),
  
  getById: (id: number) => 
    apiClient.get<Project>(`/Projects/${id}`),
  
  create: (data: CreateProjectRequest) => 
    apiClient.post<Project>('/Projects', data),
  
  update: (id: number, data: UpdateProjectRequest) => 
    apiClient.put<Project>(`/Projects/${id}`, data),
  
  archive: (id: number) => 
    apiClient.delete(`/Projects/${id}`),
  
  deletePermanent: (id: number) => 
    apiClient.delete(`/Projects/${id}/permanent`),
  
  getMembers: (id: number) => 
    apiClient.get<ProjectMember[]>(`/Projects/${id}/members`),
  
  inviteMember: (id: number, data: { email: string; role: ProjectRole }) => 
    apiClient.post(`/Projects/${id}/members`, data),
  
  updateMemberRole: (projectId: number, userId: number, role: ProjectRole) => 
    apiClient.put(`/Projects/${projectId}/members/${userId}/role`, { NewRole: role }),
  
  removeMember: (projectId: number, userId: number) => 
    apiClient.delete(`/Projects/${projectId}/members/${userId}`),
  
  assignTeam: (projectId: number, data: { teamId: number; defaultRole?: ProjectRole; memberRoleOverrides?: Record<number, ProjectRole> }) => 
    apiClient.post(`/Projects/${projectId}/assign-team`, data),
  
  getAssignedTeams: (projectId: number) => 
    apiClient.get(`/Projects/${projectId}/assigned-teams`),
};
```

**Note:** Project permissions are accessed via `permissionsApi.getProjectPermissions(projectId)`, not `projectsApi`.

**Example: `memberService.ts`**
```typescript
export const memberService = {
  getMembers: (projectId: number) => 
    apiClient.get<ProjectMember[]>(`/Projects/${projectId}/members`),
  
  inviteMember: (projectId: number, data: InviteMemberRequest) => 
    apiClient.post(`/Projects/${projectId}/members`, data),
  
  changeRole: (projectId: number, userId: number, data: ChangeRoleRequest) => 
    apiClient.put(`/Projects/${projectId}/members/${userId}/role`, data),
  
  removeMember: (projectId: number, userId: number) => 
    apiClient.delete(`/Projects/${projectId}/members/${userId}`),
  
  getUserRole: (projectId: number) => 
    apiClient.get<ProjectRole | null>(`/Projects/${projectId}/my-role`),
};
```

**Example: `users.ts`**

```typescript
/**
 * Maps frontend sortField values to backend expected values.
 * Backend accepts: Username, Email, CreatedAt, LastLoginAt, Role, IsActive
 */
function mapSortFieldToBackend(frontendSortField: string): string | undefined {
  const mapping: Record<string, string> = {
    'name': 'CreatedAt', // Backend doesn't have a "name" field, use CreatedAt as default
    'email': 'Email',
    'role': 'Role',
    'createdAt': 'CreatedAt',
    'status': 'IsActive',
    // Also accept backend values directly
    'Username': 'Username',
    'Email': 'Email',
    'CreatedAt': 'CreatedAt',
    'LastLoginAt': 'LastLoginAt',
    'Role': 'Role',
    'IsActive': 'IsActive',
  };
  
  return mapping[frontendSortField];
}

export const usersApi = {
  getAllPaginated: async (
    page = 1,
    pageSize = 20,
    role?: string,
    isActive?: boolean,
    sortField?: string,
    sortDescending = false,
    searchTerm?: string
  ): Promise<PagedResponse<UserListDto>> => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
      sortDescending: sortDescending.toString(),
    });
    if (role) params.append('role', role);
    if (isActive !== undefined) params.append('isActive', isActive.toString());
    
    // Map frontend sortField values to backend expected values
    if (sortField) {
      const backendSortField = mapSortFieldToBackend(sortField);
      if (backendSortField) {
        params.append('sortField', backendSortField);
      }
    }
    
    if (searchTerm) params.append('searchTerm', searchTerm);
    return apiClient.get<PagedResponse<UserListDto>>(`/api/v1/Users?${params.toString()}`);
  },
  
  getUserProjects: (userId, page = 1, pageSize = 5) => 
    apiClient.get<PagedResponse<ProjectListDto>>(`/Users/${userId}/projects?page=${page}&pageSize=${pageSize}`),
  
  getUserActivity: (userId, limit = 10) => 
    apiClient.get<GetRecentActivityResponse>(`/Users/${userId}/activity?limit=${limit}`),
  
  // ... other methods
};
```

**Key Features:**
- **Tenant Isolation**: Uses `/api/v1/Users` endpoint which automatically filters by organization:
  - Admin users see only members of their organization
  - SuperAdmin users see all members from all organizations
- **Sort Field Mapping**: Converts frontend sort field names to backend expected values
- **Type Safety**: Returns `PagedResponse<UserListDto>` with proper TypeScript types

**Example: `settings.ts`**
```typescript
export const settingsApi = {
  getAll: (category?: string) => 
    apiClient.get<Record<string, string>>(`/Settings${category ? `?category=${category}` : ''}`),
  
  update: (key: string, value: string, category?: string) => 
    apiClient.put(`/Settings/${encodeURIComponent(key)}`, { value, category }),
  
  sendTestEmail: (email: string) => 
    apiClient.post<{ success: boolean; message: string }>('/Settings/test-email', { email }),
};
```

### 7.2 Authentication Flow

#### 7.2.1 Login Flow

1. User submits login form
2. `authApi.login()` called
3. Backend sets httpOnly cookies (`auth_token`, `refresh_token`)
4. `AuthContext` fetches user data via `authApi.getMe()`
5. User state updated in context
6. Redirect based on role (Admin → `/admin/dashboard`, User → `/dashboard`)

#### 7.2.2 Token Management

- **Access Token**: Stored in httpOnly cookie (15 min expiration)
- **Refresh Token**: Stored in httpOnly cookie (7 days expiration)
- **Automatic Refresh**: Frontend API client automatically attempts token refresh on 401 errors
  - If refresh succeeds, the original request is retried
  - If refresh fails, user is redirected to login page
  - Prevents infinite redirect loops with `isRefreshing` and `isRedirecting` flags
- **Logout**: Clears cookies via `/api/v1/Auth/logout`

### 7.3 Error Handling

#### 7.3.1 Global Error Handling

The API client (`client.ts`) implements comprehensive error handling with user-friendly messages and automatic error logging:

**User-Friendly Error Messages:**
- `401`: "Session expired. Please log in again."
- `403`: "You don't have permission for this action."
- `429`: "Too many requests. Please try again later."
- `500`: "Server error. Please contact support."
- `502/503/504`: "Service temporarily unavailable. Please try again later."

**Error Handling Flow:**
1. Extract error message from response (prioritizes field-level validation errors)
2. Map to user-friendly message based on HTTP status code
3. Show toast notification (except for 401 which redirects to login)
4. Log to Sentry for server errors (5xx) if configured
5. Throw error for component-level handling

**Toast Notifications:**
- Global error toasts using Sonner toast system
- Automatic toast display for client errors (4xx) and server errors (5xx)
- 401 errors redirect to login without toast (prevents duplicate messages)
- 403 and 429 errors show specific toast messages
- Server errors (5xx) show toast and log to Sentry

**Sentry Integration:**
- Automatic error logging for server errors (5xx)
- Dynamic import to avoid bundling Sentry if not configured
- Context information included (endpoint, status, error data)
- Only logs when `VITE_SENTRY_DSN` is configured

**Example Error Handling:**
```typescript
// In component
try {
  await projectsApi.create(data);
} catch (error) {
  // Error already handled by API client:
  // - Toast notification shown
  // - Sentry logged (if 5xx)
  // - User-friendly message displayed
  // Component can handle specific cases if needed
}
```

#### 7.3.2 Legacy Error Handling

#### 7.3.1 Global Error Handling

The API client (`client.ts`) implements comprehensive error handling with user-friendly messages and automatic error logging:

**User-Friendly Error Messages:**
- `401`: "Session expired. Please log in again."
- `403`: "You don't have permission for this action."
- `429`: "Too many requests. Please try again later."
- `500`: "Server error. Please contact support."
- `502/503/504`: "Service temporarily unavailable. Please try again later."

**Error Handling Flow:**
1. Extract error message from response (prioritizes field-level validation errors)
2. Map to user-friendly message based on HTTP status code
3. Show toast notification (except for 401 which redirects to login)
4. Log to Sentry for server errors (5xx) if configured
5. Throw error for component-level handling

**Toast Notifications:**
- Global error toasts using Sonner toast system
- Automatic toast display for client errors (4xx) and server errors (5xx)
- 401 errors redirect to login without toast (prevents duplicate messages)
- 403 and 429 errors show specific toast messages
- Server errors (5xx) show toast and log to Sentry

**Sentry Integration:**
- Automatic error logging for server errors (5xx)
- Dynamic import to avoid bundling Sentry if not configured
- Context information included (endpoint, status, error data)
- Only logs when `VITE_SENTRY_DSN` is configured

**401 Unauthorized Handling:**
- Attempts automatic token refresh before redirecting
- If refresh succeeds, retries the original request
- If refresh fails, redirects to login page
- Prevents infinite loops with refresh flags
- No toast notification (prevents duplicate messages on auth pages)

**429 Too Many Requests:**
- Parses `Retry-After` header (seconds or HTTP-date)
- Also checks response body for `retryAfter` field
- Shows user-friendly retry message with wait time
- Displays toast notification with retry information

**400 Bad Request:**
- Extracts field-level validation errors from `errors` object
- Prioritizes specific field messages over generic errors
- Supports both array and object error formats
- Shows toast notification with validation message

**500 Server Error:**
- Shows user-friendly message: "Server error. Please contact support."
- Logs error to Sentry with context information
- Displays toast notification

**204 No Content:**
- Returns empty object for successful operations without response body

#### 7.3.2 Error Display

- **Toast Notifications**: Using Sonner for global error toasts (automatic)
- **Alert Dialogs**: Using SweetAlert2 for user feedback (success, error, warning, info)
- **Form Errors**: Field-level validation errors
- **Error Boundaries**: Sentry ErrorBoundary for unhandled errors

---

## 8. Components

### 8.1 UI Components (shadcn/ui)

Base components from shadcn/ui library:

#### 8.1.1 Form Components

- **Button**: Primary, secondary, destructive variants
- **Input**: Text input with validation states
- **Textarea**: Multi-line text input
- **Select**: Dropdown selection
- **Checkbox**: Boolean input
- **Radio Group**: Single selection from options
- **Switch**: Toggle switch
- **Slider**: Range input
- **Calendar**: Date picker
- **Form**: React Hook Form integration

#### 8.1.2 Layout Components

- **Card**: Container with header, content, footer
- **Sheet**: Side panel drawer
- **Dialog**: Modal dialog
- **Drawer**: Bottom drawer (mobile)
- **Tabs**: Tabbed interface
- **Accordion**: Collapsible sections
- **Separator**: Visual divider
- **Scroll Area**: Custom scrollbar

#### 8.1.3 Feedback Components

- **SweetAlert2**: Alert dialogs and toast notifications (success, error, warning, info, confirm, prompt)
- **Alert**: Alert messages (shadcn/ui)
- **Progress**: Progress indicator
- **Skeleton**: Loading placeholder
- **Badge**: Status badges

#### 8.1.4 Navigation Components

- **Sidebar**: Collapsible sidebar
- **Navigation Menu**: Dropdown navigation
- **Breadcrumb**: Breadcrumb navigation
- **Pagination**: Page navigation

#### 8.1.5 Data Display

- **Table**: Data table
- **Avatar**: User avatar
- **Tooltip**: Hover tooltip
- **Popover**: Popover content
- **Hover Card**: Hover card
- **Command**: Command palette

### 8.2 Feature Components

#### 8.2.1 Project Components

- **ProjectCard**: Project card display
- **EditProjectDialog**: Edit project modal
- **DeleteProjectDialog**: Delete confirmation
- **AddMemberDialog**: Add member to project
- **InviteMemberModal**: Invite member modal
- **ProjectMembersModal**: Members list modal
- **MemberCard**: Member card display
- **RoleBadge**: Role badge display
- **ProjectTimeline**: Project timeline view
- **TeamMembersList**: Team members list
- **ProjectCard**: Project card display component
- **AssignTeamModal**: Modal for assigning teams to projects

#### 8.2.2 Task Components

- **CreateTaskDialog**: Create task modal
- **TaskDetailSheet**: Task detail side panel
- **TaskListView**: List view of tasks
- **TaskTimelineView**: Timeline view of tasks
- **TaskBoard**: Kanban-style board with drag-and-drop functionality
  - Three columns: Todo, In Progress, Done
  - Drag-and-drop between columns using react-beautiful-dnd
  - Task cards with priority indicators, assignee avatars, story points, and due dates
  - Optimistic updates with automatic rollback on error
  - Mobile-responsive (drag disabled on mobile)
  - Loading states and empty states
- **TaskFilters**: Task filtering controls
- **StatusBadge**: Reusable badge component for task status display
  - Supports all task statuses (Todo, InProgress, Done, Blocked)
  - Multiple size variants (sm, md, lg)
  - Multiple visual variants (default, outline, dot)
  - Icon support with toggle
  - Dark mode support
  - Accessible (role="status", aria-label)
  - Memoized for performance
  - Utility functions: `getStatusColor()`, `getStatusLabel()`, `getStatusIcon()`
- **AITaskImproverDialog**: AI task improvement (used in CreateTaskDialog)
- **TaskImproverDialog**: AI task improvement dialog for existing tasks (new - created but not yet integrated in TaskDetailSheet)
- **DependencyAnalyzerPanel**: Task dependency analysis with circular dependency detection and critical path identification (new - created but not yet integrated)

#### 8.2.3 Sprint Components

- **StartSprintDialog**: Start sprint modal
- **CompleteSprintDialog**: Complete sprint modal
- **AddTasksToSprintDialog**: Add tasks to sprint
- **SprintPlanningAI**: Intelligent sprint planning with AI suggestions (new - created but not yet integrated)

#### 8.2.4 Defect Components

- **CreateDefectDialog**: Create defect modal
- **DefectDetailSheet**: Defect detail side panel

#### 8.2.5 Agent Components

- **ProjectInsightPanel**: AI project insights
- **ProjectAnalysisPanel**: Comprehensive project analysis with health status, insights, risks, and recommendations (new - created but not yet integrated)
- **RiskDetectionPanel**: Risk detection results
- **RiskDetectionDashboard**: Interactive risk detection dashboard with severity analysis and mitigation suggestions (new - created but not yet integrated)
- **SprintPlanningAssistant**: Sprint planning assistant
- **SprintPlanningAI**: Intelligent sprint planning with capacity analysis and task suggestions (new - created but not yet integrated)
- **AgentResultsDisplay**: Wrapper component that displays structured agent results
  - Routes to type-specific result components based on agent type
  - Handles JSON parsing and error fallback
  - Loading skeleton and error states
- **ProductAgentResults**: Displays prioritized items in a sortable table
  - Columns: Priority, Task, Rationale, Confidence Score
  - Drag-and-drop reordering support
  - Badge color-coding for confidence scores
- **QAAgentResults**: Displays defect patterns with visualizations
  - Pattern cards with severity badges (critical=red, high=orange, medium=yellow, low=gray)
  - Bar chart for pattern frequencies
  - Tooltips with recommendations
- **BusinessAgentResults**: Displays value metrics with trend indicators
  - Metric cards with progress bars (currentValue/targetValue)
  - Trend icons (↑ up, ↓ down, → stable)
  - Line chart for metric trends
- **ManagerAgentResults**: Displays executive summary
  - Key decisions checklist
  - Highlights section
  - Recommendations list
- **DeliveryAgentResults**: Displays delivery analysis
  - Milestone timeline with status indicators
  - Risk cards with severity and mitigation
  - Action items with priority badges

#### 8.2.6 AI Governance Components

- **QuotaStatusWidget**: Displays AI quota status with progress bars
  - Tier badge (Free/Pro/Enterprise)
  - Progress bars for Requests, Tokens, Decisions with color-coding (<50% green, 50-80% yellow, >80% red)
  - Quota reset date
  - Upgrade button for Free/Pro tiers
  - Compact mode for sidebar display
  - Auto-refresh every 60 seconds
- **QuotaAlertBanner**: Prominent alert banner for quota thresholds
  - Displays when isAlertThreshold=true or isDisabled=true
  - Messages for 80%, 100%, and Disabled states
  - Directs users to contact administrators for quota increases
  - CTA button to view details
- **QuotaExceededAlert**: Alert component for quota exceeded state
  - Uses global quota error state from API client
  - Displays quota details and directs users to contact administrators
- **AIDisabledAlert**: Alert component for AI disabled state
  - Uses global AI disabled error state from API client
  - Displays reason and support contact

#### 8.2.7 User Components

- **UserCard**: User card display component
  - Displays user avatar, name, email, role, status
  - Shows project count, join date, last login
  - Supports click navigation
  - Optional action buttons (Edit, Delete, View)
  - Inactive user indicator (grayscale filter)
- **RoleBadge**: Role badge component for users
  - Displays Admin/User roles with icons
  - Multiple size variants (sm, md, lg)
  - Color-coded (Admin: red, User: blue)

#### 8.2.8 Comment Components

- **CommentSection**: Comment thread display component
  - Displays all comments for an entity (task, project, etc.)
  - Supports nested replies (threading)
  - Shows comment author, timestamp, edit status
  - Edit/delete actions for comment authors
  - Loading states and empty states
- **CommentForm**: Comment input form with mention support
  - Rich text input with @username mention autocomplete
  - Parses mentions and highlights them
  - Supports replying to parent comments
  - Validation and error handling
- **CommentItem**: Individual comment display
  - Shows comment content with mention highlighting
  - Author avatar and name
  - Timestamp and edit indicator
  - Reply button and nested replies display
  - Edit/delete actions (author only)

#### 8.2.9 Attachment Components

- **AttachmentUpload**: Drag-and-drop file upload component
  - Drag-and-drop zone with visual feedback
  - File validation (size, type, extension)
  - Progress indicator during upload
  - Multiple file support
  - Error handling with user-friendly messages
- **AttachmentList**: List of attachments with actions
  - Displays file name, size, uploader, upload date
  - Download button for each attachment
  - Delete button (uploader or admin only)
  - File type icons
  - Empty state when no attachments

#### 8.2.10 Notification Components

- **NotificationBell**: Notification bell with badge count
  - Badge showing unread notification count
  - Click to open notification dropdown
  - Real-time updates via polling or WebSocket
  - Visual indicator for new notifications
- **NotificationDropdown**: Notification dropdown menu
  - List of recent notifications
  - Mark as read functionality
  - Link to notification source
  - Empty state when no notifications

#### 8.2.11 Admin Components

- **InviteUserDialog**: Invite organization user modal
  - Fields: Email, First Name, Last Name, Role (Admin/User dropdown)
  - Calls `POST /api/admin/users/invite`
  - Displays invitation link with copy functionality on success
  - Refreshes user list after successful invitation
- **EditUserDialog**: Edit user modal
- **DeleteUserDialog**: Delete user confirmation
- **UserDetailDialog**: User detail modal with tabs
  - **Overview Tab**: User information, role, status, organization details
  - **Projects Tab**: Paginated list of user's projects (fetches from `GET /api/v1/Users/{id}/projects`)
  - **Activity History Tab**: Recent user activities (fetches from `GET /api/v1/Users/{id}/activity`)
  - Uses React Query for data fetching with loading states

#### 8.2.12 Layout Components

- **MainLayout**: Main application layout
- **AdminLayout**: Admin section layout
- **AppSidebar**: Main sidebar navigation
  - Shows "Admin Dashboard" button in footer for admin users
  - Displays app version and build date in footer
- **AdminSidebar**: Admin sidebar navigation
- **Header**: Top header with search and notifications
- **ThemeToggle**: Theme switcher

#### 8.2.13 Utility Components

- **ErrorFallback**: Error boundary fallback
- **GlobalSearchModal**: Global search modal (Ctrl/Cmd+K)
- **NotificationDropdown**: Notification dropdown
- **RequireAdminGuard**: Admin route guard
- **PermissionGuard**: Permission-based route/component guard
- **FeatureFlag**: Feature flag conditional rendering component

#### 8.2.14 Auth Components

- **PasswordStrength**: Password strength indicator (weak/medium/strong)
  - Located in `src/components/ui/password-strength.tsx`
  - Displays strength bar and label
  - Evaluates password criteria (length, uppercase, lowercase, number, special char)
- **PasswordStrengthIndicator**: Alternative password strength indicator (4-bar display)
  - Located in `src/components/auth/PasswordStrengthIndicator.tsx`
  - Shows 4 colored bars (levels 1-4)
  - Labels: Faible (Weak), Moyen (Medium), Bon (Good), Excellent
  - Calculates strength based on length, character variety, and complexity

#### 8.2.2 Authentication Components

- **Logo**: Reusable IntelliPM logo component
  - Located in `src/components/auth/Logo.tsx`
  - Props: `variant?: 'light' | 'dark'`, `size?: 'sm' | 'md' | 'lg'`
  - Light variant: White text with backdrop blur icon background
  - Dark variant: Foreground text with gradient primary icon background
  - Responsive sizing: sm (8x8 icon), md (12x12 icon), lg (16x16 icon)
  - Displays "IntelliPM" text with "Intelligent Project Management" tagline
  - Used in login page (light variant on gradient, dark variant on mobile)

- **GeometricShapes**: Animated decorative shapes for visual backgrounds
  - Located in `src/components/auth/GeometricShapes.tsx`
  - Floating hexagons, circles, triangles with opacity variations
  - Animated with float, float-delayed, and pulse-glow animations
  - Grid pattern overlay for texture
  - Used in login page left panel for visual interest

- **LoginForm**: Standalone login form component
  - Located in `src/components/auth/LoginForm.tsx`
  - Full authentication logic integration with `useAuth` hook
  - Features:
    - Username/email input with validation
    - Password input with visibility toggle
    - Remember me checkbox
    - Forgot password link
    - Loading states with spinner
    - Error handling with SweetAlert2
    - Role-based redirections (Admin → /admin/dashboard, User → /dashboard)
  - Animated form fields with staggered fade-in-up effects
  - Gradient primary button with hover effects
  - Preserves all existing authentication functionality

### 8.3 Component Patterns

#### 8.3.1 Controlled Components

```typescript
const [value, setValue] = useState('');
<input value={value} onChange={(e) => setValue(e.target.value)} />
```

#### 8.3.2 Uncontrolled Components

```typescript
const inputRef = useRef<HTMLInputElement>(null);
<input ref={inputRef} />
```

#### 8.3.3 Compound Components

```typescript
<Card>
  <CardHeader>
    <CardTitle>Title</CardTitle>
  </CardHeader>
  <CardContent>Content</CardContent>
</Card>
```

---

## 9. Pages

### 9.1 Authentication Pages

#### 9.1.1 Login (`/login`)

**Modern Split-Screen Design** (v2.19.0)

- **Layout**: Split-screen design with gradient panel (left) and login form (right)
  - Desktop: 50/50 split (lg) or 55/45 split (xl)
  - Mobile: Stacked layout with logo above form
- **Left Panel** (Desktop only):
  - Gradient dark background with animated geometric shapes
  - Large IntelliPM logo (light variant)
  - Tagline: "Gérez vos projets avec intelligence. Simplifiez. Automatisez. Réussissez."
  - Bottom gradient fade effect
- **Right Panel**:
  - Mobile logo (dark variant) - visible on mobile only
  - Login card with rounded corners and shadow
  - Welcome message: "Bienvenue" with subtitle
  - LoginForm component with full authentication logic
  - Footer with copyright notice
- **Features**:
  - Username/email input with validation
  - Password input with visibility toggle
  - "Remember me" checkbox
  - "Forgot password" link to `/forgot-password`
  - Loading states with spinner and "Connexion en cours..." message
  - Error handling with SweetAlert2
  - Role-based redirections:
    - Admin → `/admin/dashboard`
    - User → `/dashboard`
  - Automatic redirect if already authenticated
- **Animations**:
  - Fade-in animations for logo and elements
  - Staggered fade-in-up for form fields
  - Slide-in-right for login card
  - Floating animations for geometric shapes
- **Components Used**:
  - `Logo` (light variant on left, dark variant on mobile)
  - `GeometricShapes` (left panel background)
  - `LoginForm` (standalone form component)

#### 9.1.2 Register (`/register`)

- **DISABLED** - Public registration is disabled
- Displays message: "Les inscriptions publiques sont désactivées pour des raisons de sécurité"
- Instructs users to contact administrator for an invitation
- Provides link back to login page

#### 9.1.3 Forgot Password (`/forgot-password`)

- Password reset request form
- Email input field
- Sends password reset email if user exists
- Link to login page

#### 9.1.4 Reset Password (`/reset-password/:token`)

- Password reset form with token validation
- New password and confirm password fields
- Password strength indicator
- Redirects to login on success

#### 9.1.5 Accept Invite (`/invite/accept/:token`)

- Validates invitation token from URL parameters
- Displays invitation details (email, organization name, role)
- Form fields:
  - Email (read-only, from invitation)
  - Username (user chooses, 3-50 chars, alphanumeric + underscore)
  - Password (min 8 chars, must contain uppercase, lowercase, number)
  - Confirm Password (must match password)
- Uses react-hook-form + zod for validation
- Password strength indicator (PasswordStrength component)
- On success: Creates user account, generates JWT tokens, redirects to dashboard
- On error: Displays appropriate error messages (token expired, username taken, etc.)

### 9.2 Main Application Pages

#### 9.2.1 Dashboard (`/dashboard`)

- Overview metrics
- Recent activity feed
- Quick actions
- Project summaries

#### 9.2.2 Projects (`/projects`)

- Project list/grid view
- Create project button
- Filter and search
- Project cards with key metrics

#### 9.2.3 Project Detail (`/projects/:id`)

- Project overview
- Key metrics
- Recent activity
- Quick actions
- Member list
- Sprints, tasks, and releases tabs

#### 9.2.4 Release Detail (`/projects/:projectId/releases/:releaseId`)

- Release information and status
- Sprint management
- Quality gates
- Release notes and changelog
- Deployment controls

#### 9.2.5 Release Health Dashboard (`/projects/:projectId/releases/health`)

- Release health metrics
- Quality trends
- Deployment frequency
- Blocked releases

#### 9.2.6 Project Members (`/projects/:id/members`)

- Member list
- Role management
- Invite members
- Remove members

#### 9.2.7 Tasks (`/tasks`)

- Task list view
- Task timeline view
- Filters (status, priority, assignee, project)
- Create task button
- Task detail sheet

#### 9.2.8 Sprints (`/sprints`)

- Sprint list
- Active sprint display
- Sprint planning
- Sprint completion

#### 9.2.9 Backlog (`/backlog`)

- Epic/Feature/Story hierarchy
- Create backlog items
- Story point estimation
- Backlog refinement

#### 9.2.10 Defects (`/defects`)

- Defect list
- Severity and status filters
- Create defect
- Defect detail view

#### 9.2.11 Teams (`/teams`)

- Team list
- Team capacity management
- Team member management

#### 9.2.12 Metrics (`/metrics`)

- Velocity charts
- Burndown charts
- Defect trends
- Task distribution
- Team performance

#### 9.2.13 Insights (`/insights`)

- AI-generated insights
- Risk detection results
- Recommendations
- Project health indicators

#### 9.2.14 Agents (`/agents`)

- AI agent interface with structured result displays
- Five AI agents: Product, QA, Business, Manager, Delivery
- AgentResultsDisplay component with type-specific result components
- Quota status widget and alert banner integration
- Automatic quota notifications (80% warning, 100% error)
- Project-scoped agent execution

#### 9.2.15 Users (`/users`)

- Read-only user list for non-admin users
- Grid layout with user cards
- Search by name or email
- Filter by role (Admin/User) and status (Active/Inactive)
- Pagination support
- Displays user avatars, roles, status, project count, join date, and last login

#### 9.2.16 Profile (`/profile`)

- User profile information
- Account settings
- Password change
- Notification preferences

#### 9.2.17 AI Quota Details (`/settings/ai-quota`)

- Detailed AI quota usage view
- Historical usage graphs (30 days)
- Breakdown by agent type
- Current quota status with usage percentages
- ✅ Route configured in App.tsx
- ⚠️ Note: Currently uses mock data for usage history and breakdown; backend endpoint integration pending

### 9.3 Admin Pages

#### 9.3.1 Admin Dashboard (`/admin/dashboard`)

- System overview with key metrics:
  - Total Users (with active/inactive breakdown)
  - Total Projects (with active projects count)
  - Organizations count
  - System Health status (CPU, Memory, Database status)
- **User Growth Chart**: Line chart showing user growth over last 6 months
- **Role Distribution Chart**: Pie chart showing Admin vs User distribution
- **Recent Activities**: List of last 10 system activities with user and timestamp
- Error handling for missing data (empty states for charts)
- Safe date formatting with error handling

#### 9.3.2 Admin Users (`/admin/users`)

- User list with pagination and search
- "Inviter un utilisateur" button opens InviteUserDialog
- Edit user (EditUserDialog)
- Delete user (DeleteUserDialog with confirmation)
- View user details (UserDetailDialog):
  - **Overview Tab**: User information, role, status, organization
  - **Projects Tab**: Paginated list of user's projects with project details
  - **Activity History Tab**: Recent user activities with timestamps
- Invite user (InviteUserDialog):
  - Form fields: Email, First Name, Last Name, Role (Admin/User)
  - Calls `POST /api/admin/users/invite`
  - Shows invitation link on success for manual sharing
  - Automatically refreshes user list after invitation
- **Last Login Column**: Displays user's last login timestamp or "Never" if never logged in

#### 9.3.3 Admin Permissions (`/admin/permissions`)

- Permission matrix
- Role-permission mapping
- Update permissions

#### 9.3.8 Admin Member Permissions (`/admin/permissions/members`)

- **Member Permissions Management**: Admin-only page for managing member roles and permissions within their organization
- **Member List**: Paginated table showing all organization members with their current roles and permissions
- **Search Functionality**: Search members by name or email
- **Edit Modal**: Dialog for updating member roles and permissions
  - Role selection (User/Admin only - SuperAdmin role cannot be assigned by Admin)
  - Permission selection filtered by organization policy
  - Only permissions allowed by organization policy are available
  - Permissions derived from role when role is changed
- **Policy Enforcement**: UI automatically filters available permissions based on organization permission policy
- **Tenant Isolation**: Admin can only manage members within their own organization

#### 9.3.9 Admin Organization Members (`/admin/organization/members`)

- **Organization Members Management**: Admin-only page for viewing and managing organization members
- **Tenant Isolation**: Properly implements tenant isolation based on user role:
  - **Admin**: Sees only members of their own organization (filtered automatically by backend)
  - **SuperAdmin**: Sees all members from all organizations
- **API Endpoint**: Uses `/api/v1/Users` endpoint which leverages `OrganizationScopingService` for automatic tenant isolation
- **Member List**: Paginated table showing:
  - Member name (with avatar initials)
  - Email address
  - Role badge (User, Admin, SuperAdmin with color coding)
  - Status badge (Active/Inactive)
  - Joined date (formatted as "MMM dd, yyyy")
  - Actions (role change button for non-SuperAdmin users)
- **Search Functionality**: Real-time search by name or email (debounced)
- **Role Management**: 
  - Change member role between User and Admin (SuperAdmin role cannot be changed by Admin)
  - Role change dialog with confirmation
  - Updates member role via `PUT /api/admin/organization/members/{userId}/global-role`
- **Pagination**: 20 items per page with navigation controls
- **Loading States**: Skeleton loaders during data fetch
- **Empty States**: "No members found" message when no results
- **Type Safety**: Uses `UserListDto` type from `@/api/users` API client
- **Field Mapping**: Correctly maps `user.globalRole` (from API) instead of `user.role`

**Technical Details:**
- Uses `usersApi.getAllPaginated()` which includes `mapSortFieldToBackend()` function
- Maps frontend sortField values (`name`, `email`, `role`, `createdAt`, `status`) to backend values (`CreatedAt`, `Email`, `Role`, `IsActive`)
- Fixed 400 Bad Request errors when sorting by `name` (now correctly maps to `CreatedAt`)

#### 9.3.4 Admin Settings (`/admin/settings`)

Complete settings management with four tabs:

- **General Tab**:
  - Application name configuration
  - Default timezone selection
  - Default language selection
  - Date format configuration
  - Project creation permissions (allowed roles)

- **Security Tab**:
  - Token expiration settings
  - Password policy configuration:
    - Minimum password length
    - Require uppercase letters
    - Require lowercase letters
    - Require digits
    - Require special characters
  - Maximum login attempts
  - Session duration
  - 2FA requirement toggle

- **Email Tab**:
  - SMTP host configuration
  - SMTP port
  - SMTP username
  - SMTP password (masked input)
  - SSL/TLS toggle
  - From email address
  - From name
    - Test email functionality:
      - Email input field (pre-filled with current user's email)
      - "Send Test Email" button
      - Success/error notifications using SweetAlert2

- **Feature Flags Tab**:
  - Feature flag management
  - Enable/disable features
  - Organization-specific flags

#### 9.3.5 Admin Audit Logs (`/admin/audit-logs`)

- View system audit logs
- Filter by user, action, date range
- Paginated list of audit entries

#### 9.3.6 Admin System Health (`/admin/system-health`)

- System health monitoring dashboard
- Database connectivity status
- Ollama LLM status
- Memory usage
- Health check endpoints status

#### 9.3.7 AI Governance (`/admin/ai-governance`)

- **Overview Tab**: AI usage statistics, quota status, decision summary
- **AI Decisions Tab**: Paginated list of AI decisions with filters
  - Filter by decision type, agent type, entity type, date range
  - View decision details (reasoning, confidence score, status)
  - Approve/reject decisions requiring human approval
- **Quotas Tab**: AI quota management across organizations
  - View all organization quotas
  - Update quota limits and tiers
  - Enable/disable AI for organizations (kill switch)
  - Export decisions to CSV

### 9.4 SuperAdmin Pages

#### 9.4.1 SuperAdmin Organization Permissions (`/admin/organizations/:orgId/permissions`)

- **Permission Policy Management**: SuperAdmin-only page for managing allowed permissions per organization
- **Organization Selection**: Accessible from organization detail page
- **Permission Checklist**: Complete checklist/matrix of all system permissions
  - Permissions grouped by category
  - Search functionality to filter permissions
  - Select All / Deselect All buttons
  - Visual count of selected permissions
- **Policy Activation**: Toggle switch to activate/deactivate policy
  - When inactive: all permissions allowed (default behavior)
  - When active: only selected permissions are allowed
- **Policy Information**: Display of policy creation and update timestamps
- **Save Policy**: Upsert operation (create if not exists, update if exists)
- **Default Behavior**: If no policy exists, all permissions are allowed

---

## 10. Styling

### 10.1 Tailwind CSS

#### 10.1.1 Configuration

Located in `tailwind.config.ts`:

- **Theme**: Custom color palette, spacing, typography
- **Dark Mode**: Class-based dark mode
- **Plugins**: `tailwindcss-animate` for animations
- **Content**: Scans `src/**/*.{ts,tsx}` for class usage

#### 10.1.2 Design System

**Colors:**
- Primary, Secondary, Destructive, Muted, Accent
- Semantic colors (border, input, ring, background, foreground)
- Sidebar-specific colors
- All colors support dark mode via CSS variables

**Spacing:**
- Standard Tailwind spacing scale
- Custom spacing for specific components

**Typography:**
- Font families: Inter (sans), Lora (serif), Space Mono (mono)
- Responsive typography scales

**Shadows:**
- Custom shadow scale (2xs to 2xl)

### 10.2 CSS Variables

Colors defined as CSS variables for theme switching:

```css
:root {
  --background: 0 0% 100%;
  --foreground: 222.2 84% 4.9%;
  --primary: 222.2 47.4% 11.2%;
  /* ... */
}

.dark {
  --background: 222.2 84% 4.9%;
  --foreground: 210 40% 98%;
  /* ... */
}
```

### 10.3 Component Styling

#### 10.3.1 Utility Classes

```typescript
<div className="flex items-center justify-between p-4 bg-card rounded-lg">
  <h2 className="text-lg font-semibold">Title</h2>
  <Button variant="outline">Action</Button>
</div>
```

#### 10.3.2 Responsive Design

```typescript
<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
  {/* Responsive grid */}
</div>
```

#### 10.3.3 Dark Mode

Automatic dark mode support via CSS variables:
```typescript
<div className="bg-background text-foreground">
  {/* Automatically adapts to theme */}
</div>
```

### 10.4 shadcn/ui Styling

Components use `cn()` utility for conditional classes:

```typescript
import { cn } from '@/lib/utils';

<div className={cn(
  "base-classes",
  condition && "conditional-classes",
  className // Allow override
)} />
```

---

## 11. Internationalization (i18n)

### 11.1 Overview

IntelliPM uses **react-i18next** for internationalization, providing support for multiple languages with dynamic language switching and backend synchronization.

**Supported Languages:**
- **English (en)** - Default language
- **Français (fr)** - French

**Key Features:**
- 18 namespaces organized by feature
- Dynamic language switching at runtime
- Backend synchronization of language preference
- Automatic browser language detection
- Locale-aware date and number formatting
- Translation validation script

### 11.2 Translation File Structure

Translation files are located in `public/locales/{lang}/{ns}.json`:

```
public/locales/
├── en/
│   ├── common.json
│   ├── auth.json
│   ├── projects.json
│   ├── tasks.json
│   └── ... (18 namespaces total)
└── fr/
    ├── common.json
    ├── auth.json
    ├── projects.json
    ├── tasks.json
    └── ... (18 namespaces total)
```

**Namespaces:**
- `common` - Common UI elements
- `auth` - Authentication
- `projects` - Project management
- `tasks` - Task management
- `admin` - Administration
- `navigation` - Navigation menu
- `notifications` - Notifications
- `errors` - Error messages
- `dashboard` - Dashboard
- `sprints` - Sprint management
- `teams` - Team management
- `backlog` - Backlog management
- `defects` - Defect tracking
- `metrics` - Metrics and analytics
- `insights` - Project insights
- `agents` - AI agents
- `milestones` - Milestone tracking
- `releases` - Release management

### 11.3 LanguageContext API

The `LanguageContext` provides language state and management:

```typescript
import { useLanguage } from '@/contexts/LanguageContext';

function MyComponent() {
  const { 
    language,           // Current language code ('en', 'fr')
    changeLanguage,     // Function to change language
    availableLanguages, // Array of available languages
    isLoading          // Loading state
  } = useLanguage();
  
  return (
    <select value={language} onChange={(e) => changeLanguage(e.target.value)}>
      {availableLanguages.map(lang => (
        <option key={lang.code} value={lang.code}>
          {lang.label}
        </option>
      ))}
    </select>
  );
}
```

**Language Detection Priority:**
1. Backend user preference (if authenticated)
2. localStorage
3. Browser language preference
4. Default: English (en)

### 11.4 Using useTranslation Hook

The custom `useTranslation` hook provides translation functions:

```typescript
import { useTranslation } from 'react-i18next';

function MyComponent() {
  // Basic usage with default namespace
  const { t } = useTranslation();
  const welcome = t('common.welcome');
  
  // With specific namespace
  const { t } = useTranslation('projects');
  const title = t('title');
  
  // With interpolation
  const message = t('showing', { count: 5, total: 10 });
  // Output: "Showing 5 of 10 projects"
  
  // Safe translation with fallback
  const { safeT } = useTranslation();
  const text = safeT('common.newKey', 'Fallback Text');
}
```

**Multiple Namespaces:**
```typescript
const { t } = useTranslation(['projects', 'common']);

// Use with namespace prefix
<h1>{t('projects:title')}</h1>
<button>{t('common:save')}</button>
```

### 11.5 Date Formatting Utilities

Use `formatDate` for locale-aware date formatting:

```typescript
import { formatDate, DateFormats, formatRelativeTime } from '@/utils/dateFormat';
import { useLanguage } from '@/contexts/LanguageContext';

function DateDisplay({ date }: { date: Date }) {
  const { language } = useLanguage();
  
  // Short date: "01/08/2026" (US) or "08/01/2026" (FR)
  const shortDate = formatDate(date, DateFormats.SHORT(language), language);
  
  // Long date: "January 8, 2026" (US) or "8 janvier 2026" (FR)
  const longDate = formatDate(date, DateFormats.LONG(language), language);
  
  // Relative time: "2 hours ago" or "il y a 2 heures"
  const relative = formatRelativeTime(date, language);
  
  return <div>{longDate}</div>;
}
```

**Available Date Formats:**
- `DateFormats.SHORT(language)` - Short date
- `DateFormats.LONG(language)` - Long date
- `DateFormats.DATETIME(language)` - Date with time
- `DateFormats.TIME(language)` - Time only
- `DateFormats.MONTH_DAY(language)` - Month and day
- `DateFormats.DAY_OF_WEEK(language)` - Day of week
- `DateFormats.PRETTY(language)` - Pretty print

### 11.6 Number Formatting Utilities

Use `formatNumber` for locale-aware number formatting:

```typescript
import { formatNumber, formatCurrency, formatPercentage } from '@/utils/numberFormat';
import { useLanguage } from '@/contexts/LanguageContext';

function NumberDisplay({ value }: { value: number }) {
  const { language } = useLanguage();
  
  // Formatted number: "1,234.56" (US) or "1 234,56" (FR)
  const formatted = formatNumber(value, language);
  
  // Currency: "$1,234.56" (US) or "1 234,56 €" (FR)
  const currency = formatCurrency(value, language, 'USD');
  
  // Percentage: "45.5%" (US) or "45,5 %" (FR)
  const percentage = formatPercentage(value, language);
  
  return <div>{formatted}</div>;
}
```

### 11.7 Language Toggle Component

The `LanguageToggle` component provides a dropdown to switch languages:

```typescript
import { LanguageToggle } from '@/components/layout/LanguageToggle';

function Header() {
  return (
    <header>
      <LanguageToggle />
    </header>
  );
}
```

The component:
- Shows current language with flag icon
- Provides dropdown with available languages
- Handles language switching
- Syncs with backend (if authenticated)
- Persists preference in localStorage

### 11.8 Adding Translations to Components

**Step 1:** Identify text to translate and choose namespace

**Step 2:** Add keys to translation files:

```json
// public/locales/en/projects.json
{
  "title": "Projects",
  "create": {
    "button": "Create Project"
  }
}

// public/locales/fr/projects.json
{
  "title": "Projets",
  "create": {
    "button": "Créer un projet"
  }
}
```

**Step 3:** Use translations in component:

```typescript
import { useTranslation } from 'react-i18next';

function ProjectsPage() {
  const { t } = useTranslation('projects');
  
  return (
    <div>
      <h1>{t('title')}</h1>
      <button>{t('create.button')}</button>
    </div>
  );
}
```

### 11.9 Validation

Run the validation script to check translation completeness:

```bash
npm run i18n:check
```

This checks:
- All keys exist in all languages
- No empty values
- Valid JSON structure
- Reports missing translations

### 11.10 Documentation

For detailed i18n documentation, see:
- **[i18n Documentation](../docs/i18n.md)** - Complete i18n guide
- **[Translation Guide](../docs/TRANSLATION_GUIDE.md)** - Contribution guide
- **[i18n Checklist](../docs/i18n-checklist.md)** - Implementation checklist

---

## 12. Forms & Validation

### 11.1 React Hook Form

#### 11.1.1 Basic Form

```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

const schema = z.object({
  name: z.string().min(1, 'Name is required'),
  email: z.string().email('Invalid email'),
});

const form = useForm({
  resolver: zodResolver(schema),
  defaultValues: {
    name: '',
    email: '',
  },
});
```

#### 11.1.2 Form Component

Using shadcn/ui Form component:

```typescript
<Form {...form}>
  <form onSubmit={form.handleSubmit(onSubmit)}>
    <FormField
      control={form.control}
      name="name"
      render={({ field }) => (
        <FormItem>
          <FormLabel>Name</FormLabel>
          <FormControl>
            <Input {...field} />
          </FormControl>
          <FormMessage />
        </FormItem>
      )}
    />
    <Button type="submit">Submit</Button>
  </form>
</Form>
```

### 11.2 Validation

#### 11.2.1 Zod Schemas

```typescript
const createProjectSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200, 'Name too long'),
  description: z.string().max(1000, 'Description too long'),
  type: z.enum(['Scrum', 'Kanban', 'Waterfall']),
  sprintDurationDays: z.number().min(1).max(30),
});
```

#### 11.2.2 Custom Validation

```typescript
const schema = z.object({
  password: z.string()
    .min(8, 'Password must be at least 8 characters')
    .regex(/[A-Z]/, 'Password must contain uppercase letter')
    .regex(/[a-z]/, 'Password must contain lowercase letter')
    .regex(/[0-9]/, 'Password must contain number'),
});
```

### 11.3 Form Patterns

#### 11.3.1 Create Forms

- Validation on submit
- Loading state during submission
- Success/error feedback
- Form reset on success

#### 11.3.2 Edit Forms

- Pre-populate with existing data
- Only submit changed fields
- Optimistic updates
- Error rollback

---

## 13. Testing

### 13.1 Testing Stack

- **Vitest**: Test runner
- **@testing-library/react**: Component testing
- **@testing-library/user-event**: User interaction simulation
- **@testing-library/jest-dom**: DOM matchers
- **MSW**: API mocking

### 13.2 Test Structure

```
src/
├── components/
│   └── [Component].test.tsx
├── pages/
│   └── [Page].test.tsx
├── contexts/
│   └── [Context].test.tsx
├── hooks/
│   └── [Hook].test.ts
├── lib/
│   └── utils.test.ts
└── test/
    ├── setup.ts          # Test setup
    └── test-utils.tsx    # Testing utilities
```

### 13.3 Test Examples

#### 13.3.1 Component Test

```typescript
import { render, screen } from '@testing-library/react';
import { Button } from './Button';

test('renders button with text', () => {
  render(<Button>Click me</Button>);
  expect(screen.getByText('Click me')).toBeInTheDocument();
});
```

#### 13.3.2 Hook Test

```typescript
import { renderHook } from '@testing-library/react';
import { useDebounce } from './use-debounce';

test('debounces value', async () => {
  const { result, rerender } = renderHook(
    ({ value }) => useDebounce(value, 300),
    { initialProps: { value: 'test' } }
  );
  
  expect(result.current).toBe('test');
  rerender({ value: 'updated' });
  // Wait for debounce...
});
```

#### 13.3.3 API Mocking

```typescript
import { server } from '@/mocks/server';
import { rest } from 'msw';

server.use(
  rest.get('/api/v1/Projects', (req, res, ctx) => {
    return res(ctx.json({ data: mockProjects }));
  })
);
```

### 13.4 Running Tests

```bash
# Run all tests
npm test

# Run tests in watch mode
npm test -- --watch

# Run tests with UI
npm run test:ui

# Run tests with coverage
npm run test:coverage

# Run tests once
npm run test:run
```

### 13.5 Test Coverage

Target: 70% coverage for:
- Lines
- Functions
- Branches
- Statements

---

## 14. Development Setup

### 14.1 Prerequisites

- **Node.js**: v18 or higher (recommend using nvm)
- **npm**: v9 or higher (comes with Node.js)
- **Git**: For version control

### 14.2 Installation

#### 14.2.1 Clone Repository

```bash
git clone <repository-url>
cd frontend
```

#### 14.2.2 Install Dependencies

```bash
npm install
```

#### 14.2.3 Environment Variables

Create `.env` file in the frontend root (copy from `.env.example`):

```env
# API Configuration
VITE_API_BASE_URL=http://localhost:5001

# Sentry Error Tracking (Optional)
VITE_SENTRY_DSN=
VITE_SENTRY_ENVIRONMENT=development
```

**Environment Variables:**
- `VITE_API_BASE_URL` - Backend API base URL (without trailing slash, default: `http://localhost:5001`)
- `VITE_SENTRY_DSN` - Sentry DSN for error tracking (optional, leave empty to disable)
- `VITE_SENTRY_ENVIRONMENT` - Environment name for Sentry (default: `development`)

**Note:** All environment variables must be prefixed with `VITE_` to be accessible in the frontend code.

#### 14.2.4 Start Development Server

```bash
npm run dev
```

Application runs on `http://localhost:8080`

### 14.3 Development Tools

#### 14.3.1 Vite Dev Server

- **Hot Module Replacement (HMR)**: Instant updates
- **Fast Refresh**: Preserves component state
- **Source Maps**: Debugging support
- **Port**: 8080 (configurable)

#### 14.3.2 Browser DevTools

- **React DevTools**: Component inspection
- **TanStack Query DevTools**: Query inspection (if enabled)
- **Redux DevTools**: Not used (using Context API)

### 14.4 Code Quality

#### 14.4.1 Linting

```bash
npm run lint
```

**ESLint Configuration:**
- React Hooks rules
- TypeScript rules
- Import ordering
- Code style enforcement

#### 14.4.2 Type Checking

TypeScript strict mode is enabled with comprehensive type checking:

```bash
# Run type checking explicitly
npm run type-check

# Type checking also runs during build
npm run build
```

**TypeScript Strict Mode Configuration:**

The project uses strict TypeScript configuration (`tsconfig.app.json`) with the following options enabled:

```json
{
  "strict": true,
  "noImplicitAny": true,
  "strictNullChecks": true,
  "strictFunctionTypes": true,
  "strictBindCallApply": true,
  "strictPropertyInitialization": true,
  "noImplicitThis": true,
  "alwaysStrict": true,
  "noUnusedLocals": true,
  "noUnusedParameters": true,
  "noImplicitReturns": true,
  "noFallthroughCasesInSwitch": true
}
```

**Benefits:**
- Catches errors at compile-time before runtime
- Prevents null/undefined access errors
- Enforces explicit type annotations
- Detects unused code
- Improves code quality and maintainability

**Status:** ✅ All TypeScript strict mode errors have been resolved. The project passes type checking with 0 errors.

### 13.5 Development Workflow

1. **Create Feature Branch**
   ```bash
   git checkout -b feature/new-feature
   ```

2. **Make Changes**
   - Write code
   - Write tests
   - Update documentation

3. **Test Changes**
   ```bash
   npm test
   npm run lint
   npm run type-check  # Verify TypeScript strict mode compliance
   ```

4. **Commit Changes**
   ```bash
   git add .
   git commit -m "feat: add new feature"
   ```

5. **Push and Create PR**
   ```bash
   git push origin feature/new-feature
   ```

---

## 15. Build & Deployment

### 14.1 Building for Production

#### 14.1.1 Build Command

```bash
npm run build
```

**Output:**
- `dist/` directory with optimized production build
- Minified JavaScript
- Optimized CSS
- Asset optimization
- Code splitting

#### 14.1.2 Build Configuration

Located in `vite.config.ts`:
- **Output**: `dist/`
- **Source Maps**: Disabled in production
- **Minification**: Enabled
- **Tree Shaking**: Automatic

### 14.2 Preview Build

```bash
npm run preview
```

Serves production build locally for testing.

### 14.3 Deployment Options

#### 14.3.1 Static Hosting

Deploy `dist/` folder to:
- **Vercel**: Automatic deployments from Git
- **Netlify**: Drag-and-drop or Git integration
- **Azure Static Web Apps**: Azure integration
- **GitHub Pages**: Free static hosting
- **AWS S3 + CloudFront**: Scalable hosting
- **Any Web Server**: Nginx, Apache, etc.

#### 14.3.2 Docker

Create `Dockerfile`:

```dockerfile
FROM nginx:alpine
COPY dist/ /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

#### 14.3.3 Environment-Specific Builds

```bash
# Development build
npm run build:dev

# Production build
npm run build
```

### 14.4 Environment Variables

#### 14.4.1 Environment Variables Reference

All environment variables must be prefixed with `VITE_` to be accessible in the frontend code.

**Required Variables:**
- `VITE_API_BASE_URL` - Backend API base URL (default: `http://localhost:5001`)

**Optional Variables:**
- `VITE_SENTRY_DSN` - Sentry DSN for error tracking (leave empty to disable)
- `VITE_SENTRY_ENVIRONMENT` - Environment name for Sentry (default: `development`)

**Example `.env` file:**
```bash
# API Configuration
VITE_API_BASE_URL=http://localhost:5001

# Sentry Error Tracking (Optional)
VITE_SENTRY_DSN=
VITE_SENTRY_ENVIRONMENT=development
```

**Example `.env.production` file:**
```bash
VITE_API_BASE_URL=https://api.intellipm.com
VITE_SENTRY_DSN=https://your-dsn@sentry.io/project-id
VITE_SENTRY_ENVIRONMENT=production
```

#### 14.4.2 Build-Time Variables

Environment variables are embedded at build time. Changes require a rebuild.

```typescript
const apiUrl = import.meta.env.VITE_API_BASE_URL;
```

#### 14.4.3 Production Variables

Set environment variables in your hosting platform:

- **Vercel**: Environment variables in dashboard
- **Netlify**: Environment variables in dashboard
- **Docker**: Pass via `-e` flags or `.env` file

### 14.5 Deployment Checklist

- [ ] Update `VITE_API_BASE_URL` for production
- [ ] Configure Sentry DSN
- [ ] Set up CORS on backend
- [ ] Test production build locally
- [ ] Verify API connectivity
- [ ] Test authentication flow
- [ ] Check error tracking
- [ ] Verify analytics (if used)
- [ ] Test on multiple browsers
- [ ] Test responsive design
- [ ] Verify performance (Lighthouse)

---

## 16. Best Practices

### 15.1 Component Design

#### 15.1.1 Component Structure

```typescript
// 1. Imports
import React from 'react';
import { Button } from '@/components/ui/button';

// 2. Types/Interfaces
interface ComponentProps {
  title: string;
  onAction: () => void;
}

// 3. Component
export const Component: React.FC<ComponentProps> = ({ title, onAction }) => {
  // 4. Hooks
  const [state, setState] = useState();
  
  // 5. Handlers
  const handleClick = () => {
    // Logic
  };
  
  // 6. Render
  return (
    <div>
      <h1>{title}</h1>
      <Button onClick={handleClick}>Action</Button>
    </div>
  );
};
```

#### 15.1.2 Component Guidelines

- **Single Responsibility**: One component, one purpose
- **Composition**: Build complex UIs from simple components
- **Props Interface**: Always define TypeScript interfaces
- **Default Props**: Use default parameters or defaultProps
- **Memoization**: Use `React.memo` for expensive renders
- **Extract Logic**: Move complex logic to custom hooks

### 15.2 State Management

#### 15.2.1 When to Use Context

- **Global State**: Authentication, theme
- **Shared State**: Current project, user preferences
- **Avoid**: Frequently changing state (use TanStack Query)

#### 15.2.2 When to Use TanStack Query

- **Server State**: API data
- **Caching**: Automatic caching and invalidation
- **Background Updates**: Automatic refetching
- **Optimistic Updates**: Update UI before server response

#### 15.2.3 When to Use Local State

- **UI State**: Modal open/close, form inputs
- **Component State**: Component-specific state
- **Derived State**: Computed from props

### 15.3 Performance Optimization

#### 15.3.1 Code Splitting

```typescript
// Lazy load routes
const AdminDashboard = lazy(() => import('./pages/admin/AdminDashboard'));

<Suspense fallback={<Loading />}>
  <AdminDashboard />
</Suspense>
```

#### 15.3.2 Memoization

```typescript
// Memoize expensive computations
const expensiveValue = useMemo(() => {
  return computeExpensiveValue(data);
}, [data]);

// Memoize callbacks
const handleClick = useCallback(() => {
  doSomething(id);
}, [id]);
```

#### 15.3.3 Virtualization

For long lists, use virtualization:
- `react-window` or `react-virtualized`
- Only render visible items

### 15.4 Accessibility

#### 15.4.1 ARIA Labels

```typescript
<button aria-label="Close dialog">
  <X />
</button>
```

#### 15.4.2 Keyboard Navigation

- All interactive elements keyboard accessible
- Focus management in modals
- Skip links for navigation

#### 15.4.3 Screen Readers

- Semantic HTML
- Proper heading hierarchy
- Alt text for images
- Form labels

### 15.5 Error Handling

#### 15.5.1 Error Boundaries

```typescript
<Sentry.ErrorBoundary fallback={ErrorFallback}>
  <App />
</Sentry.ErrorBoundary>
```

#### 15.5.2 API Error Handling

The API client (`client.ts`) provides comprehensive automatic error handling:

**Automatic Error Handling:**
- User-friendly error messages based on HTTP status codes
- Toast notifications for all errors (except 401 redirects)
- Sentry logging for server errors (5xx)
- Automatic token refresh on 401 errors
- Rate limit handling with retry-after information

**Error Message Mapping:**
```typescript
// Status code → User-friendly message
401 → "Session expired. Please log in again."
403 → "You don't have permission for this action."
429 → "Too many requests. Please try again later."
500 → "Server error. Please contact support."
502/503/504 → "Service temporarily unavailable. Please try again later."
```

**Component-Level Error Handling:**
```typescript
try {
  await projectsApi.create(data);
  // Success - toast already shown by API client
} catch (error) {
  // Error already handled by API client:
  // - Toast notification shown
  // - Sentry logged (if 5xx)
  // - User-friendly message displayed
  // Component can handle specific cases if needed
}
```

**Legacy Pattern (Still Supported):**
```typescript
import { showToast, showError } from '@/lib/sweetalert';

try {
  await api.create(data);
  showToast('Created successfully', 'success');
} catch (error) {
  showError('Failed to create', error instanceof Error ? error.message : 'An error occurred');
}
```

#### 15.5.3 Form Validation

- Client-side validation (Zod)
- Server-side error display
- Field-level error messages

---

## 17. Troubleshooting

### 16.1 Common Issues

#### 16.1.1 CORS Errors

**Issue**: `CORS policy: No 'Access-Control-Allow-Origin' header`

**Solutions:**
- Verify backend CORS configuration
- Check `VITE_API_BASE_URL` matches backend URL
- Ensure `credentials: 'include'` in API client
- Check backend allows frontend origin

#### 16.1.2 Authentication Issues

**Issue**: User not authenticated after login

**Solutions:**
- Check cookies are being set (DevTools → Application → Cookies)
- Verify `credentials: 'include'` in fetch requests
- Check backend cookie settings (SameSite, Secure, HttpOnly)
- Clear cookies and try again

#### 16.1.3 Build Errors

**Issue**: TypeScript errors during build

**Solutions:**
- Run `npm run type-check` to see all TypeScript errors
- Fix TypeScript errors (strict mode enabled)
- Check `tsconfig.app.json` settings
- Verify all imports are correct
- Ensure null checks for optional properties
- Remove unused imports and variables
- Add explicit type annotations where needed

**Common Strict Mode Errors:**
- **TS6133**: Unused variable/import - Remove or prefix with `_`
- **TS2339**: Property doesn't exist - Check API response structure
- **TS18048**: Possibly undefined - Add null check or optional chaining
- **TS7006**: Implicit any - Add explicit type annotation

#### 16.1.4 Styling Issues

**Issue**: Styles not applying

**Solutions:**
- Check Tailwind classes are in content paths
- Verify `index.css` is imported in `main.tsx`
- Check for CSS conflicts
- Clear browser cache

#### 16.1.5 API Errors

**Issue**: 401 Unauthorized errors

**Solutions:**
- Check authentication token in cookies
- Verify token hasn't expired
- Check backend authentication middleware
- Try logging out and back in

**Issue**: 500 Internal Server Error on `/api/admin/dashboard/stats`

**Solutions:**
- Check browser console for detailed error message
- Verify backend logs for specific error
- Ensure user has valid organization ID
- Check that all required data is available in database

**Issue**: 500 Internal Server Error on `/api/admin/users/invite`

**Solutions:**
- Check that invitation was created (even if email failed)
- Verify SMTP configuration in Admin Settings → Email tab
- Check backend logs for email service errors
- Test email configuration using "Send Test Email" button
- Verify email template exists on backend

### 16.2 Debugging

#### 16.2.1 React DevTools

- Install React DevTools browser extension
- Inspect component tree
- View props and state
- Profile performance

#### 16.2.2 TanStack Query DevTools

Enable in development:

```typescript
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';

<QueryClientProvider client={queryClient}>
  <App />
  <ReactQueryDevtools initialIsOpen={false} />
</QueryClientProvider>
```

#### 16.2.3 Network Tab

- Inspect API requests
- Check request/response headers
- Verify payloads
- Check response status codes

#### 16.2.4 Console Logging

```typescript
// Development only
if (import.meta.env.DEV) {
  console.log('Debug info', data);
}
```

### 16.3 Performance Issues

#### 16.3.1 Slow Initial Load

**Solutions:**
- Enable code splitting
- Lazy load routes
- Optimize bundle size
- Use CDN for static assets

#### 16.3.2 Slow Re-renders

**Solutions:**
- Use `React.memo` for components
- Memoize expensive computations
- Optimize TanStack Query queries
- Check for unnecessary re-renders

#### 16.3.3 Memory Leaks

**Solutions:**
- Clean up event listeners
- Cancel API requests on unmount
- Clear intervals/timeouts
- Unsubscribe from observables

---

## 18. TypeScript

### 17.1 TypeScript Strict Mode

The project uses **TypeScript strict mode** for maximum type safety. All strict mode options are enabled in `tsconfig.app.json`.

#### 17.1.1 Strict Mode Configuration

```json
{
  "compilerOptions": {
    "strict": true,
    "noImplicitAny": true,
    "strictNullChecks": true,
    "strictFunctionTypes": true,
    "strictBindCallApply": true,
    "strictPropertyInitialization": true,
    "noImplicitThis": true,
    "alwaysStrict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noImplicitReturns": true,
    "noFallthroughCasesInSwitch": true
  }
}
```

#### 17.1.2 Running Type Checks

```bash
# Run type checking
npm run type-check

# Type checking runs automatically during build
npm run build
```

**Status:** ✅ All 164 initial strict mode errors have been resolved. The project now passes type checking with 0 errors.

### 17.2 Type Definitions

#### 17.2.1 Types Location

All types defined in `src/types/index.ts`:

```typescript
export interface User {
  userId: number;
  username: string;
  email: string;
  globalRole: GlobalRole;
  organizationId: number;
  permissions: string[];
  firstName?: string;
  lastName?: string;
  // ...
}
```

#### 17.2.2 Type Usage

```typescript
import type { User, Project, Task } from '@/types';

const user: User = { /* ... */ };
```

### 17.3 Type Safety

#### 17.3.1 API Responses

```typescript
const { data } = useQuery({
  queryKey: ['project', id],
  queryFn: () => projectsApi.getById(id), // Returns Promise<Project>
});
// data is typed as Project | undefined (strictNullChecks)
```

**Important:** With `strictNullChecks` enabled, always check for `undefined`:

```typescript
if (data) {
  // TypeScript knows data is Project here
  console.log(data.name);
}
```

#### 17.3.2 Component Props

```typescript
interface ComponentProps {
  title: string;
  count?: number; // Optional with strictNullChecks
  onAction: (id: number) => void;
}

export const Component: React.FC<ComponentProps> = ({ title, count, onAction }) => {
  // TypeScript enforces prop types
  // count is number | undefined, must check before use
  if (count !== undefined) {
    console.log(count);
  }
};
```

#### 17.3.3 Null Safety Patterns

With `strictNullChecks` enabled, use these patterns:

```typescript
// Optional chaining
const name = member.firstName?.toUpperCase();

// Nullish coalescing
const displayName = member.firstName ?? member.userName ?? 'Unknown';

// Type guards
if (member.firstName && member.lastName) {
  const fullName = `${member.firstName} ${member.lastName}`;
}

// Explicit null checks
if (data !== null && data !== undefined) {
  // TypeScript narrows type here
}
```

### 17.4 Type Utilities

```typescript
// Partial types
type PartialProject = Partial<Project>;

// Pick types
type ProjectSummary = Pick<Project, 'id' | 'name' | 'status'>;

// Omit types
type ProjectWithoutId = Omit<Project, 'id'>;

// Required types
type RequiredProject = Required<Project>;

// Readonly types
type ReadonlyProject = Readonly<Project>;
```

### 17.5 Common Type Patterns

#### 17.5.1 API Response Types

```typescript
// Paged responses
export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// Usage
const { data } = useQuery({
  queryKey: ['projects'],
  queryFn: () => projectsApi.getAll(), // Returns PagedResponse<Project>
});
// data.items is Project[]
```

#### 17.5.2 Event Handler Types

```typescript
// Explicitly type event handlers
const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
  event.preventDefault();
  // ...
};

const handleChange = (value: string) => {
  // ...
};

// Generic handler for form fields
const handleFieldChange = <K extends keyof UpdateTaskRequest>(
  field: K,
  value: UpdateTaskRequest[K]
) => {
  // Type-safe field updates
};
```

#### 17.5.3 Unused Parameters

For intentionally unused parameters, prefix with underscore:

```typescript
// Unused parameter
const handleClick = (_event: React.MouseEvent) => {
  // Parameter intentionally unused
};

// Unused variable
const { data: _unusedData, isLoading } = useQuery(...);
```

### 17.6 Type Checking Best Practices

1. **Always define types** for function parameters and return values
2. **Use type guards** to narrow types when checking for null/undefined
3. **Prefer explicit types** over `any` (use `unknown` if type is truly unknown)
4. **Remove unused imports and variables** (enforced by `noUnusedLocals`)
5. **Handle optional properties** with null checks or optional chaining
6. **Use type assertions sparingly** and prefer type guards
7. **Export types** that are used across modules
8. **Use `as const`** for literal types when needed

### 17.7 TypeScript Configuration Files

- **`tsconfig.json`**: Base TypeScript configuration
- **`tsconfig.app.json`**: Application-specific configuration (extends base, includes strict mode)
- **`tsconfig.node.json`**: Node.js-specific configuration (for Vite config, etc.)

For detailed information about the strict mode implementation and all fixes applied, see the [TypeScript Strict Mode Report](./TYPESCRIPT_STRICT_MODE_REPORT.md).

---

## 19. API Integration Patterns

### 18.1 Query Patterns

#### 18.1.1 List Queries

```typescript
const { data, isLoading } = useQuery({
  queryKey: ['projects', page, pageSize],
  queryFn: () => projectsApi.getAll(page, pageSize),
  keepPreviousData: true, // Smooth pagination
});
```

#### 18.1.2 Detail Queries

```typescript
const { data: project } = useQuery({
  queryKey: ['project', projectId],
  queryFn: () => projectsApi.getById(projectId),
  enabled: !!projectId, // Only fetch if projectId exists
});
```

#### 18.1.3 Dependent Queries

```typescript
const { data: project } = useQuery({
  queryKey: ['project', projectId],
  queryFn: () => projectsApi.getById(projectId),
});

const { data: tasks } = useQuery({
  queryKey: ['tasks', projectId],
  queryFn: () => tasksApi.getByProject(projectId),
  enabled: !!project, // Only fetch if project exists
});
```

### 18.2 Mutation Patterns

#### 18.2.1 Create Mutation

```typescript
import { showToast, showError } from '@/lib/sweetalert';

const createMutation = useMutation({
  mutationFn: (data: CreateProjectRequest) => projectsApi.create(data),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['projects'] });
    showToast('Project created', 'success');
    navigate('/projects');
  },
  onError: (error) => {
    showError('Failed to create project', error instanceof Error ? error.message : 'Please try again');
  },
});
```

#### 18.2.2 Update Mutation

```typescript
import { showToast } from '@/lib/sweetalert';

const updateMutation = useMutation({
  mutationFn: ({ id, data }: { id: number; data: UpdateProjectRequest }) =>
    projectsApi.update(id, data),
  onSuccess: (_, variables) => {
    queryClient.invalidateQueries({ queryKey: ['project', variables.id] });
    queryClient.invalidateQueries({ queryKey: ['projects'] });
    showToast('Project updated', 'success');
  },
});
```

#### 18.2.3 Delete Mutation

```typescript
import { showToast } from '@/lib/sweetalert';

const deleteMutation = useMutation({
  mutationFn: (id: number) => projectsApi.delete(id),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['projects'] });
    showToast('Project deleted', 'success');
    navigate('/projects');
  },
});
```

### 18.3 Optimistic Updates

```typescript
const updateMutation = useMutation({
  mutationFn: updateTask,
  onMutate: async (newTask) => {
    // Cancel outgoing refetches
    await queryClient.cancelQueries({ queryKey: ['task', newTask.id] });
    
    // Snapshot previous value
    const previousTask = queryClient.getQueryData(['task', newTask.id]);
    
    // Optimistically update
    queryClient.setQueryData(['task', newTask.id], newTask);
    
    return { previousTask };
  },
  onError: (err, newTask, context) => {
    // Rollback on error
    queryClient.setQueryData(['task', newTask.id], context?.previousTask);
  },
  onSettled: (data, error, variables) => {
    // Refetch to ensure consistency
    queryClient.invalidateQueries({ queryKey: ['task', variables.id] });
  },
});
```

---

## 20. Feature Flags

### 19.1 Overview

The application includes a comprehensive feature flag system for runtime feature toggling. Feature flags can be global (applying to all organizations) or organization-specific.

### 19.2 Architecture

```
FeatureFlagsProvider (Context)
    ↓
featureFlagService (Service Layer)
    ↓
Backend API (/api/feature-flags)
```

### 19.3 FeatureFlagsContext

The `FeatureFlagsContext` provides global access to feature flags:

```typescript
const { flags, isEnabled, isLoading, refresh } = useFeatureFlags();

// Check if a flag is enabled
if (isEnabled('EnableAIInsights')) {
  // Show AI features
}
```

**Features:**
- Fetches all flags on mount for current organization
- Auto-refreshes every 5 minutes
- Provides flags via context for global access
- Handles loading and error states

### 19.4 Feature Flag Service

The `featureFlagService` provides low-level API access with caching:

```typescript
import { featureFlagService } from '@/services/featureFlagService';

// Check if enabled
const isEnabled = await featureFlagService.isEnabled('EnableAIInsights', 'orgId');

// Get full flag details
const flag = await featureFlagService.getFlag('EnableAdvancedMetrics', 'orgId');

// Get all flags
const flags = await featureFlagService.getAllFlags('orgId');

// Refresh cache
await featureFlagService.refreshCache();

// Clear cache
featureFlagService.clearCache();
```

**Caching:**
- In-memory cache with 5-minute TTL
- Automatic expiration checking
- Prevents duplicate API calls
- Auto-refresh expired entries

### 19.5 Feature Flag Component

The `FeatureFlag` component conditionally renders UI based on flag state:

```tsx
import { FeatureFlag } from '@/components/FeatureFlag';
import { FeatureFlagName } from '@/types/featureFlags';

// Basic usage
<FeatureFlag flagName="EnableAIInsights">
  <AIInsightsPanel />
</FeatureFlag>

// With fallback
<FeatureFlag 
  flagName="EnableGanttChart" 
  fallback={<ComingSoonBadge />}
>
  <GanttChartView />
</FeatureFlag>

// Using enum
<FeatureFlag flagName={FeatureFlagName.EnableAdvancedMetrics}>
  <AdvancedMetrics />
</FeatureFlag>

// Require all flags
<FeatureFlagAll 
  flagNames={['EnableAIInsights', 'EnableAdvancedMetrics']}
>
  <CombinedFeature />
</FeatureFlagAll>

// Require any flag
<FeatureFlagAny 
  flagNames={['EnableAIInsights', 'EnableAdvancedMetrics']}
>
  <AnyFeature />
</FeatureFlagAny>
```

### 19.6 useFeatureFlag Hook

Hook to check individual feature flags:

```typescript
import { useFeatureFlag } from '@/hooks/useFeatureFlag';

const { isEnabled, isLoading, error } = useFeatureFlag('EnableAIInsights');

if (isEnabled) {
  return <AIInsightsPanel />;
}
```

**Behavior:**
- First checks FeatureFlagsContext for cached flags
- Falls back to featureFlagService if not in context
- Returns loading and error states

### 19.7 Feature Flag Utilities

Utility functions for working with feature flags:

```typescript
import { 
  checkFeatureFlag, 
  getEnabledFeatures, 
  areAllEnabled,
  isAnyEnabled 
} from '@/utils/featureFlags';

const flags = { EnableAIInsights: true, EnableAdvancedMetrics: false };

// Check single flag
const isEnabled = checkFeatureFlag('EnableAIInsights', flags);

// Get all enabled flags
const enabled = getEnabledFeatures(flags); // ['EnableAIInsights']

// Check if all flags are enabled
const allEnabled = areAllEnabled(['EnableAIInsights', 'EnableAdvancedMetrics'], flags);

// Check if any flag is enabled
const anyEnabled = isAnyEnabled(['EnableAIInsights', 'EnableAdvancedMetrics'], flags);
```

### 19.8 Feature Flag Types

All feature flag types are defined in `src/types/featureFlags.ts`:

```typescript
// Known feature flags enum
export enum FeatureFlagName {
  EnableAIInsights = 'EnableAIInsights',
  EnableAdvancedMetrics = 'EnableAdvancedMetrics',
  EnableRealTimeCollaboration = 'EnableRealTimeCollaboration',
  // ... more flags
}

// Feature flag interface
export interface FeatureFlag {
  id: string;
  name: string;
  isEnabled: boolean;
  organizationId?: string;
  description?: string;
  createdAt?: string;
  updatedAt?: string;
}
```

### 19.9 Integration

The `FeatureFlagsProvider` is integrated in `App.tsx`:

```tsx
<AuthProvider>
  <FeatureFlagsProvider>
    <App />
  </FeatureFlagsProvider>
</AuthProvider>
```

This ensures feature flags are available throughout the application.

### 19.10 Best Practices

1. **Use FeatureFlag Component**: Prefer the component over manual checks for UI rendering
2. **Use Context**: Access flags via context when possible (faster, cached)
3. **Fail-Safe**: Always assume features are disabled on errors
4. **Organization-Specific**: Use `organizationId` for org-specific flags
5. **Type Safety**: Use `FeatureFlagName` enum for known flags
6. **Cache Management**: Let the service handle caching automatically

---

## 21. SweetAlert2 Integration

### 20.1 Overview

The application uses SweetAlert2 for all alert dialogs and toast notifications, providing a consistent, beautiful, and customizable user feedback system. All previous toast notification systems (sonner) and native browser alerts have been replaced with SweetAlert2.

### 20.2 Installation

```bash
npm install sweetalert2 sweetalert2-react-content
```

### 20.3 Wrapper Utility

The SweetAlert2 wrapper is located in `src/lib/sweetalert.ts` and provides convenient functions for common alert patterns:

```typescript
import Swal from 'sweetalert2';
import withReactContent from 'sweetalert2-react-content';

export const MySwal = withReactContent(Swal);
```

### 20.4 Available Functions

#### 20.4.1 Success Alert

```typescript
import { showSuccess } from '@/lib/sweetalert';

showSuccess('Operation successful', 'Your changes have been saved.');
```

#### 20.4.2 Error Alert

```typescript
import { showError } from '@/lib/sweetalert';

showError('Operation failed', 'Please try again or contact support.');
```

#### 20.4.3 Warning Alert

```typescript
import { showWarning } from '@/lib/sweetalert';

showWarning('Invalid input', 'Please check your input and try again.');
```

#### 20.4.4 Info Alert

```typescript
import { showInfo } from '@/lib/sweetalert';

showInfo('Information', 'This feature is currently in beta.');
```

#### 20.4.5 Toast Notifications

```typescript
import { showToast } from '@/lib/sweetalert';

// Success toast
showToast('Project created successfully', 'success');

// Error toast
showToast('Failed to create project', 'error');

// Warning toast
showToast('Please review your input', 'warning');

// Info toast
showToast('New update available', 'info');
```

**Toast Configuration:**
- Position: `bottom-end` (bottom-right corner)
- Duration: 3000ms (3 seconds)
- Auto-close: Yes
- Progress bar: Yes

#### 20.4.6 Confirmation Dialog

```typescript
import { showConfirm } from '@/lib/sweetalert';

const confirmed = await showConfirm(
  'Delete Project',
  'Are you sure you want to delete this project? This action cannot be undone.',
  'Yes, delete it',
  'Cancel'
);

if (confirmed) {
  // User confirmed
  await deleteProject();
}
```

#### 20.4.7 Input Prompt

```typescript
import { showPrompt } from '@/lib/sweetalert';

const email = await showPrompt(
  'Enter Email',
  'Please enter your email address',
  'email' // or 'text', 'password'
);

if (email) {
  // User provided input
  console.log(email);
}
```

#### 20.4.8 Loading Indicator

```typescript
import { showLoading, closeAlert } from '@/lib/sweetalert';

// Show loading
showLoading('Processing your request...');

// Close after operation completes
try {
  await performOperation();
  closeAlert();
  showSuccess('Operation completed');
} catch (error) {
  closeAlert();
  showError('Operation failed', error.message);
}
```

### 20.5 Migration from Sonner

All `toast()` calls from sonner have been replaced with SweetAlert2 functions:

**Before:**
```typescript
import { useToast } from '@/hooks/use-toast';

const { toast } = useToast();
toast({ title: 'Success', description: 'Operation completed' });
```

**After:**
```typescript
import { showToast } from '@/lib/sweetalert';

showToast('Operation completed', 'success');
```

### 20.6 Migration from Native Alerts

All native browser alerts have been replaced:

**Before:**
```typescript
if (window.confirm('Are you sure?')) {
  // User confirmed
}
```

**After:**
```typescript
import { showConfirm } from '@/lib/sweetalert';

const confirmed = await showConfirm('Are you sure?', 'This action cannot be undone.');
if (confirmed) {
  // User confirmed
}
```

### 20.7 Theme Integration

SweetAlert2 automatically adapts to the application's theme (light/dark mode) via CSS variables. The alerts respect the current theme setting from `ThemeContext`.

### 20.8 Best Practices

1. **Use Toast for Non-Critical Messages**: Use `showToast()` for success messages, non-critical errors, and informational updates
2. **Use Modal Alerts for Critical Actions**: Use `showError()`, `showWarning()`, `showSuccess()` for important messages that require user attention
3. **Use Confirmation for Destructive Actions**: Always use `showConfirm()` before delete operations or other destructive actions
4. **Provide Clear Messages**: Always include descriptive titles and messages
5. **Handle Async Confirmations**: Always `await` confirmation dialogs before proceeding with actions

### 20.9 Examples in Codebase

**Mutation Success:**
```typescript
const mutation = useMutation({
  mutationFn: createProject,
  onSuccess: () => {
    showToast('Project created successfully', 'success');
    queryClient.invalidateQueries({ queryKey: ['projects'] });
  },
  onError: (error) => {
    showError('Failed to create project', error.message);
  },
});
```

**Delete Confirmation:**
```typescript
const handleDelete = async () => {
  const confirmed = await showConfirm(
    'Delete Project',
    `Are you sure you want to delete "${project.name}"? This action cannot be undone.`,
    'Yes, delete it',
    'Cancel'
  );
  
  if (confirmed) {
    await deleteMutation.mutateAsync(project.id);
    showToast('Project deleted', 'success');
  }
};
```

**API Error Handling:**
```typescript
try {
  await api.create(data);
  showToast('Created successfully', 'success');
} catch (error) {
  showError(
    'Failed to create',
    error instanceof Error ? error.message : 'An unexpected error occurred'
  );
}
```

---

## 22. Custom Hooks

### 21.1 Existing Hooks

#### 21.1.1 useAuth

```typescript
const { user, isAuthenticated, login, logout } = useAuth();
```

Provides authentication state and methods.

#### 21.1.2 useTheme

```typescript
const { theme, setTheme, resolvedTheme } = useTheme();
```

Manages theme (light/dark/system).

#### 21.1.3 useProjectPermissions

```typescript
const {
  userRole,
  canEditProject,
  canDeleteProject,
  canInviteMembers,
  // ... other permissions
} = useProjectPermissions(projectId);
```

Provides project-level permissions based on user role.

#### 21.1.4 useDebounce

```typescript
const debouncedValue = useDebounce(value, 300);
```

Debounces a value (useful for search inputs).

#### 21.1.5 useMobile

```typescript
const isMobile = useMobile();
```

Detects if device is mobile.

#### 21.1.6 useAIErrorHandler

```typescript
import { useAIErrorHandler } from '@/hooks/useAIErrorHandler';

// Automatically handles AI quota exceeded and AI disabled errors
// Shows toast notifications and manages error state
useAIErrorHandler();
```

Handles AI-related errors globally:
- Quota exceeded errors (429 status)
- AI disabled errors
- Shows appropriate toast notifications
- Manages global error state

#### 21.1.7 useQuotaNotifications

```typescript
import { useQuotaNotifications } from '@/hooks/useQuotaNotifications';

// Automatically shows quota threshold notifications
useQuotaNotifications();
```

Automatically monitors quota status and shows notifications:
- Warning toast at 80% usage (once per session)
- Error toast at 100% usage (once per session)
- Auto-refreshes every 60 seconds
- Resets flags when quota drops below thresholds

#### 21.1.8 useReadModels

Hooks for accessing CQRS read models:

```typescript
// Task board read model
const { data: taskBoard, isLoading } = useTaskBoard(projectId);

// Sprint summary read model
const { data: sprintSummary } = useSprintSummary(sprintId);

// Project overview read model
const { data: projectOverview } = useProjectOverview(projectId);

// Multiple project overviews (for dashboard)
const { data: projectOverviews } = useProjectOverviews({
  organizationId: 1,
  status: 'Active',
  page: 1,
  pageSize: 10
});
```

**Features:**
- Pre-calculated metrics and aggregations
- Optimized queries for dashboard views
- Automatic cache management via TanStack Query
- Handles both PascalCase and camelCase response formats

#### 21.1.9 useTaskDependencies

Hook for managing task dependencies:

```typescript
const { data: dependencies, isLoading, addDependency, removeDependency } = useTaskDependencies(taskId);
```

#### 21.1.10 useProjectTaskDependencies

Hook for managing project-level task dependencies:

```typescript
const { data: dependencies, isLoading } = useProjectTaskDependencies(projectId);
```

#### 21.1.11 useLookups

Hooks for accessing lookup/reference data:

```typescript
// Task statuses with metadata
const { data: taskStatuses, isLoading } = useTaskStatuses();

// Task priorities with metadata
const { data: taskPriorities, isLoading } = useTaskPriorities();

// Project types with metadata
const { data: projectTypes, isLoading } = useProjectTypes();
```

**Features:**
- Cached lookup data via TanStack Query
- Includes metadata (colors, icons, display order)
- Used for dropdowns, badges, and filters

#### 21.1.12 useTranslation

Custom translation hook with safe fallback:

```typescript
import { useTranslation } from '@/hooks/useTranslation';

const { t, safeT } = useTranslation('common');

// Standard usage
const text = t('buttons.save');

// Safe usage with fallback
const safeText = safeT('buttons.newKey', 'Save');
```

**Features:**
- Wraps react-i18next's useTranslation
- Provides `safeT` function that returns fallback if key not found
- Prevents displaying translation keys to users

#### 21.1.13 useLanguage

Hook for language management:

```typescript
import { useLanguage } from '@/contexts/LanguageContext';

const { language, changeLanguage, availableLanguages, isLoading } = useLanguage();
```

**Features:**
- Current language state
- Language switching with backend sync
- Available languages list
- Loading state during language change

#### 21.1.14 useDebouncedCallback

Hook for debouncing callback functions:

```typescript
const debouncedCallback = useDebouncedCallback((value: string) => {
  // Handle debounced value
}, 300);
```

#### 21.1.15 useRequestDeduplication

Hook for preventing duplicate API requests:

```typescript
const { deduplicateRequest } = useRequestDeduplication();

// Prevents multiple simultaneous requests with same key
const result = await deduplicateRequest('key', () => apiCall());
```

#### 21.1.16 useUserRole

Hook for getting user role in project:

```typescript
const { userRole, isOwner, isAdmin, isLoading } = useUserRole(projectId);
```

**Features:**
- Returns user's role in specific project
- Helper booleans for common checks
- Handles loading and error states

#### 21.1.17 usePermissions

Hook for global permission checking:

```typescript
const { hasPermission, hasAnyPermission, hasAllPermissions, isLoading } = usePermissions();

// Check single permission
if (hasPermission('projects.create')) {
  // Show create button
}

// Check multiple permissions
if (hasAllPermissions(['projects.edit', 'projects.delete'])) {
  // Show edit and delete buttons
}
```

**Features:**
- Global permission checking
- Project-scoped permission checking via `usePermissionsWithProject(projectId)`
- Cached permission data
- Loading and error states

### 21.2 Creating Custom Hooks

#### 21.2.1 Hook Pattern

```typescript
export function useCustomHook(param: string) {
  const [state, setState] = useState();
  
  useEffect(() => {
    // Side effects
  }, [param]);
  
  const handler = useCallback(() => {
    // Handler logic
  }, [dependencies]);
  
  return {
    state,
    handler,
    // ... other values
  };
}
```

---

## 23. Accessibility

### 22.1 Keyboard Navigation

- **Tab**: Navigate between interactive elements
- **Enter/Space**: Activate buttons/links
- **Escape**: Close modals/dialogs
- **Arrow Keys**: Navigate lists/menus
- **Ctrl/Cmd+K**: Open global search

### 22.2 ARIA Attributes

Components use proper ARIA attributes:
- `aria-label`: Descriptive labels
- `aria-labelledby`: Reference to label element
- `aria-describedby`: Reference to description
- `aria-expanded`: State of expandable elements
- `aria-hidden`: Hide decorative elements

### 22.3 Focus Management

- Focus trap in modals
- Focus return on modal close
- Visible focus indicators
- Skip links for main content

### 22.4 Screen Reader Support

- Semantic HTML elements
- Proper heading hierarchy (h1 → h2 → h3)
- Alt text for images
- Form labels associated with inputs
- Error messages associated with fields

### 22.5 Recent Accessibility Improvements

**Form Fields:**
- Added `id`, `name`, and `autocomplete` attributes to all form inputs
- Improved autofill support for login, registration, and password reset forms
- Enhanced form field accessibility in:
  - `Login.tsx`: Username and password fields
  - `AcceptInvite.tsx`: Email, username, password, and confirm password fields
  - `InviteUserDialog.tsx`: Email, first name, last name, and role fields
  - `CommentForm.tsx`: Comment textarea
  - `Header.tsx`: Global search input
  - `Tasks.tsx`: Task search input
  - `Projects.tsx`: Member search input

**Focus Management:**
- Fixed `aria-hidden` warning in Radix UI Tabs by preventing default focus behavior in `DialogContent`
- Added `onOpenAutoFocus` handler to `GlobalSearchModal` for proper focus management
- Improved focus trap in modal dialogs

**Dialog Components:**
- Enhanced `DialogContent` with focus management to prevent accessibility warnings
- Proper focus return on modal close

---

## 24. Performance Optimization

### 23.1 Code Splitting

#### 23.1.1 Route-Based Splitting

```typescript
import { lazy, Suspense } from 'react';

const AdminDashboard = lazy(() => import('./pages/admin/AdminDashboard'));

<Suspense fallback={<Loading />}>
  <AdminDashboard />
</Suspense>
```

#### 23.1.2 Component-Based Splitting

```typescript
const HeavyComponent = lazy(() => import('./HeavyComponent'));

{showHeavy && (
  <Suspense fallback={<Skeleton />}>
    <HeavyComponent />
  </Suspense>
)}
```

### 23.2 Image Optimization

- Use appropriate image formats (WebP, AVIF)
- Lazy load images
- Use responsive images
- Optimize image sizes

### 23.3 Bundle Optimization

- Tree shaking (automatic with Vite)
- Code splitting (automatic with Vite)
- Minification (production builds)
- Gzip/Brotli compression (server-side)

### 23.4 Caching Strategy

- **Static Assets**: Long-term caching with versioning
- **API Data**: TanStack Query caching (5 minutes default)
- **Browser Cache**: HTTP cache headers

---

## 25. Security

### 24.1 Authentication

- **Cookie-Based**: httpOnly cookies prevent XSS
- **CSRF Protection**: SameSite cookie attribute
- **Token Refresh**: Automatic token refresh
- **Session Management**: Logout clears all tokens

### 24.2 Input Validation

- **Client-Side**: Zod schema validation
- **Server-Side**: Backend validation (trust but verify)
- **Sanitization**: Prevent XSS attacks
- **Type Safety**: TypeScript prevents type errors

### 24.3 Content Security

- **CSP Headers**: Backend sets Content Security Policy
- **XSS Prevention**: React escapes by default
- **No eval()**: Avoid dynamic code execution

### 24.4 API Security

- **HTTPS Only**: In production
- **CORS**: Properly configured
- **Rate Limiting**: Backend rate limiting
- **Error Messages**: Don't expose sensitive info

---

## 26. Monitoring & Analytics

### 24.1 Error Tracking

#### 23.1.1 Sentry Integration

```typescript
// Automatic error tracking
Sentry.init({
  dsn: import.meta.env.VITE_SENTRY_DSN,
  environment: import.meta.env.VITE_SENTRY_ENVIRONMENT,
  integrations: [
    Sentry.browserTracingIntegration(),
    Sentry.replayIntegration(),
  ],
});
```

#### 23.1.2 Error Boundaries

```typescript
<Sentry.ErrorBoundary fallback={ErrorFallback}>
  <App />
</Sentry.ErrorBoundary>
```

### 24.2 Performance Monitoring

- **Sentry Performance**: Automatic performance tracking
- **Web Vitals**: Core Web Vitals monitoring
- **Lighthouse**: Performance audits

### 24.3 User Analytics

- **Session Replay**: Sentry Replay for debugging
- **User Context**: Sentry user context for errors
- **Custom Events**: Track user actions (if needed)

---

## 27. Future Improvements

### 24.1 Features

- [x] Complete Admin Settings implementation (General, Security, Email tabs)
- [x] User Detail Dialog with Projects and Activity History tabs
- [x] Last Login tracking and display in Admin Users table
- [x] Test email functionality for SMTP verification
- [x] Improved error handling in AdminDashboard
- [x] Type safety improvements (SystemHealthDto, ExternalServiceStatus)
- [ ] Real-time updates (WebSockets)
- [ ] Offline support (Service Workers)
- [ ] Progressive Web App (PWA)
- [ ] Advanced filtering and sorting
- [ ] Bulk operations
- [ ] Export functionality (PDF, Excel)
- [ ] File uploads
- [ ] Rich text editor
- [ ] Comments and mentions
- [ ] Activity timeline improvements

### 24.2 Performance

- [ ] Virtual scrolling for long lists
- [ ] Image lazy loading
- [ ] Route preloading
- [ ] Service Worker caching
- [ ] Bundle size optimization

### 24.3 Developer Experience

- [ ] Storybook for component documentation
- [ ] More comprehensive tests
- [ ] E2E tests (Playwright/Cypress)
- [ ] Component playground
- [ ] Design system documentation

---

## 29. Missing Features

Based on comprehensive audit (December 2024), the following frontend features are identified as missing or incomplete:

### 28.1 Missing UI Components

#### 28.1.1 Task Dependency Management

**Status:** ❌ **Not Implemented**

**What's Missing:**
- Dependency graph visualization component (using react-flow)
- Add dependency modal/form
- Dependency list display in TaskDetail
- Visual indicators for blocked tasks

**Estimated Implementation:** 2-3 days

#### 28.1.2 Milestone Management

**Status:** ❌ **Not Implemented**

**What's Missing:**
- Milestone card component
- Create/edit milestone modal
- Calendar view integration
- Gantt chart integration

**Estimated Implementation:** 1-2 days

#### 28.1.3 Release Management

**Status:** ❌ **Not Implemented**

**What's Missing:**
- Release card component
- Create/edit release modal
- Release notes view
- Quality gates display

**Estimated Implementation:** 1-2 days

#### 28.1.4 Test Case Management

**Status:** ❌ **Not Implemented**

**What's Missing:**
- Test case list component
- Test case form (create/edit)
- Test execution modal
- Test coverage widget
- Test execution history component

**Estimated Implementation:** 2-3 days

### 28.2 API Client Corrections Applied

#### 28.2.1 Comments API (v2.6)

**Corrections:**
- ✅ Changed `getAll()` from path parameters to query parameters
- ✅ Updated to use `URLSearchParams` for query string construction
- ✅ Endpoint: `/Comments?entityType={type}&entityId={id}`

#### 28.2.2 Attachments API (v2.6)

**Corrections:**
- ✅ Changed `getAll()` from path parameters to query parameters
- ✅ Removed `/download` suffix from download endpoint
- ✅ Endpoint: `/Attachments/{id}` (was `/Attachments/{id}/download`)

#### 28.2.3 Notifications API (v2.6)

**Corrections:**
- ✅ Changed `markAllAsRead()` from `POST` to `PATCH`
- ✅ Added fallback for `getUnreadCount()` using main endpoint

#### 28.2.4 Projects API (v2.6)

**Corrections:**
- ✅ Removed explicit `/api/v1` prefix in `assignTeam()`
- ✅ Uses relative path: `/Projects/{projectId}/assign-team`

#### 28.2.5 Auth API (v2.6)

**Corrections:**
- ✅ Added JSDoc `@deprecated` to `register()` method
- ✅ Message: "Public registration is disabled. Contact administrator for invitation link."

#### 28.2.6 Users API (v2.7)

**Corrections:**
- ✅ Limited `pageSize` to `100` (was `1000`) to match backend validation

#### 28.2.7 Teams API (v2.7)

**Status:** ✅ **Correctly Implemented**
- All endpoints use correct versioning
- Proper error handling

### 28.3 Frontend API Coverage

**Total API Clients:** 35 API clients (38 files including 3 test files) ✅ Verified

| API Client | Endpoints | Status | Issues |
|------------|-----------|--------|--------|
| `activity.ts` | 1 | ✅ | None |
| `admin.ts` | 2 | ✅ | None |
| `agents.ts` | 9 | ✅ | None |
| `aiGovernance.ts` | 5 | ✅ | None |
| `alerts.ts` | 2 | ✅ | None |
| `attachments.ts` | 4 | ✅ | Fixed (v2.6) |
| `auth.ts` | 4 | ✅ | Deprecated register() |
| `comments.ts` | 4 | ✅ | Fixed (v2.6) |
| `defects.ts` | 5 | ✅ | None |
| `insights.ts` | 1 | ✅ | None |
| `metrics.ts` | 4 | ✅ | None |
| `notifications.ts` | 4 | ✅ | Includes `/unread-count` endpoint |
| `adminAiQuota.ts` | 2 | ✅ | Admin member AI quota management |
| `adminAIQuotas.ts` | 1 | ✅ | Admin organization AI quotas list |
| `superAdminAIQuota.ts` | 3 | ✅ | SuperAdmin organization AI quota management |
| `organizations.ts` | 4 | ✅ | Organization management (Admin) |
| `organizationPermissionPolicy.ts` | 2 | ✅ | Organization permission policy (SuperAdmin) |
| `memberPermissions.ts` | 2 | ✅ | Member permissions management (Admin) |
| `memberService.ts` | 4 | ✅ | Project member operations |
| `milestones.ts` | 9 | ✅ | Milestone management |
| `releases.ts` | 17 | ✅ | Release management |
| `dependencies.ts` | 4 | ✅ | Task dependency management |
| `permissions.ts` | 4 | ✅ | Includes getMyPermissions, getProjectPermissions, getMatrix, updateRolePermissions |
| `projects.ts` | 12 | ✅ | Includes getAll, getById, create, update, archive, deletePermanent, getMembers, inviteMember, updateMemberRole, removeMember, assignTeam, getAssignedTeams |
| `search.ts` | 1 | ✅ | None |
| `settings.ts` | 3 | ✅ | None |
| `sprints.ts` | 7 | ✅ | None |
| `tasks.ts` | 8 | ✅ | None |
| `teams.ts` | 5 | ✅ | None |
| `users.ts` | 6 | ✅ | Fixed (v2.7) |
| `backlog.ts` | 4 | ✅ | None |
| `featureFlags.ts` | 2 | ✅ | None |
| `readModels.ts` | 3 | ✅ | None |

**Overall Coverage:** 100% (all endpoints implemented)

### 28.4 Accessibility Improvements

**Status:** ✅ **Applied (v2.7, v2.14.3)**

**Fixes:**
- ✅ Added `DialogTitle` to `GlobalSearchModal` (screen reader support)
- ✅ Added `DialogTitle` to `CommandDialog` (screen reader support)
- ✅ Added `SheetTitle` to `Sidebar` mobile view (screen reader support)
- ✅ Added `DialogDescription` to `AdminAIQuota` dialog (v2.14.3)
- ✅ All dialogs now properly include `DialogDescription` for screen readers
- ✅ Resolved Radix UI accessibility warnings for missing descriptions

**Remaining Issues:**
- ⚠️ Some components may need additional ARIA labels
- ⚠️ Keyboard navigation improvements needed for complex components

---

## 30. API Integration Status

### 29.1 Endpoint Matching

**Frontend vs Backend Match Rate:** ~98%

**Mismatches Fixed:**
- ✅ Comments API: Path params → Query params
- ✅ Attachments API: Path params → Query params, removed `/download`
- ✅ Notifications API: POST → PATCH for `markAllAsRead`
- ✅ Projects API: Removed explicit versioning
- ✅ Users API: Limited pageSize to 100

**Remaining Issues:**
- ⚠️ `GET /api/v1/Notifications/unread-count` - Not implemented (workaround available)

### 29.2 API Client Patterns

**Consistent Patterns:**
- ✅ All API clients use `apiClient` from `client.ts`
- ✅ Automatic versioning for `/api/v1/` routes
- ✅ Admin routes excluded from versioning (`/api/admin/...`)
- ✅ Automatic token refresh on 401 errors
- ✅ ETag caching for GET requests
- ✅ Error handling with proper error types

---

## 28. Contributing

### 26.1 Code Style

- Follow existing code patterns
- Use TypeScript for all new code
- Write tests for new features
- Update documentation
- Follow component naming conventions

### 26.2 Pull Request Process

1. Create feature branch
2. Implement changes
3. Write/update tests
4. Update documentation
5. Ensure all tests pass
6. Submit pull request

### 26.3 Component Guidelines

- Use shadcn/ui components when possible
- Follow accessibility best practices
- Make components responsive
- Support dark mode
- Write TypeScript interfaces
- Add JSDoc comments

---

## Appendix A: Component Library Reference

### A.1 shadcn/ui Components

All components located in `src/components/ui/`:

- **Layout**: Card, Separator, Scroll Area, Resizable
- **Forms**: Button, Input, Textarea, Select, Checkbox, Radio, Switch, Slider
- **Overlays**: Dialog, Sheet, Drawer, Popover, Tooltip, Hover Card
- **Navigation**: Tabs, Accordion, Navigation Menu, Breadcrumb, Pagination
- **Feedback**: SweetAlert2 (alerts and toasts), Alert, Progress, Skeleton, Badge
- **Data Display**: Table, Avatar, Command
- **Utilities**: Calendar, Carousel, Input OTP, Password Strength

### A.2 Custom Components

- **Layout**: MainLayout, AdminLayout, AppSidebar, AdminSidebar, Header
- **Projects**: ProjectCard, EditProjectDialog, MemberCard, RoleBadge, AssignTeamModal, ProjectTimeline, TeamMembersList, ProjectMembersModal
- **Tasks**: CreateTaskDialog, TaskDetailSheet, TaskListView, TaskBoard, StatusBadge, TaskFilters, TaskTimelineView, AITaskImproverDialog
- **Users**: UserCard, RoleBadge
- **Sprints**: StartSprintDialog, CompleteSprintDialog, AddTasksToSprintDialog
- **Defects**: CreateDefectDialog, DefectDetailSheet
- **Agents**: ProjectInsightPanel, RiskDetectionPanel, SprintPlanningAssistant
- **Admin**: InviteUserDialog, EditUserDialog, DeleteUserDialog, UserDetailDialog
- **Auth**: PasswordStrengthIndicator
- **Utilities**: GlobalSearchModal, NotificationDropdown, ErrorFallback, PermissionGuard, RequireAdminGuard
- **FeatureFlag**: FeatureFlag, FeatureFlagAll, FeatureFlagAny

---

## Appendix B: API Endpoint Reference

### B.1 Authentication

- `POST /api/v1/Auth/login` - Login
- `POST /api/v1/Auth/register` - Register (DEPRECATED - returns 403 Forbidden)
- `POST /api/v1/Auth/refresh` - Refresh token
- `GET /api/v1/Auth/me` - Get current user
- `POST /api/v1/Auth/logout` - Logout
- `GET /api/v1/Auth/invite/{token}` - Validate invitation token
- `POST /api/v1/Auth/invite/accept` - Accept organization invitation

### B.2 Admin Endpoints

**Note:** Admin endpoints use `/api/admin/...` route pattern without versioning in the URL.

- `POST /api/admin/users/invite` - Invite organization user (Admin only)
  - Request: `{ email, role, firstName, lastName }`
  - Response: `{ invitationId, email, invitationLink }`
- `GET /api/admin/dashboard/stats` - Get admin dashboard statistics (Admin only)
  - Response: `{ totalUsers, activeUsers, inactiveUsers, adminCount, userCount, totalProjects, activeProjects, totalOrganizations, userGrowth, recentActivities, systemHealth }`
- `GET /api/admin/feature-flags` - Get all feature flags (Admin only)
- `POST /api/admin/feature-flags` - Create feature flag (Admin only)
- `PUT /api/admin/feature-flags/{id}` - Update feature flag (Admin only)
- `GET /api/admin/permissions/members` - Get paginated list of organization members with permissions (Admin only)
  - Query Parameters: `page`, `pageSize`, `searchTerm`
  - Response: `PagedResponse<MemberPermissionDto>`
- `PUT /api/admin/permissions/members/{userId}` - Update member role and/or permissions (Admin only)
  - Request: `{ globalRole?: string, permissionIds?: number[] }`
  - Response: `MemberPermissionDto`
  - Enforces organization permission policy (assigned permissions must be subset of org allowed permissions)

### B.2.1 SuperAdmin Endpoints

**Note:** SuperAdmin endpoints use `/api/v1/superadmin/...` route pattern with versioning in the URL.

- `GET /api/v1/superadmin/organizations/{orgId}/permission-policy` - Get organization permission policy (SuperAdmin only)
  - Response: `OrganizationPermissionPolicyDto`
- `PUT /api/v1/superadmin/organizations/{orgId}/permission-policy` - Upsert organization permission policy (SuperAdmin only)
  - Request: `{ allowedPermissions: string[], isActive?: boolean }`
- `GET /api/v1/superadmin/organizations/{orgId}/ai-quota` - Get organization AI quota (SuperAdmin only)
  - Response: `OrganizationAIQuotaDto`
- `PUT /api/v1/superadmin/organizations/{orgId}/ai-quota` - Upsert organization AI quota (SuperAdmin only)
  - Request: `{ monthlyTokenLimit: number, monthlyRequestLimit?: number, resetDayOfMonth?: number, isAIEnabled?: boolean }`
- `GET /api/v1/superadmin/organizations/ai-quotas` - Get all organization AI quotas (SuperAdmin only, paginated)
  - Query Parameters: `page`, `pageSize`, `searchTerm`, `isAIEnabled`
  - Response: `PagedResponse<OrganizationAIQuotaDto>`

### B.2.2 Admin Organization Endpoints

- `GET /api/admin/organizations` - Get paginated list of organizations (SuperAdmin only)
  - Query Parameters: `page`, `pageSize`, `searchTerm`
  - Response: `PagedResponse<OrganizationDto>`
- `GET /api/admin/organizations/{orgId}` - Get organization by ID (SuperAdmin only)
  - Validates orgId > 0 (returns 400 BadRequest if invalid)
  - Handles ValidationException and returns appropriate error responses
  - Response: `OrganizationDto`
- `POST /api/admin/organizations` - Create organization (SuperAdmin only)
  - Request: `{ name: string, code: string }`
  - Response: `CreateOrganizationResponse`
- `PUT /api/admin/organizations/{orgId}` - Update organization (SuperAdmin only)
  - Request: `{ organizationId: number, name: string, code: string }`
  - Response: `UpdateOrganizationResponse`
- `DELETE /api/admin/organizations/{orgId}` - Delete organization (SuperAdmin only)
  - Response: `DeleteOrganizationResponse`

### B.2.3 Admin AI Quota Endpoints

- `GET /api/admin/ai-quota/members` - Get paginated list of organization members with AI quota (Admin/SuperAdmin)
  - Query Parameters: `organizationId?` (SuperAdmin only, optional), `page`, `pageSize`, `searchTerm`
  - Response: `PagedResponse<AdminAiQuotaMemberDto>`
  - Note: SuperAdmin can filter by organizationId (or view all if null), Admin uses their own organization (organizationId ignored)
- `GET /api/admin/ai-quota/ai-quotas/members` - Get paginated list of members with effective AI quotas (new model)
  - Query Parameters: `page`, `pageSize`, `searchTerm`
  - Response: `PagedResponse<MemberAIQuotaDto>`
- `PUT /api/admin/ai-quota/members/{userId}` - Update user AI quota override (Admin only)
  - Request: `UpdateMemberQuotaRequest`
  - Response: `UpdateMemberQuotaResponse`
- `POST /api/admin/ai-quota/members/{userId}/reset` - Reset user AI quota override (Admin only)
  - Response: `ResetMemberQuotaResponse`
- `PUT /api/admin/ai-quota/ai-quotas/members/{userId}` - Update member AI quota (new model, Admin only)
  - Request: `UpdateMemberAIQuotaRequest`
  - Response: `MemberAIQuotaDto`

### B.3 Projects

- `GET /api/v1/Projects` - List projects (paginated)
- `GET /api/v1/Projects/{id}` - Get project
- `POST /api/v1/Projects` - Create project
- `PUT /api/v1/Projects/{id}` - Update project
- `DELETE /api/v1/Projects/{id}` - Archive project (soft delete)
- `DELETE /api/v1/Projects/{id}/permanent` - Permanently delete project
- `GET /api/v1/Projects/{id}/members` - Get project members
- `POST /api/v1/Projects/{id}/members` - Invite member to project
- `PUT /api/v1/Projects/{id}/members/{userId}/role` - Change member role
- `DELETE /api/v1/Projects/{id}/members/{userId}` - Remove member from project
- `GET /api/v1/Projects/{id}/my-role` - Get current user's role in project
- `GET /api/v1/Projects/{id}/permissions` - Get current user's permissions for a project
  - Response: `ProjectPermissionsResponse` with permissions array, project role, and project ID
  - Returns 404 if user is not a member of the project
- `GET /api/v1/Projects/{id}/dependency-graph` - Get complete dependency graph for a project
  - Response: `DependencyGraphDto` with nodes (tasks) and edges (dependencies)
- `POST /api/v1/Projects/{id}/assign-team` - Assign team to project

### B.4 Tasks

- `GET /api/v1/Tasks?projectId={id}` - List tasks
- `GET /api/v1/Tasks/{id}` - Get task
- `POST /api/v1/Tasks` - Create task
- `PUT /api/v1/Tasks/{id}` - Update task
- `PATCH /api/v1/Tasks/{id}/status` - Change status
- `PATCH /api/v1/Tasks/{id}/assign` - Assign task

### B.4 Sprints

- `GET /api/v1/Sprints?projectId={id}` - List sprints
- `GET /api/v1/Sprints/{id}` - Get sprint
- `POST /api/v1/Sprints` - Create sprint
- `PATCH /api/v1/Sprints/{id}/start` - Start sprint
- `PATCH /api/v1/Sprints/{id}/complete` - Complete sprint

### B.5 Agents

- `POST /api/v1/Agents/analyze-project/{projectId}` - Analyze project
- `POST /api/v1/Agents/detect-risks/{projectId}` - Detect risks
- `POST /api/v1/Agents/plan-sprint/{sprintId}` - Plan sprint
- `GET /api/v1/Agents/metrics` - Get agent metrics
- `GET /api/v1/Agents/audit-logs` - Get audit logs

### B.6 Lookups (Reference Data)

- `GET /api/v1/Lookups/project-types` - Get all project types with metadata
  - Response: `LookupResponse` with items containing value, label, displayOrder, and metadata (color, icon)
- `GET /api/v1/Lookups/task-statuses` - Get all task statuses with metadata
  - Response: `LookupResponse` with items containing value, label, displayOrder, and metadata (color, bgColor, textColor)
- `GET /api/v1/Lookups/task-priorities` - Get all task priorities with metadata
  - Response: `LookupResponse` with items containing value, label, displayOrder, and metadata (color, bgColor, textColor)

### B.7 Settings

- `GET /api/v1/Settings?category={category}` - Get global settings (optionally filtered by category)
- `PUT /api/v1/Settings/{key}` - Update a global setting
  - Request: `{ value, category }`
- `POST /api/v1/Settings/test-email` - Send test email (Admin only)
  - Request: `{ email }`
  - Response: `{ success, message }`

### B.7 User Management

- `GET /api/v1/Users/{id}/projects` - Get user's projects (Admin only)
  - Query Parameters: `page`, `pageSize`
  - Response: `PagedResponse<ProjectListDto>`
- `GET /api/v1/Users/{id}/activity` - Get user's recent activity (Admin only)
  - Query Parameters: `limit`
  - Response: `{ activities: Activity[] }`

### B.8 Comments

- `GET /api/v1/Comments?entityType={type}&entityId={id}` - Get comments for an entity
- `POST /api/v1/Comments` - Add a comment
- `PUT /api/v1/Comments/{id}` - Update a comment
- `DELETE /api/v1/Comments/{id}` - Delete a comment

### B.9 Attachments

- `GET /api/v1/Attachments?entityType={type}&entityId={id}` - Get attachments for an entity
- `POST /api/v1/Attachments/upload` - Upload a file attachment
- `GET /api/v1/Attachments/{id}` - Download an attachment
- `DELETE /api/v1/Attachments/{id}` - Delete an attachment

### B.10 AI Governance

#### User Endpoints

- `GET /api/v1/ai/decisions` - Get AI decision logs for current organization
- `GET /api/v1/ai/quota` - Get AI quota status for current organization
- `GET /api/v1/ai/usage-statistics` - Get AI usage statistics

#### Admin Endpoints

- `GET /api/admin/ai/decisions` - Get all AI decision logs (Admin only)
- `GET /api/admin/ai/quotas` - Get all AI quotas (Admin only)
- `PUT /api/admin/ai/quota/{organizationId}` - Update AI quota (Admin only)
- `POST /api/admin/ai/disable/{organizationId}` - Disable AI for organization (Admin only)
- `POST /api/admin/ai/enable/{organizationId}` - Enable AI for organization (Admin only)
- `GET /api/admin/ai/decisions/export` - Export AI decisions to CSV (Admin only)

### B.12 Read Models

- `GET /api/v1/read-models/task-board/{projectId}` - Get task board read model
  - Returns pre-grouped tasks by status (Todo, InProgress, Done)
  - Includes task counts, story points, and task summaries
- `GET /api/v1/read-models/sprint-summary/{sprintId}` - Get sprint summary read model
  - Returns pre-calculated sprint metrics (velocity, completion percentage, burndown data)
  - Includes task status breakdown and capacity utilization
- `GET /api/v1/read-models/project-overview/{projectId}` - Get project overview read model
  - Returns aggregated project metrics (tasks, sprints, defects, velocity trends)
  - Includes project health score and risk factors
- `GET /api/v1/read-models/project-overviews` - Get multiple project overviews (for dashboard)
  - Query parameters: `organizationId`, `status`, `page`, `pageSize`
  - Returns paginated list of project overviews
- `POST /api/admin/read-models/rebuild` - Rebuild read models (Admin only)

### B.13 Feature Flags

#### Public Endpoints (All Authenticated Users)

- `GET /api/v1/feature-flags?organizationId={id}` - Get all feature flags for current organization
- `GET /api/v1/feature-flags/{name}?organizationId={id}` - Get single feature flag by name

**Note:** These endpoints are accessible to all authenticated users (not admin-only) for the feature flag service to work throughout the application.

#### Admin Endpoints (Admin Only)

- `GET /api/admin/feature-flags?organizationId={id}` - Get all feature flags (admin management)
- `POST /api/admin/feature-flags` - Create feature flag
- `PUT /api/admin/feature-flags/{id}` - Update feature flag

---

## Appendix C: Environment Variables

### C.1 Required Variables

```env
VITE_API_BASE_URL=http://localhost:5001
```

### C.2 Optional Variables

```env
VITE_SENTRY_DSN=your-sentry-dsn
VITE_SENTRY_ENVIRONMENT=development
VITE_FEATURE_FLAGS_CACHE_TTL=300000  # Cache TTL in milliseconds (default: 5 minutes)
```

### C.3 Build Variables

Automatically set by Vite:
- `VITE_APP_VERSION`: From package.json
- `VITE_BUILD_DATE`: Build date in YYYY.MM.DD format

---

## Appendix D: Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl/Cmd + K` | Open global search |
| `Esc` | Close modal/dialog |
| `Tab` | Navigate forward |
| `Shift + Tab` | Navigate backward |
| `Enter` | Submit form / Activate button |
| `Space` | Activate button / Toggle checkbox |

---

## Appendix E: Browser Support

### E.1 Supported Browsers

- **Chrome**: Latest 2 versions
- **Firefox**: Latest 2 versions
- **Safari**: Latest 2 versions
- **Edge**: Latest 2 versions

### E.2 Polyfills

- Automatic polyfills via Vite
- Modern JavaScript features
- CSS Grid and Flexbox
- Fetch API

---

**Document Version:** 2.17.0  
**Last Updated:** January 7, 2025  
**Maintained By:** Development Team

---

## Changelog

### Version 2.17.0 (January 7, 2025) - Comprehensive Codebase Scan
- ✅ **Documentation Update**: Comprehensive codebase scan and verification
  - Verified all component counts: 172 component files (170 .tsx + 2 .ts) ✅
  - Verified all page counts: 51 pages total ✅
  - Verified all API client counts: 33 API clients (36 files including 3 test files) ✅
  - Verified all hook counts: 15 hooks (14 .ts + 1 .tsx) ✅
  - All counts verified against actual codebase files using glob_file_search
  - Updated version to 2.17.0
  - Updated "Last Updated" date to reflect comprehensive scan

### Version 2.15.0 (January 7, 2025)
- ✅ **Global API Error Handling**: Enhanced error handling with user-friendly messages
  - Added user-friendly error messages mapped to HTTP status codes:
    - `401`: "Session expired. Please log in again."
    - `403`: "You don't have permission for this action."
    - `429`: "Too many requests. Please try again later."
    - `500`: "Server error. Please contact support."
    - `502/503/504`: "Service temporarily unavailable. Please try again later."
  - Global error toast notifications using Sonner toast system
  - Automatic toast display for client errors (4xx) and server errors (5xx)
  - Sentry error logging for server errors (5xx) if configured
  - Dynamic Sentry import to avoid bundling if not configured
  - Improved error message extraction (prioritizes field-level validation errors)
  - Enhanced 401 handling with automatic token refresh before redirect
  - Better 429 rate limit handling with retry-after information
- ✅ **Configuration Documentation**: Created environment variables template
  - Created `frontend/.env.example` file with all required variables
  - Documented environment variable usage and requirements
  - Added configuration documentation to development setup section

### Version 2.14.5 (January 6, 2025)
- ✅ **ESLint Fix**: Fixed `@typescript-eslint/no-explicit-any` errors in `organizations.ts`
  - Replaced `PagedResponse<any>` with `PagedResponse<UserListDto>` in `getMembers()` function
  - Added import for `UserListDto` type from `@/api/users`
  - Fixed both return type annotation and `apiClient.get()` generic type parameter
  - All ESLint errors resolved - `npm run lint` passes successfully
- ✅ **Type Safety**: Improved type safety in organizations API client
  - `getMembers()` now properly typed with `UserListDto` instead of `any`
  - Consistent with other API clients using proper TypeScript types
  - Better IDE autocomplete and type checking support

### Version 2.14.4 (January 6, 2025)
- ✅ **Admin Organization Members Tenant Isolation Fix**: Fixed tenant isolation for Admin organization members page
  - Updated `AdminOrganizationMembers.tsx` to use `/api/v1/Users` endpoint instead of `/api/admin/organization/members`
  - `/api/v1/Users` uses `OrganizationScopingService` which properly handles tenant isolation:
    - **Admin**: Automatically filters to show only members of their organization
    - **SuperAdmin**: Shows all members from all organizations
  - Fixed `usersApi.getAllPaginated()` to map frontend sortField values to backend expected values
    - Added `mapSortFieldToBackend()` function to convert frontend values (`name`, `email`, `role`, `createdAt`, `status`) to backend values (`CreatedAt`, `Email`, `Role`, `IsActive`)
    - Fixed 400 Bad Request errors when sorting by `name` (now maps to `CreatedAt`)
  - Updated `AdminOrganizationMembers.tsx` to use `UserListDto` type from `@/api/users` instead of local interface
  - Changed `user.role` to `user.globalRole` to match API response structure
  - Admin users now correctly see only their organization's members
  - SuperAdmin users correctly see all members from all organizations

### Version 2.14.3 (January 5, 2025)
- ✅ **401 Error Handling Improvements**: Enhanced error handling for expired tokens
  - Updated `NotificationBell` component to conditionally disable `refetchOnWindowFocus` based on authentication status
  - Added `refetchOnWindowFocus: isAuthenticated && !isAuthLoading` to both notification queries
  - Improved retry logic to properly detect 401 errors (checks for both "Unauthorized" and "401" in error messages)
  - Fixed 401 errors on `/api/v1/Notifications` and `/api/v1/Notifications/unread-count` endpoints during window focus refetch
  - Queries now automatically disable when `isAuthenticated` becomes `false` (via `auth:failed` event)
- ✅ **Feature Flags Service**: Improved 401 error handling
  - Updated `featureFlagService.fetchAllFlagsFromAPI()` to re-throw 401 errors instead of catching them silently
  - Updated `featureFlagService.getAllFlags()` to re-throw 401 errors
  - Allows API client to handle token refresh automatically
  - Updated `FeatureFlagsContext` to not set error state for 401 errors (API client handles authentication)
  - Fixed 401 errors on `/api/admin/feature-flags` endpoint
- ✅ **Dialog Accessibility Fix**: Fixed missing DialogDescription warning
  - Added `DialogDescription` component to `AdminAIQuota` dialog
  - Resolves Radix UI accessibility warning: "Missing `Description` or `aria-describedby={undefined}` for {DialogContent}"
  - All dialogs now properly include descriptions for screen readers
  - Improved accessibility compliance across the application

### Version 2.14.2 (January 5, 2025)
- ✅ **SuperAdmin Route Fix**: Fixed SuperAdmin API routes to use proper versioning
  - Updated frontend API client to properly transform `/api/superadmin/...` to `/api/v1/superadmin/...`
  - All SuperAdmin endpoints now correctly use versioned routes: `/api/v1/superadmin/organizations/...`
  - Fixed 404 errors on SuperAdmin endpoints (`/api/v1/superadmin/organizations/{orgId}/ai-quota`, `/api/v1/superadmin/organizations/{orgId}/permission-policy`)
  - Updated API client documentation to reflect versioned SuperAdmin routes
- ✅ **Documentation Update**: Added SuperAdmin AI Quota endpoints documentation
  - Documented `GET /api/v1/superadmin/organizations/{orgId}/ai-quota`
  - Documented `PUT /api/v1/superadmin/organizations/{orgId}/ai-quota`
  - Documented `GET /api/v1/superadmin/organizations/ai-quotas`
  - Updated "Last Updated" date to January 5, 2025

### Version 2.14.1 (January 4, 2025)
- ✅ **TypeScript Compilation Fixes**: Resolved all 28 TypeScript compilation errors
  - Fixed `PagedResponse` import in `adminAiQuota.ts` (import from `./projects` instead of `./client`)
  - Fixed `GlobalRole` import in `AuthContext.tsx`
  - Fixed `InviteUserDialog` role type to exclude `SuperAdmin` (Admin can only assign User or Admin roles)
  - Fixed dialog ref readonly issue in `dialog.tsx` using `React.MutableRefObject` cast
  - Removed unused imports and variables across multiple files (Separator, Shield, Users, Building2, editingOrg, setEditingOrg, showToast)
  - Fixed `totalPages` possibly undefined issues with null coalescing operators (`??`) in pagination components
  - Fixed `RoleMap` to include `SuperAdmin` role in `AdminPermissions.tsx` (prevents TypeScript errors)
  - Fixed possibly undefined properties in `AdminMemberAIQuotas.tsx` and `SuperAdminOrganizationAIQuota.tsx`
  - All TypeScript compilation errors resolved - `npm run type-check` passes successfully
- ✅ **API Client Fixes**: Fixed critical API integration bugs
  - Fixed `memberPermissionsApi.getMemberPermissions()` to use `URLSearchParams` for query parameters (was passing object incorrectly)
  - Fixed API endpoint paths to include `/api` prefix in `memberPermissions.ts` and `organizationPermissionPolicy.ts`
  - Endpoints now correctly route to `/api/admin/permissions/members` and `/api/v1/superadmin/organizations/{orgId}/permission-policy`
  - SuperAdmin routes now use versioned URLs: `/api/v1/superadmin/organizations/...`
- ✅ **Documentation Update**: Comprehensive codebase scan and documentation refresh
  - Updated API client count: 27 → 31 files (added organizationPermissionPolicy, memberPermissions, adminAiQuota, adminAIQuotas, superAdminAIQuota, organizations)
  - Updated admin pages count: 8 → 13 pages (added AdminAIQuota, AdminOrganizations, AdminOrganizationDetail, AdminMyOrganization, AdminOrganizationMembers, AdminMemberAIQuotas, AdminMemberPermissions)
  - Updated "Last Updated" date to January 4, 2025
  - Verified all routes, pages, and API clients are accurately documented

### Version 2.14.0 (January 2, 2025)
- ✅ **Organization Permission Policy Management**: Complete permission policy system with two-level UI
  - **SuperAdmin Level**: Manage allowed permissions per organization via `/admin/organizations/:orgId/permissions`
    - Checklist/matrix of all system permissions grouped by category
    - Search functionality to filter permissions
    - Select All / Deselect All buttons
    - Policy activation toggle (active = restrict to selected, inactive = allow all)
    - Policy information display (created/updated timestamps)
    - Upsert operation (create if not exists, update if exists)
  - **Admin Level**: Manage member roles/permissions within own organization via `/admin/permissions/members`
    - Paginated member list with search functionality
    - Edit modal for updating member roles and permissions
    - Automatic filtering: only permissions allowed by organization policy are available
    - Role-based permission derivation with policy enforcement
    - Tenant isolation: Admin can only manage members in their own organization
  - **New API Clients**: 
    - `organizationPermissionPolicy.ts`: Organization permission policy API (SuperAdmin only)
    - `memberPermissions.ts`: Member permissions management API (Admin only)
  - **New Pages**:
    - `SuperAdminOrganizationPermissions.tsx`: SuperAdmin permission policy management page
    - `AdminMemberPermissions.tsx`: Admin member permissions management page
  - **Routes Added**:
    - `/admin/organizations/:orgId/permissions` (SuperAdmin only, protected by RequireSuperAdminGuard)
    - `/admin/permissions/members` (Admin only)
  - **Navigation Updates**:
    - Added "Member Permissions" link to AdminSidebar for Admin users
    - Added "Permissions" button to AdminOrganizationDetail page for SuperAdmin users
  - **UI Features**:
    - Permission filtering based on organization policy
    - Visual indicators for allowed/disallowed permissions
    - Policy-aware permission selection
    - Role change automatically updates permissions (filtered by policy)
  - **Security**:
    - SuperAdmin can manage policies for any organization
    - Admin can only manage members in their own organization
    - Policy enforcement prevents assigning disallowed permissions
    - Default behavior: if no policy exists, all permissions are allowed

### Version 2.13.0 (January 2, 2025)
- ✅ **Billing System Removal**: Removed all billing/subscription/plan features from frontend
  - Removed all `/settings/billing` route references and navigation
  - Removed "Upgrade Plan" buttons from QuotaStatusWidget, QuotaAlertBanner, and QuotaDetails pages
  - Removed "Plan Comparison" section from QuotaDetails page
  - Removed `upgradeUrl` from QuotaErrorDetails interface
  - Updated QuotaExceededAlert to navigate to `/settings/ai-quota` instead of billing
  - Updated error messages to direct users to contact administrators instead of upgrade prompts
  - Updated placeholder text in AIQuotasList from "billing issues" to "quota exceeded"
  - No regressions: AI quota functionality remains fully intact
- ✅ **Dialog Component Fix**: Fixed dialog component to prevent body scroll lock issues
  - Updated dialog.tsx to properly handle scroll lock on open/close
  - Prevents body scroll when dialog is open
  - Restores scroll when dialog is closed
- ✅ **AdminSettings Fix**: Fixed dialog usage in AdminSettings page
  - Updated to use proper dialog component pattern
  - Improved error handling and user feedback

### Version 2.12.0 (January 2, 2025)
- ✅ **Development Tools Organization**: Moved test code to `/dev` folder
  - Moved `ReleaseApiTest.tsx` from `pages/test/` to `src/dev/` folder
  - Created `dev/README.md` with documentation for dev tools
  - Updated `vite.config.ts` to exclude `/dev` folder from test coverage
  - Dev tools are automatically excluded from production builds (not imported in production code)
  - Removed empty `pages/test/` directory
- ✅ **Production Build Safety**: Ensured test code is not included in production
  - ReleaseApiTest is not routed in App.tsx (never imported)
  - Vite automatically excludes unused files from production builds
  - Added comments in vite.config.ts documenting exclusion behavior
- ✅ **Documentation Updates**:
  - Updated project structure to reflect `/dev` folder organization
  - Added note about dev tools being excluded from production builds
  - Updated changelog with development tools organization changes

### Version 2.11.0 (January 1, 2025)
- ✅ **New AI Components**: Created 5 new AI-powered components
  - TaskImproverDialog: AI task improvement for existing tasks
  - ProjectAnalysisPanel: Comprehensive project analysis with health status
  - RiskDetectionDashboard: Interactive risk detection dashboard
  - SprintPlanningAI: Intelligent sprint planning with capacity analysis
  - DependencyAnalyzerPanel: Task dependency analysis with circular dependency detection
  - ⚠️ **Note**: All 5 components are created but not yet integrated into pages
- ✅ **Statistics Update**: Updated documentation with actual implementation counts
  - 51 pages total ✅ Verified
  - 170 components total ✅ Verified
  - 36 API clients (excluding test files) ✅ Verified
  - 14 hooks (including custom hooks) ✅ Verified
- ✅ **Route Configuration**: QuotaDetails page route now configured in App.tsx
- ⚠️ **Integration Pending**: 5 new AI components need integration into pages
  - TaskImproverDialog → TaskDetailSheet
  - ProjectAnalysisPanel → ProjectDetail (new "AI Analysis" tab)
  - RiskDetectionDashboard → Insights page or ProjectDetail
  - SprintPlanningAI → StartSprintDialog or Sprints page
  - DependencyAnalyzerPanel → Tasks page or ProjectDetail (new "Dependencies" tab)

### Version 2.10.0 (January 1, 2025)
- ✅ **AI Agent Results Display**: Structured display components for agent outputs
  - Created AgentResultsDisplay wrapper component with type-specific routing
  - Created 5 result components: ProductAgentResults, QAAgentResults, BusinessAgentResults, ManagerAgentResults, DeliveryAgentResults
  - ProductAgentResults: Sortable table with prioritized items and confidence scores
  - QAAgentResults: Defect patterns with bar charts and severity badges
  - BusinessAgentResults: Value metrics with progress bars and trend indicators
  - ManagerAgentResults: Executive summary with key decisions checklist
  - DeliveryAgentResults: Milestones, risks, and action items display
  - All components support loading states, error fallback, and JSON parsing
  - Integrated into Agents.tsx page
- ✅ **AI Governance Components**: Quota management UI components
  - Created QuotaStatusWidget for displaying quota status (compact and full modes)
  - Created QuotaAlertBanner for threshold alerts (80%, 100%, Disabled)
  - Created QuotaExceededAlert and AIDisabledAlert components
  - Integrated QuotaStatusWidget into Agents.tsx and AppSidebar
  - QuotaStatusWidget supports auto-refresh every 60 seconds
  - Color-coded progress bars (green <50%, yellow 50-80%, red >80%)
  - Tier badges with appropriate styling
- ✅ **Quota Details Page**: Detailed quota usage page
  - Created QuotaDetails.tsx page component
  - Historical usage graphs (LineChart for 30 days)
  - Breakdown by agent type (BarChart)
  - Current quota status with detailed metrics
  - Note: Route `/settings/ai-quota` configured in App.tsx
- ✅ **AI Task Improver Dialog**: Dialog for improving tasks with AI
  - Created AITaskImproverDialog.tsx component
  - Uses `/api/v1/Agent/improve-task` endpoint
  - Integrated into CreateTaskDialog
  - Supports editing improved suggestions before applying
  - Quality score badge with color coding (8-10 green, 6-8 yellow, <6 orange)
  - Acceptance criteria management (add/remove/edit)
  - Loading skeleton and error handling
  - QuotaAlertBanner integration
- ✅ **Quota Notifications Hook**: Automatic quota threshold notifications
  - Created useQuotaNotifications hook
  - Shows warning toast at 80% usage (once per session)
  - Shows error toast at 100% usage (once per session)
  - Auto-refreshes every 60 seconds
  - Integrated into Agents.tsx page
- ✅ **Type Definitions**: New TypeScript types
  - Created types/agents.ts with agent output interfaces
  - Created types/aiGovernance.ts with quota management interfaces
  - Added ImprovedTask interface in AITaskImproverDialog
- ✅ **API Client Updates**: Enhanced AI governance API client
  - Added getQuotaStatus method (currently mocked, ready for backend integration)
  - Existing methods: getAllDecisions, getAllQuotas, updateQuota, disableAI, enableAI, getOverviewStats
- ✅ **Documentation Updates**:
  - Updated component lists with new AI governance and agent result components
  - Added new hooks documentation
  - Updated API client documentation
  - Added new type definitions sections
  - Updated changelog

### Version 2.9.2 (December 30, 2024)
- ✅ **API Connectivity Testing**: Release API endpoint testing utility
  - Created `testReleaseApiConnectivity.ts` utility for testing all 17 Release API endpoints
  - Supports read-only and mutation test modes (safe vs full testing)
  - Color-coded console output with detailed error reporting and status codes
  - Available globally via `testReleaseApi()` function in browser console
  - Automatically loaded in development mode via `main.tsx`
  - Tests all endpoints: GET, POST, PUT, DELETE operations
  - Handles authentication, validation errors, and endpoint existence verification
- ✅ **Release API Test Page**: Visual UI for API connectivity testing
  - Created `ReleaseApiTest.tsx` page component using shadcn/ui components
  - Table view of all test results with status badges and icons
  - Summary statistics card showing success/failed/skipped counts
  - Alert messages for test outcomes
  - Accessible at `/test/release-api` route (requires route configuration)
- ✅ **Documentation Updates**:
  - Updated project structure to include test utility and test page
  - Added API connectivity testing to utils and pages sections
  - Updated changelog with new testing features

### Version 2.9.1 (December 30, 2024)
- ✅ **Documentation Updates**:
  - Updated Feature Pages list to include Releases and Milestones pages
  - Verified release components documentation (18 components)
  - Verified releases API client documentation
  - Updated version to 2.9.1

### Version 2.9 (December 29, 2024)
- ✅ **Milestones Feature**: Complete milestone management UI
  - Created milestone components (8 components): CreateMilestoneDialog, EditMilestoneDialog, CompleteMilestoneDialog, MilestoneCard, MilestonesList, MilestoneStatistics, MilestoneTimeline, NextMilestone
  - Added `milestones.ts` API client with full CRUD operations
  - Integrated milestone management into ProjectDetail page
  - Milestone statistics and timeline views
  - Next milestone widget for dashboard
- ✅ **Releases Feature**: Complete release management UI
  - Created release components (18 components): CreateReleaseDialog, EditReleaseDialog, DeployReleaseDialog, ReleaseCard, ReleasesList, ReleaseStatistics, ReleaseTimeline, ReleaseNotesEditor, ReleaseNotesViewer, QualityGatesPanel, QualityGateWidget, QualityTrendChart, ReleaseHealthDashboard, BlockedReleasesWidget, NextReleaseWidget, PendingApprovalsWidget, DeploymentFrequencyChart, SprintSelectorDialog
  - Added `releases.ts` API client with full release management operations
  - Created ReleaseDetailPage and ReleaseHealthDashboard pages
  - Quality gates visualization and management
  - Release notes editor with Markdown support
  - Release health dashboard with metrics and charts
- ✅ **Task Dependencies**: Task dependency management UI
  - Created dependency components: TaskDependenciesList, AddDependencyDialog, DependencyGraph, BlockedBadge
  - Added `dependencies.ts` API client for dependency management
  - Created hooks: `useTaskDependencies`, `useProjectTaskDependencies`
  - Visual dependency graph using React Flow
  - Dependency management integrated into TaskDetailSheet
- ✅ **Accessibility Improvements**: Enhanced form accessibility
  - Added `id` and `name` attributes to all form fields
  - Fixed `htmlFor` associations for labels
  - Added `autocomplete` attributes where appropriate
  - Fixed React warnings about controlled/uncontrolled components
- ✅ **Authentication Improvements**: Enhanced authentication handling
  - Fixed 401 errors in NotificationBell and RecentActivity components
  - Added authentication checks to prevent API calls when not authenticated
  - Conditional query enabling based on authentication status
- ✅ **Documentation Updates**: 
  - Updated API client list with new modules (milestones, releases, dependencies)
  - Updated component list with milestone and release components
  - Updated pages list with new release pages
  - Updated changelog with all new features

### Version 2.8 (December 19, 2024)
- ✅ **API Audit**: Comprehensive audit of all frontend API clients vs backend endpoints
- ✅ **API Client Corrections**: Fixed Comments, Attachments, Notifications, Projects, Users APIs
- ✅ **Accessibility**: Added DialogTitle/SheetTitle for screen reader support
- ✅ **Missing Features Documentation**: Added section for missing UI components
- ✅ **API Integration Status**: Documented endpoint matching and patterns
- ✅ **Documentation Updates**: 
  - Updated API client coverage statistics
  - Documented all corrections applied
  - Added missing features roadmap

### Version 2.7 (December 26, 2024)
- ✅ **API Client Improvements**: Enhanced error handling and token refresh logic
  - Automatic token refresh on 401 errors with retry logic
  - Improved rate limit handling with `Retry-After` header parsing
  - Better field-level validation error extraction
  - ETag caching support for GET requests (304 Not Modified)
  - Request/response normalization for PascalCase/camelCase compatibility
- ✅ **Projects API**: Added missing endpoints
  - `deletePermanent`: Permanently delete a project (with confirmation dialog)
  - `getUserRole`: Get current user's role in a project (via `memberService`)
- ✅ **Member Service**: New API client module for project member operations
  - `getMembers`: Get all project members
  - `inviteMember`: Invite a member to a project
  - `changeRole`: Change a member's role
  - `removeMember`: Remove a member from a project
  - `getUserRole`: Get current user's role in a project
- ✅ **Read Models Service**: New service for CQRS read models
  - `getTaskBoard`: Get task board read model with pre-grouped tasks
  - `getSprintSummary`: Get sprint summary with pre-calculated metrics
  - `getProjectOverview`: Get project overview with aggregated metrics
  - `getProjectOverviews`: Get multiple project overviews (for dashboard)
  - Handles both PascalCase and camelCase response formats
- ✅ **Read Models Hooks**: New hooks for accessing read models
  - `useTaskBoard`: Hook for task board read model
  - `useSprintSummary`: Hook for sprint summary read model
  - `useProjectOverview`: Hook for project overview read model
  - `useProjectOverviews`: Hook for multiple project overviews
- ✅ **AppSidebar**: Added "Admin Dashboard" button for admin users
  - Visible only to users with Admin role
  - Located in sidebar footer
  - Displays app version and build date
- ✅ **Delete Project Dialog**: Enhanced permanent deletion flow
  - Requires typing "DELETE" to confirm
  - Clear warning about permanent data loss
  - Improved error handling with detailed messages
- ✅ **Accessibility Improvements**: Comprehensive form field enhancements
  - Added `id`, `name`, and `autocomplete` attributes to all form inputs
  - Improved autofill support across all forms
  - Fixed `aria-hidden` warning in Radix UI Tabs
  - Enhanced focus management in modal dialogs
  - Better screen reader support
- ✅ **Error Handling**: Improved error messages and user feedback
  - Better extraction of backend validation errors
  - Field-level error messages for form validation
  - Improved error display in `TeamMembersList` component
  - Enhanced error handling in `DeleteProjectDialog`
- 📝 **Documentation**: Updated API reference, component documentation, and added new services/hooks sections

### Version 2.6 (December 25, 2024)
- ✅ **Comment System**: Complete comment system with threading support
  - Comment entity with polymorphic relationships (Task, Project, Sprint, Defect)
  - CommentSection, CommentForm, CommentItem components
  - @username mention parsing and highlighting
  - CommentAddedEvent, CommentUpdatedEvent, CommentDeletedEvent domain events
  - UserMentionedEvent for mention notifications
- ✅ **Mention System**: User mention tracking and notifications
  - Mention entity with position tracking
  - MentionParser service for parsing @username mentions
  - Automatic notification creation for mentioned users
  - Notification preference checking before sending
- ✅ **Notification Preferences**: User-configurable notification settings
  - NotificationPreference entity per user and notification type
  - Email, in-app, and push channel preferences
  - Frequency settings (instant, daily, weekly, never)
  - Default preferences initialization for new users
- ✅ **File Attachments**: File upload and download system
  - Attachment entity with polymorphic relationships
  - AttachmentUpload component with drag-and-drop
  - AttachmentList component with download/delete
  - File validation (size, type, extension)
  - LocalFileStorageService for file storage
- ✅ **AI Governance**: Complete AI decision logging and quota management
  - AIDecisionLog entity for decision tracking
  - AIQuota entity for usage tracking per organization
  - AIGovernance page with overview, decisions, and quotas tabs
  - Admin endpoints for quota management and kill switch
  - User endpoints for viewing decisions and quota status
- ✅ **Read Models**: CQRS read models for optimized queries
  - TaskBoardReadModel, SprintSummaryReadModel, ProjectOverviewReadModel
  - ReadModelsController for accessing read model data
  - Admin endpoint for rebuilding read models
- ✅ **Notification Bell**: Real-time notification badge component
  - Badge showing unread notification count
  - NotificationDropdown for viewing notifications
  - Integrated into Header component
- ✅ **API Clients**: New API client modules
  - comments.ts: Comment management API
  - attachments.ts: File attachment API
  - aiGovernance.ts: AI governance API
  - Updated notifications.ts with preferences support
- 📝 **Documentation**: Updated both backend and frontend documentation with all new features

### Version 2.5 (December 25, 2024)
- ✅ **SweetAlert2 Migration**: Complete migration from sonner and native alerts to SweetAlert2
  - Replaced all `toast()` calls from sonner with `showToast()`, `showSuccess()`, `showError()`, `showWarning()`
  - Replaced all `window.alert()`, `window.confirm()`, `window.prompt()` with SweetAlert2 equivalents
  - Created comprehensive SweetAlert2 wrapper utility (`src/lib/sweetalert.ts`)
  - Migrated 38 files to use SweetAlert2 consistently
  - Removed Toaster and Sonner components from App.tsx
  - All notifications now use consistent SweetAlert2 UI with theme support
- ✅ **TaskBoard Component**: Kanban-style board with drag-and-drop functionality
  - Integrated react-beautiful-dnd for smooth drag-and-drop experience
  - Three columns: Todo, In Progress, Done
  - Drag-and-drop between columns triggers status change
  - Task cards display priority, assignee, story points, and due dates
  - Optimistic updates with automatic rollback on error
  - Mobile-responsive (drag disabled on screens < 768px)
  - Loading skeletons and empty states
  - Visual feedback during drag (shadow, rotation)
  - Drop zone highlighting
- ✅ **StatusBadge Component**: Reusable badge component for task status display
  - Supports all task statuses (Todo, InProgress, Done, Blocked)
  - Three size variants: sm, md, lg
  - Three visual variants: default (filled), outline, dot
  - Icon support with toggle option
  - Dark mode support via Tailwind CSS
  - Accessible (role="status", aria-label)
  - Memoized with React.memo for performance
  - Utility functions: `getStatusColor()`, `getStatusLabel()`, `getStatusIcon()`
  - Consistent color scheme across the application
- ✅ **UserCard Component**: Reusable user card display component
  - Displays user avatar, name, email, role, status
  - Shows project count, join date, last login
  - Supports click navigation to user details
  - Optional action buttons (Edit, Delete, View)
  - Inactive user indicator (grayscale filter)
  - Keyboard accessible
- ✅ **RoleBadge Component**: Role badge component for users
  - Displays Admin/User roles with icons (Shield for Admin, User for User)
  - Multiple size variants (sm, md, lg)
  - Color-coded (Admin: red, User: blue)
  - Consistent styling across the application
- ✅ **Users Page**: Read-only user list page for non-admin users
  - Grid layout with UserCard components
  - Search by name or email (debounced)
  - Filter by role (Admin/User) and status (Active/Inactive)
  - Pagination support
  - Loading skeletons and empty states
  - Error handling with SweetAlert2
- ✅ **ProjectCard Component**: Reusable project card display component
  - Displays project name, type, status, description
  - Shows member count, task progress, current sprint
  - Owner information with avatar
  - Last updated timestamp
  - Supports `default` and `compact` variants
  - Optional action buttons (Edit, Delete, View)
  - Click navigation to project details
- ✅ **AssignTeamModal Component**: Modal for assigning teams to projects
  - Multi-select for teams with search
  - Default role selection for team members
  - Advanced mode for individual member role overrides
  - Displays team members with avatars
  - Confirmation for multiple team assignments
  - Loading states and error handling
- ✅ **AdminDashboard**: Fixed all linting errors and improved error handling
  - Removed unused imports (Cpu, HardDrive, Database, XCircle)
  - Fixed unused variable in pie chart map
  - Added safe date formatting with try-catch
  - Added type checks for SystemHealth properties
  - Added empty state messages for charts
  - Improved key generation for activity list items
- ✅ **AdminSettings**: Complete implementation of all settings tabs
  - **General Tab**: Application name, timezone, language, date format, project creation permissions
  - **Security Tab**: Token expiration, password policy (min length, uppercase, lowercase, digits, special chars), max login attempts, session duration, 2FA requirement
  - **Email Tab**: SMTP configuration (host, port, username, password, SSL/TLS, from email/name) with test email functionality
  - **Feature Flags Tab**: Feature flag management
- ✅ **UserDetailDialog**: Complete implementation with three tabs
  - **Overview Tab**: User information display
  - **Projects Tab**: Paginated list of user's projects (fetches from `GET /api/v1/Users/{id}/projects`)
  - **Activity History Tab**: Recent user activities (fetches from `GET /api/v1/Users/{id}/activity`)
- ✅ **AdminUsers**: Added LastLoginAt column to user table
  - Displays formatted last login timestamp or "Never" if never logged in
  - Added state management for UserDetailDialog
- ✅ **API Integration**: Added new endpoints
  - `getUserProjects`: Fetches user's projects with pagination
  - `getUserActivity`: Fetches user's recent activity
  - `sendTestEmail`: Sends test email to verify SMTP configuration
- ✅ **Type Definitions**: Added missing interfaces
  - `SystemHealthDto`: Complete interface with all properties
  - `ExternalServiceStatus`: Interface for external service status
- ✅ **UserListDto**: Added `lastLoginAt` field to interface
- 📝 **Documentation**: Updated API reference, component documentation, and added SweetAlert2 integration guide

### Version 2.19.0 (January 8, 2025) - Comprehensive Codebase Scan & Login Design Migration
- ✅ **Documentation Update**: Comprehensive codebase scan and verification
  - Verified all component counts: 163 component files (excluding test files) ✅
  - Verified all page counts: 44 pages (excluding test files) ✅
  - Verified all API client counts: 35 API clients (38 files including 3 test files) ✅
  - Verified all hook counts: 15 hooks (14 .ts + 1 .tsx) ✅
  - Verified all context counts: 7 files (5 .tsx + 2 .tsx test files) ✅
  - All counts verified against actual codebase files using PowerShell file system scanning
  - Updated version to 2.19.0
  - Updated "Last Updated" date to reflect comprehensive scan
- ✅ **Login Page Design Migration**: Complete redesign of login page with modern split-screen layout
  - **New Components Created**:
    - `Logo.tsx`: Reusable logo component with light/dark variants and size options (sm, md, lg)
    - `GeometricShapes.tsx`: Animated decorative shapes component for visual background
    - `LoginForm.tsx`: Standalone login form component with full authentication logic integration
  - **Login Page Refactored**:
    - Split-screen design: gradient panel (left) + login form (right)
    - Responsive layout: mobile shows logo above form, desktop shows split-screen
    - Animated elements: fade-in, slide-in-right, float animations
    - Preserved all authentication logic: useAuth, authApi.login, role-based redirections
    - Removed statistics section for cleaner design
  - **CSS Enhancements**:
    - Added custom CSS variables for gradients and shadows
    - Added keyframe animations: float, float-delayed, pulse-glow, fade-in-up, fade-in, slide-in-right
    - Added utility classes: gradient-primary, gradient-dark, text-gradient, shadow-card
  - **Tailwind Config Updates**:
    - Added new boxShadow utilities: card, card-hover, input-focus
    - Added new animations: float, float-delayed, pulse-glow, fade-in-up, fade-in, slide-in-right
  - **Features**:
    - Full authentication functionality preserved
    - Role-based redirections (Admin → /admin/dashboard, User → /dashboard)
    - Error handling with SweetAlert2
    - Password visibility toggle
    - Remember me checkbox
    - Forgot password link
    - Loading states and animations
    - Mobile-responsive design

### Version 2.21.0 (January 9, 2026) - Comprehensive Codebase Scan & Bug Fixes
- ✅ **Bug Fixes**: Fixed critical API endpoint errors
  - Fixed `GET /api/admin/organizations/{id}` 400 Bad Request error (added validation for orgId > 0)
  - Fixed `GET /api/admin/ai-quota/members/{id}` 404 Not Found error (added organizationId query parameter support for SuperAdmin)
  - Fixed translation key error: `common.buttons.confirm` → `buttons.confirm` with `ns: 'common'` in sweetalert.ts
- ✅ **API Updates**: Enhanced AdminAIQuotaController
  - Added `organizationId` query parameter to `GetMembers` endpoint (SuperAdmin can filter by org)
  - Updated `GetAdminAiQuotaMembersQuery` to support organizationId filtering
  - Updated handler to use request.OrganizationId for proper organization scoping
- ✅ **Internationalization**: Fixed translation key references
  - Corrected `sweetalert.ts` to use proper i18next namespace syntax
  - Fixed 2 occurrences of incorrect translation key format
- ✅ **Documentation**: Updated API endpoint documentation
  - Added new AdminAIQuotas API client documentation
  - Updated hook count (17 hooks total)
  - Updated API client count (37 API clients total)
  - Updated page count (46 pages total)
- 📝 **Structure**: Verified all file counts against actual codebase

### Version 2.18.0 (January 8, 2025) - Comprehensive Codebase Scan
- ✅ **Documentation Update**: Comprehensive codebase scan and verification
  - Verified all component counts: 172 component files (170 .tsx + 2 .ts) ✅
  - Verified all page counts: 51 pages total ✅
  - Verified all API client counts: 35 API clients (38 files including 3 test files) ✅
  - Verified all hook counts: 15 hooks (14 .ts + 1 .tsx) ✅
  - Verified all context counts: 7 files (5 .tsx + 2 .tsx test files) ✅
  - All counts verified against actual codebase files using glob_file_search and grep
  - Updated version to 2.18.0
  - Updated "Last Updated" date to reflect comprehensive scan

### Version 2.17.0 (January 7, 2025) - Comprehensive Codebase Scan
- ✅ **Documentation Update**: Comprehensive codebase scan and verification
  - Verified all component counts: 172 component files (170 .tsx + 2 .ts) ✅
  - Verified all page counts: 51 pages total ✅
  - Verified all API client counts: 33 API clients (36 files including 3 test files) ✅
  - Verified all hook counts: 15 hooks (14 .ts + 1 .tsx) ✅
  - All counts verified against actual codebase files using glob_file_search
  - Updated version to 2.17.0
  - Updated "Last Updated" date to reflect comprehensive scan
- ✅ **Code Quality**: Fixed all TypeScript and ESLint errors
  - Fixed all `@typescript-eslint/no-explicit-any` errors
  - Replaced `any` types with proper TypeScript interfaces
  - Installed missing type definitions (@types/react-window)
  - Removed deprecated React Query `onError` callbacks
  - Fixed unused variable warnings
  - All type-check and lint checks now pass ✅

### Version 2.4 (December 24, 2024)
- ✅ **Documentation**: Comprehensive codebase analysis and documentation update
- ✅ **Routes**: Added ForgotPassword and ResetPassword routes
- ✅ **Admin Pages**: Added AdminAuditLogs and AdminSystemHealth pages
- ✅ **API Clients**: Updated to reflect all 24 API client modules
- ✅ **Components**: Updated component counts (102 total, 51 UI components)
- ✅ **Pages**: Updated page counts (34 total pages)
- 📝 **Structure**: Accurate file counts and organization documented

### Version 2.22.0 (January 9, 2026)
- ✅ **Bug Fixes**: Fixed translation key issue in sweetalert.ts (common.buttons.confirm → buttons.confirm with ns: 'common')
- ✅ **API Enhancement**: Updated organizationsApi.getById to handle 400 BadRequest for invalid orgId
- ✅ **API Enhancement**: Updated adminAIQuotasApi to use correct endpoint `/api/admin/ai-quota/ai-quotas/members`
- ✅ **Documentation**: Updated API endpoint documentation with latest changes
- ✅ **Documentation**: Updated changelog with bug fixes and enhancements

### Version 2.25.0 (January 15, 2026)
- ✅ **Documentation**: Complete codebase scan and comprehensive documentation update
- ✅ **Statistics**: Updated codebase statistics (332 TS/TSX files: 252 TSX + 80 TS, 51 pages, 188 components, 36 API clients, 16 hooks, 9 contexts, 32+ test files)
- ✅ **Codebase Audit**: Comprehensive scan of entire frontend structure
- ✅ **Pages**: Verified page count (51 pages excluding test files)
- ✅ **Components**: Verified component count (188 component files)
- ✅ **API Clients**: Verified API client count (36 clients excluding test files)
- ✅ **Hooks**: Verified hook count (16 custom hooks)
- ✅ **Contexts**: Verified context count (9 files: 7 main + 2 test files)

### Version 2.24.0 (January 10, 2026)
- ✅ **Documentation**: Complete codebase analysis and comprehensive documentation update
- ✅ **Statistics**: Updated codebase statistics (~290 TS/TSX files, 58 pages, ~163 components, 40 API clients, 17 hooks, 7 contexts, 32 test files)
- ✅ **Codebase Audit**: Comprehensive analysis of entire frontend structure
- ✅ **Type Check**: Identified unused imports and variables (132 type-check errors for unused code)
- ✅ **File Analysis**: Complete inventory of all frontend files and their usage

### Version 2.23.0 (January 9, 2026)
- ✅ **Bug Fixes**: Fixed `format is not defined` error in TaskTimelineView.tsx (added format import from date-fns)
- ✅ **Bug Fixes**: Fixed `useNavigate() may be used only in the context of a <Router>` error in ErrorFallback.tsx (replaced with window.location.href)
- ✅ **Translations**: Added missing translation keys in tasks.json (FR and EN)
  - Added `search.placeholder`, `sort.label`, `sort.priority`, `sort.updated`, `sort.dueDate`, `sort.points`, `sort.alpha`
  - Added `columns.todo`, `columns.inProgress`, `columns.blocked`, `columns.done`
- ✅ **API Integration**: Fixed 404 error for `GET /api/v1/Projects/{id}/permissions` endpoint (now implemented in backend)
- ✅ **Documentation**: Updated API endpoint documentation with project permissions endpoint

### Version 2.3 (December 24, 2024)
- ✅ **API Client**: Fixed admin routes handling (excluded from automatic versioning)
- ✅ **Feature Flags**: Fixed 404 errors for feature flags endpoints
- ✅ **Documentation**: Updated API endpoint documentation to reflect route patterns
- 📝 **Routing**: Admin routes use `/api/admin/...` without version prefix, standard routes use `/api/v1/...`

### Version 2.3 (December 23, 2024)
- ✅ **User Invitations**: Added organization-level user invitation flow
- ✅ **Accept Invite Page**: Updated to include username, password fields with react-hook-form + zod validation
- ✅ **PasswordStrengthIndicator**: Added new password strength component with 4-bar display
- ✅ **Register Page**: Disabled public registration, shows informative message
- ✅ **Admin Users**: Added invite user functionality with InviteUserDialog (firstName, lastName, role fields)
- ✅ **Login Page**: Removed "Sign up" link
- 📝 **Documentation**: Updated pages, components, and API endpoint documentation

### Version 2.1 (December 23, 2024)
- ✅ **TypeScript Strict Mode**: Enabled comprehensive strict mode with all options
- ✅ **Type Safety**: Resolved all 164 strict mode errors
- ✅ **Code Quality**: Removed unused imports and variables
- ✅ **Null Safety**: Added comprehensive null checks throughout codebase
- ✅ **Type Checking**: Added `npm run type-check` script for explicit type checking
- 📝 **Documentation**: Updated TypeScript section with strict mode details and best practices

### Version 2.0 (December 22, 2024)
- Initial comprehensive documentation

---

---

## 31. Unused Files Analysis

### 31.1 Potentially Unused Files

Based on comprehensive codebase analysis (January 10, 2026), the following files may be candidates for removal or cleanup:

#### 31.1.1 Duplicate Script Files
- **frontend/scripts/generate-types.js** - Duplicate of `generate-types.ts`
  - **Status**: Legacy JavaScript version, TypeScript version (`generate-types.ts`) is the active one
  - **Action**: Can be removed if TypeScript version works correctly

#### 31.1.2 Empty/Placeholder Files
- **frontend/remove-unused-imports.ps1** - Empty PowerShell script with only comments
  - **Status**: Placeholder script, not functional
  - **Action**: Can be removed or implemented if needed

#### 31.1.3 Test Output Files
- **frontend/type-check-errors.txt** - Contains TypeScript type-check errors (132 unused import/variable warnings)
  - **Status**: Output file, can be regenerated
  - **Action**: Can be removed (regenerated on demand) or added to `.gitignore`
  
- **frontend/type-check-output.txt** - Empty TypeScript type-check output file
  - **Status**: Output file, empty
  - **Action**: Can be removed or added to `.gitignore`

#### 31.1.4 Unused Imports/Variables

Based on TypeScript type-check analysis, the following files contain unused imports/variables (132 total warnings):

**High Priority (Multiple Unused Items):**
- `src/components/admin/EditUserDialog.tsx` - 3 unused items
- `src/components/agents/ProjectInsightPanel.tsx` - 2 unused items
- `src/components/agents/RiskDetectionPanel.tsx` - 2 unused items
- `src/components/agents/SprintPlanningAssistant.tsx` - 2 unused items
- `src/components/defects/CreateDefectDialog.tsx` - 3 unused items
- `src/components/defects/DefectDetailSheet.tsx` - 3 unused items
- `src/components/milestones/MilestoneTimeline.tsx` - 5 unused items
- `src/components/notifications/NotificationDropdown.tsx` - 3 unused items
- `src/components/projects/AddMemberDialog.tsx` - 3 unused items
- `src/components/projects/EditProjectDialog.tsx` - 3 unused items
- `src/components/projects/InviteMemberModal.tsx` - 3 unused items
- `src/components/sprints/AddTasksToSprintDialog.tsx` - 3 unused items
- `src/components/tasks/CreateTaskDialog.tsx` - 3 unused items
- `src/components/tasks/TaskDetailSheet.tsx` - 5 unused items
- `src/components/tasks/TaskListView.tsx` - 6 unused items
- `src/pages/admin/AdminUsers.tsx` - 4 unused items
- `src/pages/Backlog.tsx` - 3 unused items
- `src/pages/Tasks.tsx` - 4 unused items

**Action**: Run `npm run type-check` and remove unused imports/variables to improve code quality.

### 31.2 Recommendations

1. **Remove Duplicate Scripts**: Remove `generate-types.js` if TypeScript version (`generate-types.ts`) is working correctly
2. **Clean Up Output Files**: Add `type-check-*.txt` to `.gitignore` or remove them
3. **Implement or Remove Placeholder Scripts**: Either implement `remove-unused-imports.ps1` or remove it
4. **Clean Up Unused Imports**: Run ESLint/TypeScript cleanup to remove unused imports and variables
5. **Add to .gitignore**: Consider adding output files like `type-check-*.txt` to `.gitignore`

### 31.3 Files to Keep

- All test files (`.test.tsx`, `.test.ts`) - Required for testing
- All configuration files - Required for build and runtime
- All documentation files - Required for maintenance
- All component/page files - Even with unused imports, they may be needed for future use

---

**Document Version:** 2.25.0  
**Last Updated:** January 15, 2026 (Complete Codebase Scan - Comprehensive Update)  
**Maintained By:** Development Team

