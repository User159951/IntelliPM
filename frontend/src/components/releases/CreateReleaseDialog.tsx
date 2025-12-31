import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';
import { Label } from '@/components/ui/label';
import { Loader2 } from 'lucide-react';
import { releasesApi } from '@/api/releases';
import { showToast } from '@/lib/sweetalert';
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { useState } from 'react';
import { format } from 'date-fns';

interface CreateReleaseDialogProps {
  projectId: number;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess?: () => void;
}

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(200, 'Name cannot exceed 200 characters'),
  version: z
    .string()
    .min(1, 'Version is required')
    .regex(/^\d+\.\d+\.\d+$/, 'Must be semantic version (e.g., 2.1.0)'),
  description: z.string().max(2000, 'Description cannot exceed 2000 characters').optional(),
  type: z.enum(['Major', 'Minor', 'Patch', 'Hotfix']),
  plannedDate: z.string().min(1, 'Planned date is required'),
  isPreRelease: z.boolean().default(false),
  tagName: z.string().max(100, 'Tag name cannot exceed 100 characters').optional(),
  sprintIds: z.array(z.number()).optional(),
});

type FormData = z.infer<typeof schema>;

export function CreateReleaseDialog({
  projectId,
  open,
  onOpenChange,
  onSuccess,
}: CreateReleaseDialogProps) {
  const queryClient = useQueryClient();
  const [selectedSprintIds, setSelectedSprintIds] = useState<number[]>([]);

  const form = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: '',
      version: '',
      description: '',
      type: 'Minor',
      plannedDate: '',
      isPreRelease: false,
      tagName: '',
      sprintIds: [],
    },
  });

  // Fetch available sprints
  const { data: availableSprints } = useQuery({
    queryKey: ['availableSprints', projectId],
    queryFn: () => releasesApi.getAvailableSprintsForRelease(projectId),
    enabled: open,
  });

  const mutation = useMutation({
    mutationFn: (data: FormData) =>
      releasesApi.createRelease(projectId, {
        ...data,
        sprintIds: selectedSprintIds.length > 0 ? selectedSprintIds : undefined,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projectReleases', projectId] });
      queryClient.invalidateQueries({ queryKey: ['releaseStatistics', projectId] });
      showToast(`Release ${form.getValues('version')} created successfully`, 'success');
      form.reset();
      setSelectedSprintIds([]);
      onSuccess?.();
      onOpenChange(false);
    },
    onError: (error: unknown) => {
      const apiError = error as { response?: { data?: { error?: string } }; message?: string };
      const message = apiError?.response?.data?.error || apiError?.message || 'Failed to create release';
      showToast(message, 'error');
    },
  });

  const onSubmit = (data: FormData) => {
    mutation.mutate(data);
  };

  const toggleSprint = (sprintId: number) => {
    setSelectedSprintIds((prev) =>
      prev.includes(sprintId) ? prev.filter((id) => id !== sprintId) : [...prev, sprintId]
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Create New Release</DialogTitle>
          <DialogDescription>
            Create a new release for this project. Fill in the details below.
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Name</FormLabel>
                  <FormControl>
                    <Input placeholder="e.g., Summer Release 2024" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="version"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Version</FormLabel>
                  <FormControl>
                    <Input placeholder="e.g., 2.1.0" {...field} />
                  </FormControl>
                  <FormDescription>Must follow semantic versioning (e.g., 2.1.0)</FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="grid grid-cols-2 gap-4">
              <FormField
                control={form.control}
                name="type"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Type</FormLabel>
                    <Select onValueChange={field.onChange} defaultValue={field.value}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select type" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value="Major">Major - Breaking changes</SelectItem>
                        <SelectItem value="Minor">Minor - New features</SelectItem>
                        <SelectItem value="Patch">Patch - Bug fixes</SelectItem>
                        <SelectItem value="Hotfix">Hotfix - Critical fixes</SelectItem>
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="plannedDate"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Planned Date</FormLabel>
                    <FormControl>
                      <Input type="date" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <FormField
              control={form.control}
              name="description"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Description (Optional)</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Describe what's included in this release..."
                      className="min-h-[100px]"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="grid grid-cols-2 gap-4">
              <FormField
                control={form.control}
                name="tagName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Git Tag (Optional)</FormLabel>
                    <FormControl>
                      <Input placeholder="e.g., v2.1.0" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="isPreRelease"
                render={({ field }) => (
                  <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                    <FormControl>
                      <Checkbox
                        checked={field.value}
                        onCheckedChange={field.onChange}
                      />
                    </FormControl>
                    <div className="space-y-1 leading-none">
                      <FormLabel>Pre-Release</FormLabel>
                      <FormDescription>Alpha, Beta, or Release Candidate</FormDescription>
                    </div>
                  </FormItem>
                )}
              />
            </div>

            {/* Sprint Selection */}
            {availableSprints && availableSprints.length > 0 && (
              <div className="space-y-2">
                <Label>Sprints (Optional)</Label>
                <div className="border rounded-md p-4 max-h-48 overflow-y-auto">
                  {availableSprints.map((sprint) => (
                    <div key={sprint.id} className="flex items-center space-x-2 py-2">
                      <Checkbox
                        checked={selectedSprintIds.includes(sprint.id)}
                        onCheckedChange={() => toggleSprint(sprint.id)}
                      />
                      <div className="flex-1">
                        <div className="font-medium">{sprint.name || `Sprint ${sprint.number}`}</div>
                        <div className="text-sm text-muted-foreground">
                          {sprint.startDate && sprint.endDate
                            ? `${format(new Date(sprint.startDate), 'MMM dd')} - ${format(new Date(sprint.endDate), 'MMM dd')}`
                            : 'No dates'}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
                {selectedSprintIds.length > 0 && (
                  <p className="text-sm text-muted-foreground">
                    {selectedSprintIds.length} sprint(s) selected
                  </p>
                )}
              </div>
            )}

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
                disabled={mutation.isPending}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={mutation.isPending}>
                {mutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Create Release
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

