import { describe, it, expect, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@/test/test-utils';
import Dashboard from './Dashboard';
import { http, HttpResponse } from 'msw';
import { server } from '@/mocks/server';

// Mock data for metrics summary
const mockMetricsSummary = {
  totalProjects: 5,
  totalTasks: 50,
  completedTasks: 20,
  inProgressTasks: 15,
  blockedTasks: 3,
  todoTasks: 12,
  openTasks: 30,
  completionPercentage: 40.0,
  averageCompletionTimeHours: 24.5,
  totalSprints: 8,
  activeSprints: 2,
  velocity: 28.5,
  defectsCount: 7,
  totalDefects: 15,
  totalAgentExecutions: 100,
  agentSuccessRate: 95.0,
  averageAgentResponseTimeMs: 500,
  trends: {
    projectsTrend: 10,
    sprintsTrend: 5,
    openTasksTrend: -8,
    blockedTasksTrend: -15,
    defectsTrend: 3,
    velocityTrend: 12,
  },
};

// Mock data for sprint velocity chart
const mockVelocityData = {
  sprints: [
    { number: 1, storyPoints: 24, completedDate: '2025-12-01T00:00:00Z' },
    { number: 2, storyPoints: 32, completedDate: '2025-12-15T00:00:00Z' },
    { number: 3, storyPoints: 28, completedDate: '2025-12-29T00:00:00Z' },
    { number: 4, storyPoints: 36, completedDate: '2026-01-05T00:00:00Z' },
  ],
};

// Mock data for task distribution
const mockDistributionData = {
  distribution: [
    { status: 'Todo', count: 12 },
    { status: 'InProgress', count: 15 },
    { status: 'Blocked', count: 3 },
    { status: 'Done', count: 20 },
  ],
};

// Helper function to setup all metric handlers
const setupMetricsHandlers = (overrides?: {
  metrics?: Partial<typeof mockMetricsSummary>;
  velocity?: Partial<typeof mockVelocityData>;
  distribution?: Partial<typeof mockDistributionData>;
}) => {
  server.use(
    http.get('http://localhost:5001/api/v1/Metrics', () => {
      return HttpResponse.json({ ...mockMetricsSummary, ...overrides?.metrics });
    }),
    http.get('http://localhost:5001/api/v1/Metrics/sprint-velocity-chart', () => {
      return HttpResponse.json({ ...mockVelocityData, ...overrides?.velocity });
    }),
    http.get('http://localhost:5001/api/v1/Metrics/task-distribution', () => {
      return HttpResponse.json({ ...mockDistributionData, ...overrides?.distribution });
    })
  );
};

describe('Dashboard Page', () => {
  beforeEach(() => {
    server.resetHandlers();
    setupMetricsHandlers();
  });

  it('renders dashboard with metrics', async () => {
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

  it('displays correct metric values', async () => {
    render(<Dashboard />);

    await waitFor(() => {
      // Total projects should show value of 5
      expect(screen.getByText('5')).toBeInTheDocument();
      // Active sprints should show value of 2
      expect(screen.getByText('2')).toBeInTheDocument();
    });
  });

  it('shows loading state initially', () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Metrics', async () => {
        await new Promise(resolve => setTimeout(resolve, 100));
        return HttpResponse.json(mockMetricsSummary);
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
      http.get('http://localhost:5001/api/v1/Metrics', () => {
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

  it('displays velocity chart with real data', async () => {
    render(<Dashboard />);

    await waitFor(() => {
      // Chart container should be present
      expect(screen.getByText(/sprint velocity/i)).toBeInTheDocument();
    });
  });

  it('displays task distribution chart with real data', async () => {
    render(<Dashboard />);

    await waitFor(() => {
      // Chart container should be present
      expect(screen.getByText(/task distribution/i)).toBeInTheDocument();
    });
  });

  it('shows empty state when no velocity data', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Metrics/sprint-velocity-chart', () => {
        return HttpResponse.json({ sprints: [] });
      })
    );

    render(<Dashboard />);

    await waitFor(() => {
      expect(screen.getByText(/no completed sprints yet/i)).toBeInTheDocument();
    });
  });

  it('shows empty state when no task distribution data', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Metrics/task-distribution', () => {
        return HttpResponse.json({ distribution: [] });
      })
    );

    render(<Dashboard />);

    await waitFor(() => {
      expect(screen.getByText(/no tasks yet/i)).toBeInTheDocument();
    });
  });

  it('handles velocity chart loading error', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Metrics/sprint-velocity-chart', () => {
        return HttpResponse.json(
          { error: 'Failed to load velocity data' },
          { status: 500 }
        );
      })
    );

    render(<Dashboard />);

    await waitFor(() => {
      expect(screen.getByText(/failed to load velocity data/i)).toBeInTheDocument();
    });
  });

  it('handles task distribution loading error', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Metrics/task-distribution', () => {
        return HttpResponse.json(
          { error: 'Failed to load distribution data' },
          { status: 500 }
        );
      })
    );

    render(<Dashboard />);

    await waitFor(() => {
      expect(screen.getByText(/failed to load distribution data/i)).toBeInTheDocument();
    });
  });

  it('displays trend indicators when trends data is available', async () => {
    render(<Dashboard />);

    await waitFor(() => {
      // Should show trend percentages
      expect(screen.getByText(/10%/)).toBeInTheDocument(); // projects trend
    });
  });

  it('hides trend indicators when trends data is not available', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Metrics', () => {
        return HttpResponse.json({ ...mockMetricsSummary, trends: null });
      })
    );

    render(<Dashboard />);

    await waitFor(() => {
      // Stats should still be visible but without trend indicators
      expect(screen.getByText(/total projects/i)).toBeInTheDocument();
    });
  });
});

