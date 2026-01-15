import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { EmptyState } from './EmptyState';
import { Inbox } from 'lucide-react';

describe('EmptyState', () => {
  it('renders correctly with all props', () => {
    const onClick = vi.fn();
    render(
      <EmptyState
        title="No items found"
        description="Try creating a new item"
        action={{
          label: 'Create Item',
          onClick,
        }}
      />
    );

    expect(screen.getByText('No items found')).toBeInTheDocument();
    expect(screen.getByText('Try creating a new item')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Create Item' })).toBeInTheDocument();
  });

  it('renders with default icon when no icon provided', () => {
    render(<EmptyState title="Empty" />);
    
    const icon = screen.getByRole('status');
    expect(icon).toBeInTheDocument();
  });

  it('renders with custom icon', () => {
    const CustomIcon = () => <div data-testid="custom-icon">Custom</div>;
    render(<EmptyState title="Empty" icon={<CustomIcon />} />);
    
    expect(screen.getByTestId('custom-icon')).toBeInTheDocument();
  });

  it('calls onClick when action button is clicked', async () => {
    const user = userEvent.setup();
    const onClick = vi.fn();
    
    render(
      <EmptyState
        title="Empty"
        action={{
          label: 'Click me',
          onClick,
        }}
      />
    );

    const button = screen.getByRole('button', { name: 'Click me' });
    await user.click(button);
    
    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it('renders without description when not provided', () => {
    render(<EmptyState title="Empty" />);
    
    expect(screen.getByText('Empty')).toBeInTheDocument();
    expect(screen.queryByText(/description/i)).not.toBeInTheDocument();
  });

  it('renders without action button when not provided', () => {
    render(<EmptyState title="Empty" />);
    
    expect(screen.queryByRole('button')).not.toBeInTheDocument();
  });

  it('has proper accessibility attributes', () => {
    render(<EmptyState title="Empty" />);
    
    const status = screen.getByRole('status');
    expect(status).toHaveAttribute('aria-live', 'polite');
  });

  it('applies custom className', () => {
    const { container } = render(
      <EmptyState title="Empty" className="custom-class" />
    );
    
    expect(container.firstChild).toHaveClass('custom-class');
  });
});
