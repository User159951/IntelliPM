import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@/test/test-utils';
import userEvent from '@testing-library/user-event';
import { EditProjectDialog } from './EditProjectDialog';
import { createMockProject } from '@/mocks/data';
import { http, HttpResponse } from 'msw';
import { server } from '@/mocks/server';

// Mock useToast
const mockToast = vi.fn();
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: mockToast,
  }),
}));

// Mock useProjectPermissions
vi.mock('@/hooks/useProjectPermissions', () => ({
  useProjectPermissions: () => ({
    canEditProject: true,
    canDeleteProject: true,
    canInviteMembers: true,
    canRemoveMembers: true,
    canChangeRoles: true,
    isLoading: false,
    userRole: 'ProductOwner' as const,
  }),
}));

describe('EditProjectDialog', () => {
  const mockProject = createMockProject({
    id: 1,
    name: 'Test Project',
    description: 'Test Description',
    type: 'Scrum',
    status: 'Active',
    sprintDurationDays: 14,
  });

  const defaultProps = {
    open: true,
    onOpenChange: vi.fn(),
    project: mockProject,
  };

  beforeEach(() => {
    server.resetHandlers();
    vi.clearAllMocks();
  });

  it('renders dialog when open', () => {
    render(<EditProjectDialog {...defaultProps} />);

    expect(screen.getByText(/edit project/i)).toBeInTheDocument();
    expect(screen.getByDisplayValue('Test Project')).toBeInTheDocument();
  });

  it('does not render when closed', () => {
    render(<EditProjectDialog {...defaultProps} open={false} />);

    expect(screen.queryByText(/edit project/i)).not.toBeInTheDocument();
  });

  it('pre-fills form with project data', () => {
    render(<EditProjectDialog {...defaultProps} />);

    expect(screen.getByDisplayValue('Test Project')).toBeInTheDocument();
    expect(screen.getByDisplayValue('Test Description')).toBeInTheDocument();
  });

  it('validates required fields', async () => {
    const user = userEvent.setup();
    render(<EditProjectDialog {...defaultProps} />);

    const nameInput = screen.getByLabelText(/project name/i);
    await user.clear(nameInput);

    const saveButton = screen.getByRole('button', { name: /save changes/i });
    await user.click(saveButton);

    // Form validation should prevent submission
    await waitFor(() => {
      expect(nameInput).toBeInvalid();
    });
  });

  it('updates project successfully', async () => {
    const user = userEvent.setup();
    const onOpenChange = vi.fn();

    server.use(
      http.put('http://localhost:5001/api/v1/Projects/1', () => {
        return HttpResponse.json({
          ...mockProject,
          name: 'Updated Project',
          description: 'Updated Description',
        });
      })
    );

    render(<EditProjectDialog {...defaultProps} onOpenChange={onOpenChange} />);

    const nameInput = screen.getByLabelText(/project name/i);
    await user.clear(nameInput);
    await user.type(nameInput, 'Updated Project');

    const descriptionInput = screen.getByLabelText(/description/i);
    await user.clear(descriptionInput);
    await user.type(descriptionInput, 'Updated Description');

    const saveButton = screen.getByRole('button', { name: /save changes/i });
    await user.click(saveButton);

    await waitFor(() => {
      expect(onOpenChange).toHaveBeenCalledWith(false);
    });
  });

  it('handles update error', async () => {
    const user = userEvent.setup();

    server.use(
      http.put('http://localhost:5001/api/v1/Projects/1', () => {
        return HttpResponse.json(
          { error: 'Update failed' },
          { status: 400 }
        );
      })
    );

    render(<EditProjectDialog {...defaultProps} />);

    const nameInput = screen.getByLabelText(/project name/i);
    await user.clear(nameInput);
    await user.type(nameInput, 'Updated Project');

    const saveButton = screen.getByRole('button', { name: /save changes/i });
    await user.click(saveButton);

    // Verify that toast was called with error message
    await waitFor(() => {
      expect(mockToast).toHaveBeenCalledWith(
        expect.objectContaining({
          title: 'Failed to update project',
          variant: 'destructive',
        })
      );
    });
  });

  it('closes dialog on cancel', async () => {
    const user = userEvent.setup();
    const onOpenChange = vi.fn();

    render(<EditProjectDialog {...defaultProps} onOpenChange={onOpenChange} />);

    const cancelButton = screen.getByRole('button', { name: /cancel/i });
    await user.click(cancelButton);

    expect(onOpenChange).toHaveBeenCalledWith(false);
  });
});

