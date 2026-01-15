import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { PageLoader } from './PageLoader';

describe('PageLoader', () => {
  it('renders with default message', () => {
    render(<PageLoader />);
    
    expect(screen.getByText('Loading...')).toBeInTheDocument();
    expect(screen.getByRole('status')).toBeInTheDocument();
  });

  it('renders with custom message', () => {
    render(<PageLoader message="Please wait..." />);
    
    expect(screen.getByText('Please wait...')).toBeInTheDocument();
  });

  it('renders loader icon', () => {
    render(<PageLoader />);
    
    const loader = screen.getByRole('status').querySelector('.animate-spin');
    expect(loader).toBeInTheDocument();
  });

  it('renders with small size', () => {
    const { container } = render(<PageLoader size="sm" />);
    
    const loader = container.querySelector('.h-4');
    expect(loader).toBeInTheDocument();
  });

  it('renders with medium size', () => {
    const { container } = render(<PageLoader size="md" />);
    
    const loader = container.querySelector('.h-8');
    expect(loader).toBeInTheDocument();
  });

  it('renders with large size by default', () => {
    const { container } = render(<PageLoader />);
    
    const loader = container.querySelector('.h-12');
    expect(loader).toBeInTheDocument();
  });

  it('has proper accessibility attributes', () => {
    render(<PageLoader message="Loading data" />);
    
    const status = screen.getByRole('status');
    expect(status).toHaveAttribute('aria-live', 'polite');
    expect(status).toHaveAttribute('aria-label', 'Loading data');
  });

  it('applies custom className', () => {
    const { container } = render(
      <PageLoader className="custom-class" />
    );
    
    expect(container.firstChild).toHaveClass('custom-class');
  });

  it('renders without message when message is empty string', () => {
    render(<PageLoader message="" />);
    
    expect(screen.queryByText('Loading...')).not.toBeInTheDocument();
    // But loader icon should still be present
    const status = screen.getByRole('status');
    expect(status).toBeInTheDocument();
  });
});
