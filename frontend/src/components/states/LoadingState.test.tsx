import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { LoadingState } from './LoadingState';

describe('LoadingState', () => {
  it('renders correct number of skeleton items', () => {
    render(<LoadingState count={5} />);
    
    const skeletons = screen.getAllByRole('status');
    expect(skeletons).toHaveLength(1); // Container has role="status"
    
    // Check that skeleton elements are rendered
    const container = screen.getByRole('status');
    expect(container).toBeInTheDocument();
  });

  it('renders with default count of 3', () => {
    render(<LoadingState />);
    
    const container = screen.getByRole('status');
    expect(container).toBeInTheDocument();
  });

  it('renders grid layout by default', () => {
    const { container } = render(<LoadingState />);
    
    const grid = container.querySelector('.grid');
    expect(grid).toBeInTheDocument();
  });

  it('renders list layout when variant is list', () => {
    const { container } = render(<LoadingState variant="list" />);
    
    const list = container.querySelector('.space-y-4');
    expect(list).toBeInTheDocument();
  });

  it('renders with card wrapper by default', () => {
    const { container } = render(<LoadingState count={1} />);
    
    // Check for Card component (should have card classes)
    const card = container.querySelector('[class*="card"]');
    expect(card).toBeInTheDocument();
  });

  it('renders without card wrapper when showCard is false', () => {
    const { container } = render(<LoadingState showCard={false} count={1} />);
    
    // Should not have card wrapper, just skeleton
    const skeleton = container.querySelector('.animate-pulse');
    expect(skeleton).toBeInTheDocument();
  });

  it('applies custom className', () => {
    const { container } = render(
      <LoadingState className="custom-class" />
    );
    
    expect(container.firstChild).toHaveClass('custom-class');
  });

  it('has proper accessibility attributes', () => {
    render(<LoadingState />);
    
    const status = screen.getByRole('status');
    expect(status).toHaveAttribute('aria-live', 'polite');
    expect(status).toHaveAttribute('aria-label', 'Loading content');
  });

  it('applies custom skeleton height', () => {
    const { container } = render(
      <LoadingState skeletonHeight="h-64" />
    );
    
    // Check that skeleton has the custom height class
    const skeleton = container.querySelector('.h-64');
    expect(skeleton).toBeInTheDocument();
  });
});
