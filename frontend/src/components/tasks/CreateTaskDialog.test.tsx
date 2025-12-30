import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@/test/test-utils';
import userEvent from '@testing-library/user-event';
import { CreateTaskDialog } from './CreateTaskDialog';
import { http, HttpResponse } from 'msw';
import { server } from '@/mocks/server';

// Mock useToast
const mockToast = vi.fn();
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: mockToast,
  }),
}));

// Mock the complex dependencies
vi.mock('@/api/users', () => ({
  usersApi: {
    getAll: vi.fn().mockResolvedValue({
      users: [
        { userId: 1, username: 'user1', email: 'user1@example.com' },
        { userId: 2, username: 'user2', email: 'user2@example.com' },
      ],
    }),
  },
}));

vi.mock('@/api/sprints', () => ({
  sprintsApi: {
    getByProject: vi.fn().mockResolvedValue({
      sprints: [
        { 
          id: 1, 
          name: 'Sprint 1', 
          status: 'Active',
          startDate: new Date().toISOString(),
          endDate: new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString(),
        },
        { 
          id: 2, 
          name: 'Sprint 2', 
          status: 'Planned',
          startDate: new Date(Date.now() + 15 * 24 * 60 * 60 * 1000).toISOString(),
          endDate: new Date(Date.now() + 29 * 24 * 60 * 60 * 1000).toISOString(),
        },
      ],
    }),
  },
}));

describe('CreateTaskDialog', () => {
  const defaultProps = {
    open: true,
    onOpenChange: vi.fn(),
    projectId: 1,
  };

  beforeEach(() => {
    server.resetHandlers();
    vi.clearAllMocks();
    mockToast.mockClear();
  });

  it('renders dialog when open', () => {
    render(<CreateTaskDialog {...defaultProps} />);

    expect(screen.getByText(/create task/i)).toBeInTheDocument();
  });

  it('does not render when closed', () => {
    render(<CreateTaskDialog {...defaultProps} open={false} />);

    expect(screen.queryByText(/create task/i)).not.toBeInTheDocument();
  });

  it('validates required fields', async () => {
    const user = userEvent.setup();
    render(<CreateTaskDialog {...defaultProps} />);

    const submitButton = screen.getByRole('button', { name: /create task/i });
    await user.click(submitButton);

    // Form validation should show errors
    await waitFor(() => {
      const titleInput = screen.getByLabelText(/title/i);
      expect(titleInput).toBeInvalid();
    });
  });

  it('creates task successfully', async () => {
    const user = userEvent.setup();
    const onOpenChange = vi.fn();

    server.use(
      http.post('http://localhost:5001/api/v1/Tasks', () => {
        return HttpResponse.json({
          id: 1,
          projectId: 1,
          title: 'New Task',
          description: 'Task description',
          status: 'Todo',
          priority: 'Medium',
        }, { status: 201 });
      })
    );

    render(<CreateTaskDialog {...defaultProps} onOpenChange={onOpenChange} />);

    const titleInput = screen.getByLabelText(/title/i);
    await user.type(titleInput, 'New Task');

    const descriptionInput = screen.getByLabelText(/description/i);
    await user.type(descriptionInput, 'Task description');

    const submitButton = screen.getByRole('button', { name: /create task/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(onOpenChange).toHaveBeenCalledWith(false);
    });
  });

  it('handles creation error', async () => {
    const user = userEvent.setup();

    server.use(
      http.post('http://localhost:5001/api/v1/Tasks', () => {
        return HttpResponse.json(
          { error: 'Creation failed' },
          { status: 400 }
        );
      })
    );

    render(<CreateTaskDialog {...defaultProps} />);

    const titleInput = screen.getByLabelText(/title/i);
    await user.type(titleInput, 'New Task');

    const submitButton = screen.getByRole('button', { name: /create task/i });
    await user.click(submitButton);

    // Verify that toast was called with error message
    await waitFor(() => {
      expect(mockToast).toHaveBeenCalledWith(
        expect.objectContaining({
          title: 'Failed to create task',
          variant: 'destructive',
        })
      );
    });
  });

  it('closes dialog on cancel', async () => {
    const user = userEvent.setup();
    const onOpenChange = vi.fn();

    render(<CreateTaskDialog {...defaultProps} onOpenChange={onOpenChange} />);

    const cancelButton = screen.getByRole('button', { name: /cancel/i });
    await user.click(cancelButton);

    expect(onOpenChange).toHaveBeenCalledWith(false);
  });
});

