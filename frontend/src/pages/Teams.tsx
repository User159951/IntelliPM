import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { teamsApi } from '@/api/teams';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Progress } from '@/components/ui/progress';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Skeleton } from '@/components/ui/skeleton';
import { showSuccess, showError } from "@/lib/sweetalert";
import { useAuth } from '@/contexts/AuthContext';
import { Plus, Loader2, Users, Settings } from 'lucide-react';
import type { RegisterTeamRequest } from '@/types';

export default function Teams() {
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [formData, setFormData] = useState<RegisterTeamRequest>({
    name: '',
    memberIds: [],
    totalCapacity: 100,
  });

  const { data: teamsData, isLoading } = useQuery({
    queryKey: ['teams'],
    queryFn: () => teamsApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: RegisterTeamRequest) => teamsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['teams'] });
      setIsDialogOpen(false);
      setFormData({ name: '', memberIds: [], totalCapacity: 100 });
      showSuccess("Team created");
    },
    onError: () => {
      showError('Failed to create team');
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!user?.userId) {
      showError("Error", "You must be logged in to create a team");
      return;
    }
    
    // Automatically add the current user as a member if not already included
    const memberIds = formData.memberIds.length > 0 
      ? formData.memberIds.includes(user.userId)
        ? formData.memberIds
        : [...formData.memberIds, user.userId]
      : [user.userId];
    
    createMutation.mutate({
      ...formData,
      memberIds,
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Teams</h1>
          <p className="text-muted-foreground">Manage your teams and capacity</p>
        </div>
        <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
          <DialogTrigger asChild>
            <Button>
              <Plus className="mr-2 h-4 w-4" />
              Create Team
            </Button>
          </DialogTrigger>
          <DialogContent className="sm:max-w-[500px]">
            <form onSubmit={handleSubmit}>
              <DialogHeader>
                <DialogTitle>Create new team</DialogTitle>
                <DialogDescription>Set up a new team to manage capacity.</DialogDescription>
              </DialogHeader>
              <div className="grid gap-4 py-4">
                <div className="space-y-2">
                  <Label htmlFor="name">Team name</Label>
                  <Input
                    id="name"
                    placeholder="Engineering Team"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="capacity">Total capacity (hours/sprint)</Label>
                  <Input
                    id="capacity"
                    type="number"
                    min={1}
                    value={formData.totalCapacity}
                    onChange={(e) =>
                      setFormData({ ...formData, totalCapacity: parseInt(e.target.value) || 100 })
                    }
                  />
                </div>
              </div>
              <DialogFooter>
                <Button type="button" variant="outline" onClick={() => setIsDialogOpen(false)}>
                  Cancel
                </Button>
                <Button type="submit" disabled={createMutation.isPending}>
                  {createMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                  Create team
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {isLoading ? (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {[1, 2, 3].map((i) => (
            <Card key={i}>
              <CardHeader>
                <Skeleton className="h-5 w-32" />
                <Skeleton className="h-4 w-24" />
              </CardHeader>
              <CardContent>
                <Skeleton className="h-24 w-full" />
              </CardContent>
            </Card>
          ))}
        </div>
      ) : teamsData?.teams?.length === 0 ? (
        <Card className="flex flex-col items-center justify-center py-16">
          <Users className="h-12 w-12 text-muted-foreground mb-4" />
          <h3 className="text-lg font-medium">No teams yet</h3>
          <p className="text-muted-foreground mb-4">Create your first team to manage capacity</p>
          <Button onClick={() => setIsDialogOpen(true)}>
            <Plus className="mr-2 h-4 w-4" />
            Create Team
          </Button>
        </Card>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {teamsData?.teams?.map((team) => (
            <Card key={team.id}>
              <CardHeader className="flex flex-row items-start justify-between">
                <div>
                  <CardTitle className="text-lg">{team.name}</CardTitle>
                  <CardDescription>{team.members?.length || 0} members</CardDescription>
                </div>
                <Button variant="ghost" size="icon">
                  <Settings className="h-4 w-4" />
                </Button>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-2">
                  <div className="flex justify-between text-sm">
                    <span>Capacity utilization</span>
                    <span className="font-medium">75%</span>
                  </div>
                  <Progress value={75} className="h-2" />
                </div>
                <div className="flex items-center justify-between text-sm text-muted-foreground">
                  <span>Total: {team.totalCapacity}h</span>
                  <span>Available: {Math.round(team.totalCapacity * 0.25)}h</span>
                </div>
                <div className="pt-2">
                  <p className="text-xs text-muted-foreground mb-2">Team members</p>
                  <div className="flex -space-x-2">
                    {team.members?.slice(0, 5).map((member) => (
                      <Avatar key={member.id} className="h-8 w-8 border-2 border-card">
                        <AvatarFallback className="text-xs bg-primary text-primary-foreground">
                          {member.name.split(' ').map((n) => n[0]).join('').toUpperCase()}
                        </AvatarFallback>
                      </Avatar>
                    ))}
                    {(team.members?.length || 0) > 5 && (
                      <div className="flex h-8 w-8 items-center justify-center rounded-full border-2 border-card bg-muted text-xs">
                        +{team.members!.length - 5}
                      </div>
                    )}
                    {(!team.members || team.members.length === 0) && (
                      <span className="text-sm text-muted-foreground">No members yet</span>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
