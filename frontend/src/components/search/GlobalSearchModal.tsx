import React, { useState, useEffect, useRef, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useDebounce } from '@/hooks/use-debounce';
import { searchApi, type SearchResult } from '@/api/search';
import {
  Dialog,
  DialogContent,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandList,
} from '@/components/ui/command';
import { Skeleton } from '@/components/ui/skeleton';
import { FolderKanban, ListTodo, User } from 'lucide-react';
import { cn } from '@/lib/utils';

interface GlobalSearchModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

const getResultIcon = (type: string) => {
  switch (type) {
    case 'project':
      return FolderKanban;
    case 'task':
      return ListTodo;
    case 'user':
      return User;
    default:
      return FolderKanban;
  }
};


export function GlobalSearchModal({ open, onOpenChange }: GlobalSearchModalProps) {
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedIndex, setSelectedIndex] = useState(0);
  const debouncedQuery = useDebounce(searchQuery, 300);
  const inputRef = useRef<HTMLInputElement>(null);

  const { data, isLoading } = useQuery({
    queryKey: ['search', debouncedQuery],
    queryFn: () => searchApi.search(debouncedQuery, 20),
    enabled: debouncedQuery.length >= 2,
  });

  const results = useMemo(() => data?.results || [], [data?.results]);
  const groupedResults = useMemo(() => ({
    projects: results.filter(r => r.type === 'project'),
    tasks: results.filter(r => r.type === 'task'),
    users: results.filter(r => r.type === 'user'),
  }), [results]);

  // Reset selected index when results change
  useEffect(() => {
    setSelectedIndex(0);
  }, [results.length]);

  // Reset state when modal closes
  useEffect(() => {
    if (!open) {
      setSearchQuery('');
      setSelectedIndex(0);
    }
  }, [open]);

  // Handle keyboard navigation
  useEffect(() => {
    if (!open) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'ArrowDown') {
        e.preventDefault();
        setSelectedIndex((prev) => Math.min(prev + 1, results.length - 1));
      } else if (e.key === 'ArrowUp') {
        e.preventDefault();
        setSelectedIndex((prev) => Math.max(prev - 1, 0));
      } else if (e.key === 'Enter' && results.length > 0 && selectedIndex >= 0) {
        e.preventDefault();
        const selectedResult = results[selectedIndex];
        if (selectedResult?.url) {
          navigate(selectedResult.url);
          onOpenChange(false);
        }
      } else if (e.key === 'Escape') {
        e.preventDefault();
        onOpenChange(false);
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [open, results, selectedIndex, navigate, onOpenChange]);

  const highlightText = (text: string, query: string): React.ReactNode => {
    if (!query) return text;
    // Escape special regex characters
    const escapedQuery = query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    const parts = text.split(new RegExp(`(${escapedQuery})`, 'gi'));
    return parts.map((part, i) =>
      part.toLowerCase() === query.toLowerCase() ? (
        <mark key={i} className="bg-yellow-200 dark:bg-yellow-900 rounded px-1">
          {part}
        </mark>
      ) : (
        part
      )
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange} modal={true}>
      <DialogContent 
        className="max-w-2xl p-0 overflow-hidden"
        onOpenAutoFocus={(e) => {
          // Prevent default auto-focus since we handle it manually
          e.preventDefault();
          // Focus input after dialog is fully mounted
          requestAnimationFrame(() => {
            inputRef.current?.focus();
          });
        }}
      >
        <DialogTitle className="sr-only">Search</DialogTitle>
        <Command className="rounded-lg border-none">
          <CommandInput
            ref={inputRef}
            placeholder="Search projects, tasks, users..."
            value={searchQuery}
            onValueChange={setSearchQuery}
            className="h-12 text-base"
          />
          <CommandList className="max-h-[400px] overflow-y-auto">
            {isLoading && debouncedQuery.length >= 2 ? (
              <div className="p-4 space-y-2">
                {[1, 2, 3].map((i) => (
                  <Skeleton key={i} className="h-12 w-full" />
                ))}
              </div>
            ) : debouncedQuery.length < 2 ? (
              <CommandEmpty>
                <div className="py-6 text-center text-sm text-muted-foreground">
                  Type at least 2 characters to search
                </div>
              </CommandEmpty>
            ) : results.length === 0 ? (
              <CommandEmpty>
                <div className="py-6 text-center text-sm text-muted-foreground">
                  No results found for &quot;{debouncedQuery}&quot;
                </div>
              </CommandEmpty>
            ) : (
              <>
                {groupedResults.projects.length > 0 && (
                  <CommandGroup heading="Projects">
                    {groupedResults.projects.map((result) => {
                      const globalIdx = results.indexOf(result);
                      const Icon = getResultIcon(result.type);
                      return (
                        <button
                          key={`project-${result.id}`}
                          type="button"
                          onClick={() => {
                            if (result.url) {
                              navigate(result.url);
                              onOpenChange(false);
                            }
                          }}
                          onKeyDown={(e) => {
                            if (e.key === 'Enter' || e.key === ' ') {
                              e.preventDefault();
                              if (result.url) {
                                navigate(result.url);
                                onOpenChange(false);
                              }
                            }
                          }}
                          className={cn(
                            'w-full text-left px-4 py-3 hover:bg-accent focus-visible:bg-accent focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring rounded-md transition-colors',
                            globalIdx === selectedIndex && 'bg-accent'
                          )}
                          aria-label={`Navigate to ${result.title || result.name || 'result'}`}
                        >
                          <div className="flex items-center gap-3">
                            <Icon className="h-4 w-4 text-muted-foreground" />
                            <div className="flex-1">
                              <div className="font-medium">
                                {highlightText(result.title, debouncedQuery)}
                              </div>
                              {result.description && (
                                <div className="text-xs text-muted-foreground line-clamp-1">
                                  {highlightText(result.description, debouncedQuery)}
                                </div>
                              )}
                              {result.subtitle && (
                                <div className="text-xs text-muted-foreground mt-0.5">
                                  {result.subtitle}
                                </div>
                              )}
                            </div>
                          </div>
                        </button>
                      );
                    })}
                  </CommandGroup>
                )}

                {groupedResults.tasks.length > 0 && (
                  <CommandGroup heading="Tasks">
                    {groupedResults.tasks.map((result) => {
                      const globalIdx = results.indexOf(result);
                      const Icon = getResultIcon(result.type);
                      return (
                        <button
                          key={`task-${result.id}`}
                          type="button"
                          onClick={() => {
                            if (result.url) {
                              navigate(result.url);
                              onOpenChange(false);
                            }
                          }}
                          onKeyDown={(e) => {
                            if (e.key === 'Enter' || e.key === ' ') {
                              e.preventDefault();
                              if (result.url) {
                                navigate(result.url);
                                onOpenChange(false);
                              }
                            }
                          }}
                          className={cn(
                            'w-full text-left px-4 py-3 hover:bg-accent focus-visible:bg-accent focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring rounded-md transition-colors',
                            globalIdx === selectedIndex && 'bg-accent'
                          )}
                          aria-label={`Navigate to ${result.title || result.name || 'result'}`}
                        >
                          <div className="flex items-center gap-3">
                            <Icon className="h-4 w-4 text-muted-foreground" />
                            <div className="flex-1">
                              <div className="font-medium">
                                {highlightText(result.title, debouncedQuery)}
                              </div>
                              {result.description && (
                                <div className="text-xs text-muted-foreground line-clamp-1">
                                  {highlightText(result.description, debouncedQuery)}
                                </div>
                              )}
                              {result.subtitle && (
                                <div className="text-xs text-muted-foreground mt-0.5">
                                  {result.subtitle}
                                </div>
                              )}
                            </div>
                          </div>
                        </button>
                      );
                    })}
                  </CommandGroup>
                )}

                {groupedResults.users.length > 0 && (
                  <CommandGroup heading="Users">
                    {groupedResults.users.map((result) => {
                      const globalIdx = results.indexOf(result);
                      const Icon = getResultIcon(result.type);
                      return (
                        <button
                          key={`user-${result.id}`}
                          type="button"
                          onClick={() => {
                            if (result.url) {
                              navigate(result.url);
                              onOpenChange(false);
                            }
                          }}
                          onKeyDown={(e) => {
                            if (e.key === 'Enter' || e.key === ' ') {
                              e.preventDefault();
                              if (result.url) {
                                navigate(result.url);
                                onOpenChange(false);
                              }
                            }
                          }}
                          className={cn(
                            'w-full text-left px-4 py-3 hover:bg-accent focus-visible:bg-accent focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring rounded-md transition-colors',
                            globalIdx === selectedIndex && 'bg-accent'
                          )}
                          aria-label={`Navigate to ${result.title || result.name || 'result'}`}
                        >
                          <div className="flex items-center gap-3">
                            <Icon className="h-4 w-4 text-muted-foreground" />
                            <div className="flex-1">
                              <div className="font-medium">
                                {highlightText(result.title, debouncedQuery)}
                              </div>
                              {result.description && (
                                <div className="text-xs text-muted-foreground">
                                  {result.description}
                                </div>
                              )}
                              {result.subtitle && (
                                <div className="text-xs text-muted-foreground mt-0.5">
                                  {result.subtitle}
                                </div>
                              )}
                            </div>
                          </div>
                        </button>
                      );
                    })}
                  </CommandGroup>
                )}
              </>
            )}
          </CommandList>
        </Command>
      </DialogContent>
    </Dialog>
  );
}
