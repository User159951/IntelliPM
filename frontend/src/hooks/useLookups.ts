import { useQuery } from '@tanstack/react-query';
import { lookupsApi, type LookupItem } from '@/api/lookups';

/**
 * Configuration for lookup queries
 */
const LOOKUP_STALE_TIME = 24 * 60 * 60 * 1000; // 24 hours - lookup data rarely changes
const LOOKUP_CACHE_TIME = 7 * 24 * 60 * 60 * 1000; // 7 days - keep in cache for a week

/**
 * Hook to fetch task statuses with metadata
 * @returns Lookup items sorted by displayOrder, plus loading/error states
 */
export function useTaskStatuses() {
  const {
    data,
    isLoading,
    isError,
    error,
    refetch,
  } = useQuery({
    queryKey: ['lookups', 'task-statuses'],
    queryFn: () => lookupsApi.getTaskStatuses(),
    staleTime: LOOKUP_STALE_TIME,
    gcTime: LOOKUP_CACHE_TIME, // Previously cacheTime
    retry: 2,
  });

  // Sort by displayOrder if available, otherwise maintain API order
  const items: LookupItem[] = data?.items
    ? [...data.items].sort((a, b) => (a.displayOrder ?? 999) - (b.displayOrder ?? 999))
    : [];

  return {
    statuses: items,
    isLoading,
    isError,
    error,
    refetch,
  };
}

/**
 * Hook to fetch task priorities with metadata
 * @returns Lookup items sorted by displayOrder, plus loading/error states
 */
export function useTaskPriorities() {
  const {
    data,
    isLoading,
    isError,
    error,
    refetch,
  } = useQuery({
    queryKey: ['lookups', 'task-priorities'],
    queryFn: () => lookupsApi.getTaskPriorities(),
    staleTime: LOOKUP_STALE_TIME,
    gcTime: LOOKUP_CACHE_TIME,
    retry: 2,
  });

  const items: LookupItem[] = data?.items
    ? [...data.items].sort((a, b) => (a.displayOrder ?? 999) - (b.displayOrder ?? 999))
    : [];

  return {
    priorities: items,
    isLoading,
    isError,
    error,
    refetch,
  };
}

/**
 * Hook to fetch project types with metadata
 * @returns Lookup items sorted by displayOrder, plus loading/error states
 */
export function useProjectTypes() {
  const {
    data,
    isLoading,
    isError,
    error,
    refetch,
  } = useQuery({
    queryKey: ['lookups', 'project-types'],
    queryFn: () => lookupsApi.getProjectTypes(),
    staleTime: LOOKUP_STALE_TIME,
    gcTime: LOOKUP_CACHE_TIME,
    retry: 2,
  });

  const items: LookupItem[] = data?.items
    ? [...data.items].sort((a, b) => (a.displayOrder ?? 999) - (b.displayOrder ?? 999))
    : [];

  return {
    projectTypes: items,
    isLoading,
    isError,
    error,
    refetch,
  };
}

/**
 * Helper function to get lookup item by value
 */
export function getLookupItem(items: LookupItem[], value: string): LookupItem | undefined {
  return items.find((item) => item.value === value);
}

/**
 * Helper function to get lookup label by value
 */
export function getLookupLabel(items: LookupItem[], value: string): string {
  return getLookupItem(items, value)?.label ?? value;
}

