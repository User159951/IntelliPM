import { describe, it, expect } from 'vitest'
import { render, screen } from '@/test/test-utils'
import { AppSidebar } from './AppSidebar'
import { SidebarProvider } from '@/components/ui/sidebar'

describe('AppSidebar', () => {

  it('renders all main navigation links', () => {
    render(
      <SidebarProvider>
        <AppSidebar />
      </SidebarProvider>
    )

    expect(screen.getByRole('link', { name: /dashboard/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /projects/i })).toBeInTheDocument()
  })

  it('renders all project navigation links', () => {
    render(
      <SidebarProvider>
        <AppSidebar />
      </SidebarProvider>
    )

    expect(screen.getByRole('link', { name: /tasks/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /sprints/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /backlog/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /defects/i })).toBeInTheDocument()
  })

  it('renders all team navigation links', () => {
    render(
      <SidebarProvider>
        <AppSidebar />
      </SidebarProvider>
    )

    expect(screen.getByRole('link', { name: /teams/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /metrics/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /insights/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /ai agents/i })).toBeInTheDocument()
  })

  it('has correct hrefs for all navigation links', () => {
    render(
      <SidebarProvider>
        <AppSidebar />
      </SidebarProvider>
    )

    // Main nav
    expect(screen.getByRole('link', { name: /dashboard/i })).toHaveAttribute('href', '/dashboard')
    expect(screen.getByRole('link', { name: /projects/i })).toHaveAttribute('href', '/projects')

    // Project nav
    expect(screen.getByRole('link', { name: /tasks/i })).toHaveAttribute('href', '/tasks')
    expect(screen.getByRole('link', { name: /sprints/i })).toHaveAttribute('href', '/sprints')
    expect(screen.getByRole('link', { name: /backlog/i })).toHaveAttribute('href', '/backlog')
    expect(screen.getByRole('link', { name: /defects/i })).toHaveAttribute('href', '/defects')

    // Team nav
    expect(screen.getByRole('link', { name: /teams/i })).toHaveAttribute('href', '/teams')
    expect(screen.getByRole('link', { name: /metrics/i })).toHaveAttribute('href', '/metrics')
    expect(screen.getByRole('link', { name: /insights/i })).toHaveAttribute('href', '/insights')
    expect(screen.getByRole('link', { name: /ai agents/i })).toHaveAttribute('href', '/agents')
  })

  it('displays IntelliPM branding', () => {
    render(
      <SidebarProvider>
        <AppSidebar />
      </SidebarProvider>
    )

    expect(screen.getByText('IntelliPM')).toBeInTheDocument()
  })
})

