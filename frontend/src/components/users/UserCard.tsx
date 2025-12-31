import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Card, CardContent, CardHeader, CardFooter } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { formatDistanceToNow } from 'date-fns';
import { Mail, LogIn, Edit, Trash2 } from 'lucide-react';
import { RoleBadge } from './RoleBadge';
import { cn } from '@/lib/utils';
import type { UserListDto } from '@/api/users';

/**
 * User data interface for UserCard component.
 * Compatible with both the new interface and UserListDto.
 */
export interface UserCardUser {
  id: number;
  username: string;
  email: string;
  firstName?: string;
  lastName?: string;
  role?: 'Admin' | 'User';
  globalRole?: string; // For compatibility with UserListDto
  isActive: boolean;
  lastLoginAt?: string | null;
  createdAt?: string; // Optional for compatibility
  projectCount?: number; // Optional for compatibility
}

/**
 * Props for the UserCard component.
 */
export interface UserCardProps {
  /** User data to display */
  user: UserCardUser | UserListDto;
  /** Optional click handler for the card */
  onClick?: (userId: number) => void;
  /** Whether to show action buttons in footer */
  showActions?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * UserCard component for displaying user information in a card layout.
 * 
 * Features:
 * - Avatar with user initials
 * - User name and username
 * - Role badge (Admin/User)
 * - Email with icon
 * - Status indicator (Active/Inactive)
 * - Last login timestamp (relative time)
 * - Optional click handler
 * - Optional action buttons
 * - Keyboard accessible
 * - Responsive design
 * 
 * @example
 * ```tsx
 * <UserCard 
 *   user={userData} 
 *   onClick={(id) => navigate(`/users/${id}`)}
 *   showActions={true}
 * />
 * ```
 */
export function UserCard({ user, onClick, showActions = false, className }: UserCardProps) {
  // Normalize user data to handle both interfaces
  const normalizedUser: UserCardUser = {
    id: user.id,
    username: user.username,
    email: user.email,
    firstName: 'firstName' in user ? user.firstName : undefined,
    lastName: 'lastName' in user ? user.lastName : undefined,
    role: ('role' in user && user.role) 
      ? user.role as 'Admin' | 'User'
      : ('globalRole' in user && user.globalRole) 
        ? (user.globalRole === 'Admin' ? 'Admin' : 'User')
        : 'User',
    isActive: user.isActive,
    lastLoginAt: user.lastLoginAt ?? undefined,
    createdAt: 'createdAt' in user ? user.createdAt : undefined,
    projectCount: 'projectCount' in user ? user.projectCount : undefined,
  };

  const getInitials = (firstName?: string, lastName?: string, username?: string): string => {
    if (firstName && lastName) {
      return `${firstName[0]}${lastName[0]}`.toUpperCase();
    }
    if (firstName) {
      return firstName[0].toUpperCase();
    }
    if (username) {
      return username[0].toUpperCase();
    }
    return 'U';
  };

  const fullName = normalizedUser.firstName && normalizedUser.lastName
    ? `${normalizedUser.firstName} ${normalizedUser.lastName}`.trim()
    : normalizedUser.username;

  const handleClick = () => {
    if (onClick) {
      onClick(normalizedUser.id);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (onClick && (e.key === 'Enter' || e.key === ' ')) {
      e.preventDefault();
      handleClick();
    }
  };

  const handleActionClick = (e: React.MouseEvent, action: 'edit' | 'delete') => {
    e.stopPropagation();
    // Action handlers would be passed as props in a real implementation
    console.log(`${action} user ${normalizedUser.id}`);
  };

  const lastLoginText = normalizedUser.lastLoginAt
    ? `Last seen ${formatDistanceToNow(new Date(normalizedUser.lastLoginAt), { addSuffix: true })}`
    : 'Never logged in';

  const isClickable = !!onClick;

  return (
    <Card
      className={cn(
        'transition-all duration-200',
        isClickable && 'cursor-pointer hover:shadow-lg hover:scale-[1.02]',
        !normalizedUser.isActive && 'opacity-75 grayscale',
        className
      )}
      onClick={isClickable ? handleClick : undefined}
      onKeyDown={isClickable ? handleKeyDown : undefined}
      role={isClickable ? 'button' : undefined}
      tabIndex={isClickable ? 0 : undefined}
      aria-label={isClickable ? `View details for ${fullName}` : undefined}
    >
      <CardHeader className="pb-3">
        <div className="flex items-start gap-3">
          <Avatar className="h-12 w-12 flex-shrink-0">
            <AvatarFallback className="bg-primary text-primary-foreground">
              {getInitials(normalizedUser.firstName, normalizedUser.lastName, normalizedUser.username)}
            </AvatarFallback>
          </Avatar>
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-1 flex-wrap">
              <h3 className="font-semibold text-base truncate">{fullName}</h3>
              {normalizedUser.role && <RoleBadge role={normalizedUser.role} size="sm" />}
            </div>
            <p className="text-sm text-muted-foreground truncate">@{normalizedUser.username}</p>
          </div>
        </div>
      </CardHeader>

      <CardContent className="space-y-3">
        {/* Email */}
        <div className="flex items-center gap-2 text-sm">
          <Mail className="h-4 w-4 text-muted-foreground flex-shrink-0" aria-hidden="true" />
          <span className="text-muted-foreground truncate" title={normalizedUser.email}>
            {normalizedUser.email}
          </span>
        </div>

        {/* Status indicator */}
        <div className="flex items-center gap-2 text-sm">
          <div
            className={cn(
              'h-2 w-2 rounded-full flex-shrink-0',
              normalizedUser.isActive ? 'bg-green-500' : 'bg-gray-400'
            )}
            aria-label={normalizedUser.isActive ? 'Active' : 'Inactive'}
          />
          <span className="text-muted-foreground">
            {normalizedUser.isActive ? 'Active' : 'Inactive'}
          </span>
        </div>

        {/* Last login */}
        <div className="flex items-center gap-2 text-xs text-muted-foreground">
          <LogIn className="h-3 w-3 flex-shrink-0" aria-hidden="true" />
          <span>{lastLoginText}</span>
        </div>
      </CardContent>

      {/* Optional footer with actions */}
      {showActions && (
        <CardFooter className="pt-3 border-t flex items-center justify-end gap-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={(e) => handleActionClick(e, 'edit')}
            aria-label={`Edit ${fullName}`}
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={(e) => handleActionClick(e, 'delete')}
            aria-label={`Delete ${fullName}`}
            className="text-destructive hover:text-destructive"
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </CardFooter>
      )}
    </Card>
  );
}
