import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ErrorState } from './ErrorState';
import { AlertCircle } from 'lucide-react';

describe('ErrorState', () => {
  it('renders correctly with all props', () => {
    const onRetry = vi.fn();
    render(
      <ErrorState
        title="Error occurred"
        message="Something went wrong"
        onRetry={onRetry}
        retryLabel="Try again"
      />
    );

    expect(screen.getByText('Error occurred')).toBeInTheDocument();
    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Try again' })).toBeInTheDocument();
  });

  it('renders with default title and message', () => {
    render(<ErrorState />);
    
    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    expect(screen.getByText(/We encountered an error/i)).toBeInTheDocument();
  });

  it('calls onRetry when retry button is clicked', async () => {
    const user = userEvent.setup();
    const onRetry = vi.fn();
    
    render(<ErrorState onRetry={onRetry} />);

    const button = screen.getByRole('button', { name: 'Try again' });
    await user.click(button);
    
    expect(onRetry).toHaveBeenCalledTimes(1);
  });

  it('renders without retry button when onRetry is not provided', () => {
    render(<ErrorState title="Error" />);
    
    expect(screen.queryByRole('button')).not.toBeInTheDocument();
  });

  it('renders custom retry label', () => {
    const onRetry = vi.fn();
    render(<ErrorState onRetry={onRetry} retryLabel="Retry now" />);
    
    expect(screen.getByRole('button', { name: 'Retry now' })).toBeInTheDocument();
  });

  it('renders with custom icon', () => {
    const CustomIcon = () => <div data-testid="custom-icon">Custom</div>;
    render(<ErrorState icon={<CustomIcon />} />);
    
    expect(screen.getByTestId('custom-icon')).toBeInTheDocument();
  });

  it('renders additional actions', () => {
    render(
      <ErrorState
        actions={
          <button type="button">Custom Action</button>
        }
      />
    );
    
    expect(screen.getByRole('button', { name: 'Custom Action' })).toBeInTheDocument();
  });

  it('has proper accessibility attributes', () => {
    render(<ErrorState />);
    
    const alert = screen.getByRole('alert');
    expect(alert).toHaveAttribute('aria-live', 'assertive');
  });

  it('applies custom className', () => {
    const { container } = render(
      <ErrorState className="custom-class" />
    );
    
    expect(container.firstChild).toHaveClass('custom-class');
  });
});
