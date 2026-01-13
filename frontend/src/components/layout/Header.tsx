import { Moon, Sun, LogOut, User, Search } from 'lucide-react';
import { useAuth } from '@/contexts/AuthContext';
import { useTheme } from '@/contexts/ThemeContext';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { SidebarTrigger } from '@/components/ui/sidebar';
import { useNavigate } from 'react-router-dom';
import NotificationBell from '@/components/notifications/NotificationBell';
import { LanguageToggle } from './LanguageToggle';
import { useTranslation } from 'react-i18next';

interface HeaderProps {
  onSearchClick?: () => void;
}

export function Header({ onSearchClick }: HeaderProps) {
  const { user, logout } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const navigate = useNavigate();
  const { t } = useTranslation('navigation');

  const initials = user
    ? `${user.firstName?.[0] || user.username[0]}${user.lastName?.[0] || ''}`
    : 'U';

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <header className="sticky top-0 z-50 flex h-14 items-center gap-4 border-b border-border bg-card px-4">
      <SidebarTrigger />
      
      <div className="flex flex-1 items-center gap-4">
        <div className="hidden w-full max-w-sm md:flex">
          <button
            type="button"
            onClick={onSearchClick}
            className="flex h-9 w-full items-center gap-2 rounded-md border border-input bg-background px-3 py-1 text-sm text-muted-foreground hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring transition-colors"
            aria-label="Open global search"
          >
            <Search className="h-4 w-4 shrink-0" />
            <span className="flex-1 text-left">{t('header.searchPlaceholder')}</span>
            <kbd className="pointer-events-none hidden h-5 select-none items-center gap-1 rounded border bg-muted px-1.5 font-mono text-[10px] font-medium opacity-100 sm:flex">
              {navigator.platform.toUpperCase().indexOf('MAC') >= 0 ? (
                <>
                  <span className="text-xs">⌘</span>K
                </>
              ) : (
                <>
                  <span className="text-xs">Ctrl</span>+K
                </>
              )}
            </kbd>
          </button>
        </div>
      </div>

      <div className="flex items-center gap-2">
        <NotificationBell />

        <LanguageToggle />

        <Button variant="ghost" size="icon" onClick={toggleTheme}>
          {theme === 'dark' ? (
            <Sun className="h-4 w-4" />
          ) : (
            <Moon className="h-4 w-4" />
          )}
        </Button>

        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" className="relative h-8 w-8 rounded-full">
              <Avatar className="h-8 w-8">
                <AvatarFallback className="bg-primary text-primary-foreground text-xs">
                  {initials.toUpperCase()}
                </AvatarFallback>
              </Avatar>
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent className="w-56" align="end" forceMount>
            <DropdownMenuLabel className="font-normal">
              <div className="flex flex-col space-y-1">
                <p className="text-sm font-medium leading-none">
                  {user?.firstName} {user?.lastName}
                </p>
                <p className="text-xs leading-none text-muted-foreground">
                  {user?.email}
                </p>
              </div>
            </DropdownMenuLabel>
            <DropdownMenuSeparator />
            <DropdownMenuItem onClick={() => navigate('/profile')}>
              <User className="mr-2 h-4 w-4" />
              <span>{t('header.profile')}</span>
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem onClick={handleLogout} className="text-destructive">
              <LogOut className="mr-2 h-4 w-4" />
              <span>{t('header.logOut')}</span>
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </header>
  );
}
