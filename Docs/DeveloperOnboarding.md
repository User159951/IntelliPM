# IntelliPM Frontend Developer Onboarding Guide

## Overview

This guide helps new developers get started with the IntelliPM frontend codebase. It covers setup, architecture, permission system, and common development tasks.

---

## Prerequisites

Before starting, ensure you have:

- **Node.js 18+** and **npm** ([Install with nvm](https://github.com/nvm-sh/nvm))
- **Git** for version control
- **Code Editor**: VS Code recommended (with ESLint and Prettier extensions)
- **Browser**: Chrome or Firefox with React DevTools extension

---

## Initial Setup

### 1. Clone Repository

```bash
git clone <repository-url>
cd intelliPM-V2/frontend
```

### 2. Install Dependencies

```bash
npm install
```

### 3. Configure Environment

Create `.env.local` file:

```env
VITE_API_BASE_URL=http://localhost:5001
```

### 4. Start Development Server

```bash
npm run dev
```

The application will be available at `http://localhost:5173` (or the port shown in terminal).

### 5. Verify Setup

- Open browser to `http://localhost:5173`
- You should see the login page
- Check browser console for errors

---

## Project Structure

```
frontend/
├── src/
│   ├── api/              # API client modules (36 files)
│   │   ├── client.ts     # Base API client
│   │   ├── projects.ts   # Project API calls
│   │   ├── tasks.ts      # Task API calls
│   │   └── ...
│   │
│   ├── components/       # React components
│   │   ├── ui/          # shadcn/ui components (51 components)
│   │   ├── guards/      # Permission guards
│   │   ├── layout/      # Layout components
│   │   ├── projects/    # Project-specific components
│   │   ├── tasks/       # Task-specific components
│   │   └── ...
│   │
│   ├── contexts/        # React contexts
│   │   ├── AuthContext.tsx
│   │   ├── ThemeContext.tsx
│   │   └── FeatureFlagsContext.tsx
│   │
│   ├── hooks/           # Custom React hooks
│   │   ├── usePermissions.ts
│   │   ├── useUserRole.ts
│   │   └── ...
│   │
│   ├── pages/           # Page components (51 files)
│   │   ├── Dashboard.tsx
│   │   ├── Projects.tsx
│   │   └── ...
│   │
│   ├── types/           # TypeScript type definitions
│   │   ├── index.ts
│   │   └── ...
│   │
│   ├── lib/             # Utility functions
│   │   └── sweetalert.ts
│   │
│   ├── App.tsx          # Main app component
│   └── main.tsx         # Entry point
│
├── public/              # Static assets
├── package.json         # Dependencies and scripts
├── tsconfig.json       # TypeScript configuration
├── vite.config.ts      # Vite configuration
└── tailwind.config.ts  # Tailwind CSS configuration
```

---

## Architecture Overview

### Technology Stack

- **React 18**: UI framework
- **TypeScript**: Type safety
- **Vite**: Build tool and dev server
- **TanStack Query (React Query)**: Data fetching and caching
- **React Router v6**: Routing
- **shadcn/ui**: UI component library (Radix UI)
- **Tailwind CSS**: Styling
- **React Hook Form + Zod**: Form handling and validation

### Key Patterns

#### 1. Component-Based Architecture
- Components are organized by feature (projects, tasks, etc.)
- Reusable UI components in `components/ui/`
- Feature-specific components in `components/{feature}/`

#### 2. Custom Hooks
- `usePermissions`: Permission checking
- `useUserRole`: Role checking
- `useAuth`: Authentication state
- API hooks: Data fetching with React Query

#### 3. API Client Pattern
- Centralized API client in `api/client.ts`
- Feature-specific API modules (e.g., `api/projects.ts`)
- Automatic error handling and token management

#### 4. Permission Guards
- `PermissionGuard`: Protect UI elements based on permissions
- `RequireAdminGuard`: Protect admin routes
- Role-based conditional rendering

---

## Permission System

### Understanding Permissions

IntelliPM uses a two-tier permission system:

1. **Global Permissions**: Organization-wide (Admin, User, SuperAdmin)
2. **Project Permissions**: Project-specific (ProductOwner, ScrumMaster, Developer, Tester, Viewer, Manager)

### Permission Format

Permissions follow the format: `resource.action`

Examples:
- `projects.create`
- `tasks.edit`
- `sprints.start`
- `releases.approve`

### Using Permissions in Components

#### Method 1: usePermissions Hook

```tsx
import { usePermissions, PERMISSIONS } from '@/hooks/usePermissions';

function MyComponent() {
  const { can, canAny, canAll } = usePermissions();

  if (can(PERMISSIONS.PROJECTS_CREATE)) {
    return <CreateProjectButton />;
  }

  return null;
}
```

#### Method 2: PermissionGuard Component

```tsx
import { PermissionGuard } from '@/components/guards/PermissionGuard';
import { PERMISSIONS } from '@/hooks/usePermissions';

function MyComponent() {
  return (
    <PermissionGuard requiredPermission={PERMISSIONS.PROJECTS_CREATE}>
      <CreateProjectButton />
    </PermissionGuard>
  );
}
```

#### Method 3: Project-Specific Permissions

```tsx
import { usePermissionsWithProject } from '@/hooks/usePermissions';

function ProjectComponent({ projectId }: { projectId: number }) {
  const { can } = usePermissionsWithProject(projectId);

  if (can(PERMISSIONS.SPRINTS_START)) {
    return <StartSprintButton />;
  }

  return null;
}
```

### Using Roles in Components

```tsx
import { useUserRole } from '@/hooks/useUserRole';

function MyComponent({ projectId }: { projectId: number }) {
  const { isScrumMaster, isProductOwner, projectRole } = useUserRole(projectId);

  if (isScrumMaster) {
    return <SprintManagementPanel />;
  }

  if (isProductOwner) {
    return <ProductBacklogPanel />;
  }

  return <ViewerPanel />;
}
```

---

## Adding New Permissions

### Step 1: Define Permission Constant

Add to `frontend/src/hooks/usePermissions.ts`:

```typescript
export const PERMISSIONS = {
  // ... existing permissions
  NEW_FEATURE_CREATE: 'new-feature.create',
  NEW_FEATURE_EDIT: 'new-feature.edit',
  NEW_FEATURE_DELETE: 'new-feature.delete',
  NEW_FEATURE_VIEW: 'new-feature.view',
} as const;
```

### Step 2: Map Permission to Roles

Update `getProjectRolePermissions` function:

```typescript
function getProjectRolePermissions(role: ProjectRole | null | undefined): Permission[] {
  if (!role) return [];

  const permissions: Permission[] = [];

  switch (role) {
    case 'ProductOwner':
      permissions.push(
        // ... existing permissions
        PERMISSIONS.NEW_FEATURE_CREATE,
        PERMISSIONS.NEW_FEATURE_EDIT,
        PERMISSIONS.NEW_FEATURE_DELETE,
        PERMISSIONS.NEW_FEATURE_VIEW,
      );
      break;

    case 'ScrumMaster':
      permissions.push(
        // ... existing permissions
        PERMISSIONS.NEW_FEATURE_CREATE,
        PERMISSIONS.NEW_FEATURE_EDIT,
        PERMISSIONS.NEW_FEATURE_VIEW,
      );
      break;

    case 'Developer':
      permissions.push(
        // ... existing permissions
        PERMISSIONS.NEW_FEATURE_VIEW,
      );
      break;

    // ... other roles
  }

  return permissions;
}
```

### Step 3: Use Permission in Components

```tsx
import { PermissionGuard } from '@/components/guards/PermissionGuard';
import { PERMISSIONS } from '@/hooks/usePermissions';

function NewFeatureComponent() {
  return (
    <>
      <PermissionGuard requiredPermission={PERMISSIONS.NEW_FEATURE_VIEW}>
        <FeatureList />
      </PermissionGuard>

      <PermissionGuard requiredPermission={PERMISSIONS.NEW_FEATURE_CREATE}>
        <CreateFeatureButton />
      </PermissionGuard>
    </>
  );
}
```

### Step 4: Backend Integration

**Important**: Permissions must also be defined and enforced on the backend. See backend documentation for details.

---

## Protecting UI Actions

### Method 1: Conditional Rendering

```tsx
import { usePermissions } from '@/hooks/usePermissions';
import { PERMISSIONS } from '@/hooks/usePermissions';

function TaskActions({ task }: { task: Task }) {
  const { can } = usePermissions();

  return (
    <div>
      {can(PERMISSIONS.TASKS_EDIT) && (
        <Button onClick={handleEdit}>Edit</Button>
      )}
      {can(PERMISSIONS.TASKS_DELETE) && (
        <Button onClick={handleDelete}>Delete</Button>
      )}
    </div>
  );
}
```

### Method 2: PermissionGuard

```tsx
import { PermissionGuard } from '@/components/guards/PermissionGuard';

function TaskActions({ task }: { task: Task }) {
  return (
    <div>
      <PermissionGuard requiredPermission={PERMISSIONS.TASKS_EDIT}>
        <Button onClick={handleEdit}>Edit</Button>
      </PermissionGuard>

      <PermissionGuard requiredPermission={PERMISSIONS.TASKS_DELETE}>
        <Button onClick={handleDelete}>Delete</Button>
      </PermissionGuard>
    </div>
  );
}
```

### Method 3: Disabled State with Tooltip

```tsx
import { PermissionGuard } from '@/components/guards/PermissionGuard';
import { Button } from '@/components/ui/button';
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip';

function TaskActions({ task }: { task: Task }) {
  return (
    <PermissionGuard
      requiredPermission={PERMISSIONS.TASKS_DELETE}
      showTooltip={true}
      tooltipText="You need Delete permission to delete tasks"
    >
      <Button onClick={handleDelete}>Delete</Button>
    </PermissionGuard>
  );
}
```

### Method 4: API Call Protection

```tsx
import { usePermissions } from '@/hooks/usePermissions';
import { taskService } from '@/api/tasks';

function TaskComponent({ task }: { task: Task }) {
  const { can } = usePermissions();

  const handleDelete = async () => {
    if (!can(PERMISSIONS.TASKS_DELETE)) {
      showError('Access Denied', 'You do not have permission to delete tasks');
      return;
    }

    try {
      await taskService.deleteTask(task.id);
      // Handle success
    } catch (error) {
      // Handle error (backend will also validate)
    }
  };

  return <Button onClick={handleDelete}>Delete</Button>;
}
```

**Note**: Always validate permissions on the backend. Frontend checks are for UX only.

---

## Protecting API Endpoints

### Backend Protection

Backend endpoints are protected using `[RequirePermission]` attributes:

```csharp
[HttpDelete("{id}")]
[RequirePermission("tasks.delete")]
public async Task<IActionResult> DeleteTask(int id)
{
    // Handler implementation
}
```

### Frontend API Calls

Frontend API calls should handle permission errors gracefully:

```typescript
// api/tasks.ts
export const taskService = {
  async deleteTask(id: number): Promise<void> {
    try {
      await apiClient.delete(`/api/v1/Tasks/${id}`);
    } catch (error) {
      if (error.status === 403) {
        throw new Error('You do not have permission to delete tasks');
      }
      throw error;
    }
  },
};
```

---

## Common Development Tasks

### Adding a New Page

1. **Create Page Component**

```tsx
// src/pages/NewFeature.tsx
export default function NewFeature() {
  return (
    <div>
      <h1>New Feature</h1>
      {/* Page content */}
    </div>
  );
}
```

2. **Add Route**

```tsx
// src/App.tsx
import NewFeature from './pages/NewFeature';

// In Routes:
<Route path="/new-feature" element={<NewFeature />} />
```

3. **Add Navigation** (if needed)

```tsx
// src/components/layout/AppSidebar.tsx
<NavLink to="/new-feature">New Feature</NavLink>
```

### Adding a New API Endpoint

1. **Create API Service**

```typescript
// src/api/newFeature.ts
import { apiClient } from './client';

export interface NewFeature {
  id: number;
  name: string;
}

export const newFeatureService = {
  async getAll(): Promise<NewFeature[]> {
    return apiClient.get<NewFeature[]>('/api/v1/NewFeature');
  },

  async create(data: Omit<NewFeature, 'id'>): Promise<NewFeature> {
    return apiClient.post<NewFeature>('/api/v1/NewFeature', data);
  },
};
```

2. **Use in Component**

```tsx
import { useQuery, useMutation } from '@tanstack/react-query';
import { newFeatureService } from '@/api/newFeature';

function NewFeatureComponent() {
  const { data, isLoading } = useQuery({
    queryKey: ['new-features'],
    queryFn: () => newFeatureService.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: newFeatureService.create,
    onSuccess: () => {
      // Invalidate queries, show success message, etc.
    },
  });

  // Component implementation
}
```

### Adding a New Component

1. **Create Component File**

```tsx
// src/components/new-feature/NewFeatureCard.tsx
interface NewFeatureCardProps {
  feature: NewFeature;
  onEdit?: (feature: NewFeature) => void;
}

export function NewFeatureCard({ feature, onEdit }: NewFeatureCardProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{feature.name}</CardTitle>
      </CardHeader>
      {onEdit && (
        <CardFooter>
          <Button onClick={() => onEdit(feature)}>Edit</Button>
        </CardFooter>
      )}
    </Card>
  );
}
```

2. **Export from Index** (optional)

```typescript
// src/components/new-feature/index.ts
export { NewFeatureCard } from './NewFeatureCard';
```

---

## Common Pitfalls and Solutions

### Pitfall 1: Not Checking Permissions

**Problem**: Showing UI elements without permission checks.

**Solution**: Always use `PermissionGuard` or `usePermissions` hook.

```tsx
// ❌ Bad
<Button onClick={handleDelete}>Delete</Button>

// ✅ Good
<PermissionGuard requiredPermission={PERMISSIONS.TASKS_DELETE}>
  <Button onClick={handleDelete}>Delete</Button>
</PermissionGuard>
```

### Pitfall 2: Forgetting Project Context

**Problem**: Checking permissions without project context.

**Solution**: Use `usePermissionsWithProject` for project-specific permissions.

```tsx
// ❌ Bad (checks global permissions only)
const { can } = usePermissions();
if (can(PERMISSIONS.SPRINTS_START)) { ... }

// ✅ Good (checks project permissions)
const { can } = usePermissionsWithProject(projectId);
if (can(PERMISSIONS.SPRINTS_START)) { ... }
```

### Pitfall 3: Not Handling Loading States

**Problem**: Showing content before permissions are loaded.

**Solution**: Check `isLoading` state.

```tsx
const { can, isLoading } = usePermissions();

if (isLoading) {
  return <Skeleton />;
}

if (can(PERMISSIONS.PROJECTS_CREATE)) {
  return <CreateButton />;
}
```

### Pitfall 4: Not Handling API Errors

**Problem**: Not handling 403 (Forbidden) errors from API.

**Solution**: Handle permission errors gracefully.

```typescript
try {
  await taskService.deleteTask(id);
} catch (error) {
  if (error.status === 403) {
    showError('Access Denied', 'You do not have permission to delete tasks');
  } else {
    showError('Error', 'Failed to delete task');
  }
}
```

### Pitfall 5: Hardcoding Role Names

**Problem**: Using string literals for roles.

**Solution**: Use constants or type-safe enums.

```tsx
// ❌ Bad
if (role === 'ProductOwner') { ... }

// ✅ Good
import { ProjectRole } from '@/types';
if (role === ProjectRole.ProductOwner) { ... }
```

---

## Testing

### Running Tests

```bash
npm test              # Run tests in watch mode
npm run test:run      # Run tests once
npm run test:coverage # Generate coverage report
npm run test:ui       # Open test UI
```

### Writing Tests

```tsx
// src/components/__tests__/TaskCard.test.tsx
import { render, screen } from '@testing-library/react';
import { TaskCard } from '../TaskCard';

describe('TaskCard', () => {
  it('renders task name', () => {
    render(<TaskCard task={{ id: 1, name: 'Test Task' }} />);
    expect(screen.getByText('Test Task')).toBeInTheDocument();
  });
});
```

---

## Code Style Guidelines

### TypeScript

- Use TypeScript strict mode
- Define types for all props and functions
- Use interfaces for object shapes
- Use type aliases for unions and complex types

### React

- Use functional components with hooks
- Use `useCallback` for event handlers passed as props
- Use `useMemo` for expensive computations
- Extract complex logic into custom hooks

### Naming Conventions

- Components: PascalCase (e.g., `TaskCard`)
- Functions: camelCase (e.g., `handleDelete`)
- Constants: UPPER_SNAKE_CASE (e.g., `PERMISSIONS`)
- Files: Match component/function name

### File Organization

- One component per file
- Co-locate related files (component + styles + tests)
- Use index files for clean imports

---

## Debugging Tips

### React DevTools

- Install React DevTools browser extension
- Inspect component props and state
- Check hook values

### Browser DevTools

- Check Network tab for API calls
- Check Console for errors
- Use Sources tab for breakpoints

### VS Code Debugging

Configure `.vscode/launch.json`:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "type": "chrome",
      "request": "launch",
      "name": "Launch Chrome",
      "url": "http://localhost:5173",
      "webRoot": "${workspaceFolder}/frontend"
    }
  ]
}
```

---

## Getting Help

### Documentation

- [Roles and Permissions Guide](./RolesAndPermissions.md)
- [Workflow Guide](./WorkflowGuide.md)
- [AI Governance Guide](./AIGovernanceGuide.md)
- [Backend Documentation](../IntelliPM_Backend.md)

### Code References

- `usePermissions` hook: `src/hooks/usePermissions.ts`
- `PermissionGuard` component: `src/components/guards/PermissionGuard.tsx`
- API client: `src/api/client.ts`
- Type definitions: `src/types/index.ts`

### Common Questions

**Q: How do I check if a user is an admin?**
```tsx
const { hasGlobalRole } = usePermissions();
if (hasGlobalRole('Admin')) { ... }
```

**Q: How do I protect a route?**
```tsx
<Route
  path="/admin"
  element={
    <RequireAdminGuard>
      <AdminPage />
    </RequireAdminGuard>
  }
/>
```

**Q: How do I fetch data with React Query?**
```tsx
const { data, isLoading, error } = useQuery({
  queryKey: ['tasks', projectId],
  queryFn: () => taskService.getTasks(projectId),
});
```

---

*Last Updated: 2025-01-06*
*Version: 1.0*

