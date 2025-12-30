import { describe, it, expect, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@/test/test-utils';
import Dashboard from './Dashboard';
import { http, HttpResponse } from 'msw';
import { server } from '@/mocks/server';

describe('Dashboard Page', () => {
  beforeEach(() => {
    server.resetHandlers();
  });

  it('renders dashboard with metrics', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Metrics/summary', () => {
        return HttpResponse.json({
          totalTasks: 10,
          completedTasks: 5,
          inProgressTasks: 3,
          blockedTasks: 1,
          todoTasks: 1,
          completionPercentage: 50.0,
          averageCompletionTimeHours: 24.5,
          totalSprints: 5,
          activeSprints: 2,
          totalAgentExecutions: 100,
          agentSuccessRate: 95.0,
          averageAgentResponseTimeMs: 500,
        });
      })
    );

    render(<Dashboard />);

    await waitFor(() => {
      expect(screen.getByText(/dashboard/i)).toBeInTheDocument();
    });

    // Should show metrics
    await waitFor(() => {
      expect(screen.getByText(/total projects/i)).toBeInTheDocument();
      expect(screen.getByText(/active sprints/i)).toBeInTheDocument();
    });
  });

  it('shows loading state initially', () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Metrics/summary', async () => {
        await new Promise(resolve => setTimeout(resolve, 100));
        return HttpResponse.json({
          totalTasks: 10,
          completedTasks: 5,
        });
      })
    );

    render(<Dashboard />);

    // Should show loading skeletons - check for Skeleton component class
    const skeletons = document.querySelectorAll('.animate-pulse');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it('displays recent activity', async () => {
    render(<Dashboard />);

    await waitFor(() => {
      expect(screen.getByText(/recent activity/i)).toBeInTheDocument();
    });
  });

  it('handles metrics loading error gracefully', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Metrics/summary', () => {
        return HttpResponse.json(
          { error: 'Failed to load metrics' },
          { status: 500 }
        );
      })
    );

    render(<Dashboard />);

    await waitFor(() => {
      // Should still render dashboard even if metrics fail
      expect(screen.getByText(/dashboard/i)).toBeInTheDocument();
    });
  });
});

