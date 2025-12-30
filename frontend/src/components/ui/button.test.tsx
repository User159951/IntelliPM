import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@/test/test-utils'
import { Button } from './button'
import userEvent from '@testing-library/user-event'

describe('Button', () => {
  it('renders children correctly', () => {
    render(<Button>Click me</Button>)
    expect(screen.getByRole('button', { name: /click me/i })).toBeInTheDocument()
  })

  it('handles click events', async () => {
    const handleClick = vi.fn()
    render(<Button onClick={handleClick}>Click me</Button>)

    const button = screen.getByRole('button')
    await userEvent.click(button)

    expect(handleClick).toHaveBeenCalledTimes(1)
  })

  it('is disabled when disabled prop is true', () => {
    render(<Button disabled>Click me</Button>)
    expect(screen.getByRole('button')).toBeDisabled()
  })

  it('applies variant styles correctly', () => {
    const { container } = render(<Button variant="destructive">Delete</Button>)
    const button = container.querySelector('button')
    expect(button).toHaveClass('bg-destructive')
  })

  it('applies size styles correctly', () => {
    const { container } = render(<Button size="lg">Large Button</Button>)
    const button = container.querySelector('button')
    expect(button).toHaveClass('h-11')
  })
})

