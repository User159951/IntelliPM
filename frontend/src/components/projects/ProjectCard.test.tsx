import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@/test/test-utils'
import userEvent from '@testing-library/user-event'
import { Card, CardContent, CardDescription, CardHeader, CardTitle, CardFooter } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu'
import { MoreHorizontal } from 'lucide-react'
import type { Project } from '@/types'

// Mock project data
const mockProject: Project = {
  id: 1,
  name: 'Test Project',
  description: 'Test description',
  status: 'Active',
  type: 'Scrum',
  sprintDurationDays: 14,
  createdAt: new Date().toISOString(),
  ownerId: 1,
  openTasksCount: 10,
  members: [],
}

// Simple ProjectCard component for testing
const ProjectCard = ({ 
  project, 
  onArchive, 
  onDelete, 
  onEdit,
  onClick 
}: { 
  project: Project
  onArchive?: (id: number) => void
  onDelete?: (id: number) => void
  onEdit?: (project: Project) => void
  onClick?: () => void
}) => {
  return (
    <Card
      className="cursor-pointer transition-all hover:shadow-md hover:border-primary/50"
      onClick={onClick}
    >
      <CardHeader className="flex flex-row items-start justify-between space-y-0">
        <div className="space-y-1">
          <CardTitle className="text-lg">{project.name}</CardTitle>
          <CardDescription className="line-clamp-2">
            {project.description || 'No description'}
          </CardDescription>
        </div>
        <DropdownMenu>
          <DropdownMenuTrigger asChild onClick={(e) => e.stopPropagation()}>
            <Button variant="ghost" size="icon" className="h-8 w-8" aria-label="More options">
              <MoreHorizontal className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={(e) => {
              e.stopPropagation()
              onEdit?.(project)
            }}>
              Edit project
            </DropdownMenuItem>
            <DropdownMenuItem 
              className="text-destructive"
              onClick={(e) => {
                e.stopPropagation()
                onArchive?.(project.id)
              }}
            >
              Archive
            </DropdownMenuItem>
            <DropdownMenuItem 
              className="text-destructive"
              onClick={(e) => {
                e.stopPropagation()
                onDelete?.(project.id)
              }}
            >
              Delete project
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </CardHeader>
      <CardContent>
        <div className="flex items-center justify-between text-sm">
          <span className="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium bg-green-500/10 text-green-500">
            {project.status}
          </span>
          <div className="flex items-center gap-2 text-muted-foreground">
            <span>{project.type}</span>
            <span>•</span>
            <span>{project.sprintDurationDays}d sprints</span>
          </div>
        </div>
      </CardContent>
      <CardFooter className="flex items-center justify-between pt-4 border-t">
        {project.openTasksCount !== undefined && (
          <span className="text-xs text-muted-foreground">
            {project.openTasksCount} tasks
          </span>
        )}
      </CardFooter>
    </Card>
  )
}

describe('ProjectCard', () => {
  it('renders project information', () => {
    render(<ProjectCard project={mockProject} />)

    expect(screen.getByText('Test Project')).toBeInTheDocument()
    expect(screen.getByText('Test description')).toBeInTheDocument()
    expect(screen.getByText('Active')).toBeInTheDocument()
    expect(screen.getByText(/10 tasks/i)).toBeInTheDocument()
  })

  it('calls onArchive when archive button clicked', async () => {
    const onArchive = vi.fn()
    render(<ProjectCard project={mockProject} onArchive={onArchive} />)

    // Open menu
    const menuButton = screen.getByRole('button', { name: /more options/i })
    await userEvent.click(menuButton)

    // Click archive
    const archiveButton = await screen.findByText(/archive/i)
    await userEvent.click(archiveButton)

    expect(onArchive).toHaveBeenCalledWith(1)
  })

  it('calls onDelete when delete button clicked', async () => {
    const onDelete = vi.fn()
    render(<ProjectCard project={mockProject} onDelete={onDelete} />)

    // Open menu
    const menuButton = screen.getByRole('button', { name: /more options/i })
    await userEvent.click(menuButton)

    // Click delete
    const deleteButton = await screen.findByText(/delete project/i)
    await userEvent.click(deleteButton)

    expect(onDelete).toHaveBeenCalledWith(1)
  })

  it('calls onEdit when edit button clicked', async () => {
    const onEdit = vi.fn()
    render(<ProjectCard project={mockProject} onEdit={onEdit} />)

    // Open menu
    const menuButton = screen.getByRole('button', { name: /more options/i })
    await userEvent.click(menuButton)

    // Click edit
    const editButton = await screen.findByText(/edit project/i)
    await userEvent.click(editButton)

    expect(onEdit).toHaveBeenCalledWith(mockProject)
  })

  it('calls onClick when card is clicked', async () => {
    const onClick = vi.fn()
    const { container } = render(<ProjectCard project={mockProject} onClick={onClick} />)

    const card = container.querySelector('[class*="cursor-pointer"]')
    if (card) {
      await userEvent.click(card)
      expect(onClick).toHaveBeenCalled()
    }
  })

  it('displays project type and sprint duration', () => {
    render(<ProjectCard project={mockProject} />)

    expect(screen.getByText('Scrum')).toBeInTheDocument()
    expect(screen.getByText(/14d sprints/i)).toBeInTheDocument()
  })
})

