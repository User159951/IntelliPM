import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@/test/test-utils';
import { MainLayout } from './MainLayout';
import { http, HttpResponse } from 'msw';
import { server } from '@/mocks/server';

// Mock the child components
vi.mock('./AppSidebar', () => ({
  AppSidebar: () => <div data-testid="app-sidebar">Sidebar</div>,
}));

vi.mock('./Header', () => ({
  Header: ({ onSearchClick }: { onSearchClick: () => void }) => (
    <div data-testid="header">
      <button onClick={onSearchClick}>Search</button>
    </div>
  ),
}));

vi.mock('@/components/search/GlobalSearchModal', () => ({
  GlobalSearchModal: ({ open }: { open: boolean; onOpenChange: (open: boolean) => void }) =>
    open ? <div data-testid="search-modal">Search Modal</div> : null,
}));

describe('MainLayout', () => {
  beforeEach(() => {
    server.resetHandlers();
  });

  it('shows loading state while checking authentication', () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', async () => {
        // Delay response to test loading state
        await new Promise(resolve => setTimeout(resolve, 100));
        return HttpResponse.json({ userId: 1, username: 'test', email: 'test@example.com' });
      })
    );

    render(<MainLayout />);

    // The loading state shows skeletons, not text
    const skeletons = document.querySelectorAll('.animate-pulse');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it('redirects to login when not authenticated (routing guard)', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json(
          { error: 'Unauthorized' },
          { status: 401 }
        );
      })
    );

    render(<MainLayout />);

    await waitFor(() => {
      // Navigate component should redirect to /login
      // Check that we're not showing the authenticated layout
      expect(screen.queryByTestId('app-sidebar')).not.toBeInTheDocument()
      expect(screen.queryByTestId('header')).not.toBeInTheDocument()
    }, { timeout: 3000 });
  });

  it('renders layout when authenticated', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json({
          userId: 1,
          username: 'testuser',
          email: 'test@example.com',
          roles: ['User'],
        });
      })
    );

    render(<MainLayout />);

    await waitFor(() => {
      expect(screen.getByTestId('app-sidebar')).toBeInTheDocument();
      expect(screen.getByTestId('header')).toBeInTheDocument();
    });
  });

  it('opens search modal on Ctrl+K', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json({
          userId: 1,
          username: 'testuser',
          email: 'test@example.com',
          roles: ['User'],
        });
      })
    );

    render(<MainLayout />);

    await waitFor(() => {
      expect(screen.getByTestId('header')).toBeInTheDocument();
    });

    // Simulate Ctrl+K
    const event = new KeyboardEvent('keydown', {
      key: 'k',
      ctrlKey: true,
      bubbles: true,
    });
    window.dispatchEvent(event);

    await waitFor(() => {
      expect(screen.getByTestId('search-modal')).toBeInTheDocument();
    });
  });
});

