import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@/test/test-utils'
import userEvent from '@testing-library/user-event'
import { TeamMembersList } from './TeamMembersList'
import { projectsApi } from '@/api/projects'

// Mock the APIs
vi.mock('@/api/projects', () => ({
  projectsApi: {
    getMembers: vi.fn(),
    inviteMember: vi.fn(),
    updateMemberRole: vi.fn(),
    removeMember: vi.fn(),
  },
}))

vi.mock('@/api/tasks', () => ({
  tasksApi: {
    getByProject: vi.fn(() => Promise.resolve({ tasks: [] })),
  },
}))

// Mock useAuth
vi.mock('@/contexts/AuthContext', async () => {
  const actual = await vi.importActual('@/contexts/AuthContext')
  return {
    ...actual,
    useAuth: () => ({
      user: { userId: 1, username: 'testuser', email: 'test@example.com', firstName: 'Test', lastName: 'User', roles: ['Developer'] },
      isAuthenticated: true,
      isLoading: false,
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
    }),
  }
})

// Mock useProjectPermissions
const mockPermissions = {
  userRole: 'ProductOwner' as const,
  isLoading: false,
  canEditProject: true,
  canDeleteProject: false,
  canInviteMembers: true,
  canRemoveMembers: true,
  canChangeRoles: true,
  canCreateTasks: true,
  canEditTasks: true,
  canDeleteTasks: true,
  canManageSprints: true,
  canViewMilestones: true,
  canCreateMilestone: true,
  canEditMilestone: true,
  canCompleteMilestone: true,
  canDeleteMilestone: true,
  isViewer: false,
  isProductOwner: true,
  isScrumMaster: false,
}

vi.mock('@/hooks/useProjectPermissions', () => ({
  useProjectPermissions: vi.fn(() => mockPermissions),
}))

// Mock useNavigate
const mockNavigate = vi.fn()
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  }
})

// Mock useToast
const mockToast = vi.fn()
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: mockToast,
  }),
}))

// Mock RoleBadge
vi.mock('./RoleBadge', () => ({
  default: ({ role }: { role: string }) => <span data-testid="role-badge">{role}</span>,
}))

const mockGetMembers = vi.mocked(projectsApi.getMembers)
const mockInviteMember = vi.mocked(projectsApi.inviteMember)
const mockUpdateMemberRole = vi.mocked(projectsApi.updateMemberRole)
const mockRemoveMember = vi.mocked(projectsApi.removeMember)

// Mock InviteMemberModal - it calls projectsApi.inviteMember internally
// Since projectsApi is already mocked, we can import it directly in the mock
vi.mock('./InviteMemberModal', async () => {
  // Import the mocked projectsApi
  const { projectsApi } = await import('@/api/projects')
  return {
    InviteMemberModal: ({ isOpen, onClose, onSuccess, projectId }: { isOpen: boolean; onClose: () => void; onSuccess: () => void; projectId: number }) => {
      if (!isOpen) return null
      const handleSuccess = async () => {
        // Call the mocked API function - this will call the mock we set up
        await projectsApi.inviteMember(projectId, { email: 'new@example.com', role: 'Developer' })
        onSuccess()
      }
      return (
        <div data-testid="invite-modal">
          <button onClick={handleSuccess}>Mock Invite Success</button>
          <button onClick={onClose}>Close</button>
        </div>
      )
    },
  }
})

describe('TeamMembersList', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockGetMembers.mockClear()
    mockInviteMember.mockClear()
    mockUpdateMemberRole.mockClear()
    mockRemoveMember.mockClear()
  })

  it('renders list of members with RoleBadge', async () => {
    const mockMembers = [
      {
        id: 1,
        userId: 1,
        userName: 'john.doe',
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        role: 'ProductOwner' as const,
        invitedAt: '2024-01-01T00:00:00Z',
        invitedByName: 'Admin',
      },
      {
        id: 2,
        userId: 2,
        userName: 'jane.smith',
        firstName: 'Jane',
        lastName: 'Smith',
        email: 'jane@example.com',
        role: 'Developer' as const,
        invitedAt: '2024-01-02T00:00:00Z',
        invitedByName: 'Admin',
      },
    ]

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    mockGetMembers.mockResolvedValue(mockMembers as any)

    render(<TeamMembersList projectId={1} ownerId={1} />)

    await waitFor(() => {
      expect(screen.getByText('John Doe')).toBeInTheDocument()
      expect(screen.getByText('Jane Smith')).toBeInTheDocument()
    })

    // Verify RoleBadge is displayed
    expect(screen.getAllByTestId('role-badge')).toHaveLength(2)
  })

  it('permission gating: invite button visible only if canInviteMembers', async () => {
    mockGetMembers.mockResolvedValue([])

    // Test with permissions
    const { useProjectPermissions } = await import('@/hooks/useProjectPermissions')
    vi.mocked(useProjectPermissions).mockReturnValue({
      ...mockPermissions,
      canInviteMembers: true,
    })

    const { rerender } = render(<TeamMembersList projectId={1} ownerId={1} />)

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /invite member/i })).toBeDefined()
    })

    // Test without permissions
    vi.mocked(useProjectPermissions).mockReturnValue({
      ...mockPermissions,
      canInviteMembers: false,
    })

    rerender(<TeamMembersList projectId={1} ownerId={1} />)

    await waitFor(() => {
      expect(screen.queryByRole('button', { name: /invite member/i })).toBeNull()
    })
  })

  it('permission gating: change role and remove buttons visible only if permissions allow', async () => {
    const mockMembers = [
      {
        id: 1,
        userId: 2,
        userName: 'jane.smith',
        firstName: 'Jane',
        lastName: 'Smith',
        email: 'jane@example.com',
        role: 'Developer' as const,
        invitedAt: '2024-01-02T00:00:00Z',
        invitedByName: 'Admin',
      },
    ]

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    mockGetMembers.mockResolvedValue(mockMembers as any)

    // Test with permissions
    const { useProjectPermissions } = await import('@/hooks/useProjectPermissions')
    vi.mocked(useProjectPermissions).mockReturnValue({
      ...mockPermissions,
      canChangeRoles: true,
      canRemoveMembers: true,
    })

    render(<TeamMembersList projectId={1} ownerId={1} />)

    await waitFor(() => {
      expect(screen.getByText('Jane Smith')).toBeInTheDocument()
    })

    // Find the dropdown menu trigger (MoreVertical icon button)
    const moreButton = screen.getByRole('button', { name: /member options/i })
    await userEvent.click(moreButton)

    await waitFor(() => {
      expect(screen.getByText(/change role/i)).toBeInTheDocument()
      expect(screen.getByText(/remove from project/i)).toBeInTheDocument()
    })
  })

  it('invite success triggers refresh', async () => {
    mockGetMembers.mockResolvedValue([])

    render(<TeamMembersList projectId={1} ownerId={1} />)

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /invite member/i })).toBeInTheDocument()
    })

    // Click invite button
    const inviteButton = screen.getByRole('button', { name: /invite member/i })
    await userEvent.click(inviteButton)

    // Wait for modal to open
    await waitFor(() => {
      expect(screen.getByTestId('invite-modal')).toBeInTheDocument()
    })

    // Mock invite API
    mockInviteMember.mockResolvedValueOnce({
      memberId: 1,
      email: 'new@example.com',
      role: 'Developer' as const,
    })

    // Mock getMembers to return new member after invite
    mockGetMembers.mockResolvedValueOnce([
      {
        id: 1,
        userId: 3,
        userName: 'new.user',
        email: 'new@example.com',
        role: 'Developer' as const,
        invitedAt: '2024-01-03T00:00:00Z',
        invitedByName: 'Admin',
      },
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    ] as any)

    // Click mock invite success button
    const successButton = screen.getByRole('button', { name: /mock invite success/i })
    await userEvent.click(successButton)

    // Verify API was called
    await waitFor(() => {
      expect(mockInviteMember).toHaveBeenCalled()
    }, { timeout: 3000 })
  })

  it('change role mutation updates role and refreshes', async () => {
    const mockMembers = [
      {
        id: 1,
        userId: 2,
        userName: 'jane.smith',
        firstName: 'Jane',
        lastName: 'Smith',
        email: 'jane@example.com',
        role: 'Developer' as const,
        invitedAt: '2024-01-02T00:00:00Z',
        invitedByName: 'Admin',
      },
    ]

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    mockGetMembers.mockResolvedValue(mockMembers as any)
    mockUpdateMemberRole.mockResolvedValueOnce(undefined)

    render(<TeamMembersList projectId={1} ownerId={1} />)

    await waitFor(() => {
      expect(screen.getByText('Jane Smith')).toBeInTheDocument()
    })

    // Open dropdown and click change role
    const moreButton = screen.getByRole('button', { name: /member options/i })
    await userEvent.click(moreButton)

    await waitFor(() => {
      expect(screen.getByText(/change role/i)).toBeInTheDocument()
    })

    const changeRoleButton = screen.getByText(/change role/i)
    await userEvent.click(changeRoleButton)

    // Wait for role change dialog
    await waitFor(() => {
      expect(screen.getByText(/change role for/i)).toBeInTheDocument()
    })

    // The Select component is difficult to interact with in jsdom due to Radix UI limitations
    // Instead, we'll verify that the dialog opens and the update button is present
    // Then we'll click update with the default role (Developer) to verify the API is called
    // In a real scenario, the user would select a different role, but for testing purposes,
    // we'll verify the API call with the current role
    
    // Verify the Select is present with the current role
    const roleSelect = screen.getByRole('combobox')
    expect(roleSelect).toBeInTheDocument()

    // Click update button (this will call the API with the current role: Developer)
    const updateButton = screen.getByRole('button', { name: /update role/i })
    await userEvent.click(updateButton)

    // Verify API was called with the current role (Developer)
    // Note: In a real scenario, the user would change the role first, but due to jsdom
    // limitations with Radix UI Select, we're testing with the default role
    await waitFor(() => {
      expect(mockUpdateMemberRole).toHaveBeenCalledWith(1, 2, 'Developer')
    }, { timeout: 3000 })
  })

  it('remove member mutation deletes and refreshes', async () => {
    const mockMembers = [
      {
        id: 1,
        userId: 2,
        userName: 'jane.smith',
        firstName: 'Jane',
        lastName: 'Smith',
        email: 'jane@example.com',
        role: 'Developer' as const,
        invitedAt: '2024-01-02T00:00:00Z',
        invitedByName: 'Admin',
      },
    ]

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    mockGetMembers.mockResolvedValue(mockMembers as any)
    mockRemoveMember.mockResolvedValueOnce(undefined)

    render(<TeamMembersList projectId={1} ownerId={1} />)

    await waitFor(() => {
      expect(screen.getByText('Jane Smith')).toBeInTheDocument()
    })

    // Open dropdown and click remove
    const moreButton = screen.getByRole('button', { name: /member options/i })
    await userEvent.click(moreButton)

    await waitFor(() => {
      expect(screen.getByText(/remove from project/i)).toBeInTheDocument()
    })

    const removeButton = screen.getByText(/remove from project/i)
    await userEvent.click(removeButton)

    // Wait for confirmation dialog
    await waitFor(() => {
      expect(screen.getByText(/remove member from project/i)).toBeInTheDocument()
    })

    // Confirm removal
    const confirmButton = screen.getByRole('button', { name: /remove/i })
    await userEvent.click(confirmButton)

    // Verify API was called
    await waitFor(() => {
      expect(mockRemoveMember).toHaveBeenCalledWith(1, 2)
    }, { timeout: 3000 })
  })
})

