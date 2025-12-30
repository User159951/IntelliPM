import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { usersApi, type UserListDto } from '@/api/users';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Skeleton } from '@/components/ui/skeleton';
import { Button } from '@/components/ui/button';
import { Loader2, Mail, Calendar, Briefcase, History, Clock } from 'lucide-react';
import { format } from 'date-fns';
import { Link } from 'react-router-dom';

interface UserDetailDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user: UserListDto;
}

export function UserDetailDialog({ open, onOpenChange, user }: UserDetailDialogProps) {
  const [projectsPage, setProjectsPage] = useState(1);
  const pageSize = 10;

  const { data: projectsData, isLoading: isLoadingProjects } = useQuery({
    queryKey: ['user-projects', user.id, projectsPage],
    queryFn: () => usersApi.getUserProjects(user.id, projectsPage, pageSize),
    enabled: open && user.projectCount > 0,
  });

  const { data: activityData, isLoading: isLoadingActivity } = useQuery({
    queryKey: ['user-activity', user.id],
    queryFn: () => usersApi.getUserActivity(user.id, 50),
    enabled: open,
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>User Details</DialogTitle>
          <DialogDescription>View detailed information about this user</DialogDescription>
        </DialogHeader>

        <div className="space-y-6">
          {/* User Header */}
          <div className="flex items-center gap-4 pb-4 border-b">
            <Avatar className="h-16 w-16">
              <AvatarFallback className="bg-primary text-primary-foreground text-lg">
                {`${user.firstName[0]}${user.lastName[0]}`.toUpperCase()}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <h3 className="text-2xl font-bold">
                {user.firstName} {user.lastName}
              </h3>
              <p className="text-muted-foreground">@{user.username}</p>
              <div className="flex items-center gap-4 mt-2">
                <Badge variant={user.globalRole === 'Admin' ? 'default' : 'secondary'}>
                  {user.globalRole}
                </Badge>
                <Badge variant={user.isActive ? 'default' : 'destructive'}>
                  {user.isActive ? 'Active' : 'Inactive'}
                </Badge>
              </div>
            </div>
          </div>

          <Tabs defaultValue="details" className="w-full">
            <TabsList className="grid w-full grid-cols-3">
              <TabsTrigger value="details">Details</TabsTrigger>
              <TabsTrigger value="projects">Projects ({user.projectCount})</TabsTrigger>
              <TabsTrigger value="history">Activity History</TabsTrigger>
            </TabsList>

            {/* Details Tab */}
            <TabsContent value="details" className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1">
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Mail className="h-4 w-4" />
                    Email
                  </div>
                  <p className="font-medium">{user.email}</p>
                </div>
                <div className="space-y-1">
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Calendar className="h-4 w-4" />
                    Member Since
                  </div>
                  <p className="font-medium">
                    {format(new Date(user.createdAt), 'MMM d, yyyy')}
                  </p>
                </div>
                <div className="space-y-1">
                  <div className="text-sm text-muted-foreground">Organization</div>
                  <p className="font-medium">{user.organizationName}</p>
                </div>
                <div className="space-y-1">
                  <div className="text-sm text-muted-foreground">Projects</div>
                  <p className="font-medium">{user.projectCount} project(s)</p>
                </div>
                {user.lastLoginAt && (
                  <div className="space-y-1">
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                      <Clock className="h-4 w-4" />
                      Last Login
                    </div>
                    <p className="font-medium">
                      {format(new Date(user.lastLoginAt), 'MMM d, yyyy HH:mm')}
                    </p>
                  </div>
                )}
              </div>
            </TabsContent>

            {/* Projects Tab */}
            <TabsContent value="projects" className="space-y-4">
              {user.projectCount === 0 ? (
                <div className="text-center py-8 text-muted-foreground">
                  <Briefcase className="h-12 w-12 mx-auto mb-4 opacity-50" />
                  <p>This user is not a member of any projects.</p>
                </div>
              ) : isLoadingProjects ? (
                <div className="space-y-4">
                  {[...Array(3)].map((_, i) => (
                    <Skeleton key={i} className="h-16 w-full" />
                  ))}
                </div>
              ) : projectsData?.items && projectsData.items.length > 0 ? (
                <div className="space-y-4">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Project Name</TableHead>
                        <TableHead>Type</TableHead>
                        <TableHead>Status</TableHead>
                        <TableHead>Created</TableHead>
                        <TableHead>Actions</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {projectsData.items.map((project: any) => (
                        <TableRow key={project.id}>
                          <TableCell className="font-medium">{project.name}</TableCell>
                          <TableCell>
                            <Badge variant="outline">{project.type}</Badge>
                          </TableCell>
                          <TableCell>
                            <Badge variant={project.status === 'Active' ? 'default' : 'secondary'}>
                              {project.status}
                            </Badge>
                          </TableCell>
                          <TableCell className="text-muted-foreground">
                            {format(new Date(project.createdAt), 'MMM d, yyyy')}
                          </TableCell>
                          <TableCell>
                            <Button variant="ghost" size="sm" asChild>
                              <Link to={`/projects/${project.id}`}>View</Link>
                            </Button>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                  {projectsData.totalPages > 1 && (
                    <div className="flex items-center justify-between">
                      <p className="text-sm text-muted-foreground">
                        Showing {((projectsPage - 1) * pageSize) + 1} to {Math.min(projectsPage * pageSize, projectsData.totalCount)} of {projectsData.totalCount} projects
                      </p>
                      <div className="flex gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setProjectsPage(p => Math.max(1, p - 1))}
                          disabled={projectsPage === 1}
                        >
                          Previous
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setProjectsPage(p => p + 1)}
                          disabled={projectsPage >= projectsData.totalPages}
                        >
                          Next
                        </Button>
                      </div>
                    </div>
                  )}
                </div>
              ) : (
                <div className="text-center py-8 text-muted-foreground">
                  <p>No projects found.</p>
                </div>
              )}
            </TabsContent>

            {/* Activity History Tab */}
            <TabsContent value="history" className="space-y-4">
              {isLoadingActivity ? (
                <div className="space-y-4">
                  {[...Array(5)].map((_, i) => (
                    <Skeleton key={i} className="h-16 w-full" />
                  ))}
                </div>
              ) : activityData?.activities && activityData.activities.length > 0 ? (
                <div className="space-y-4">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Timestamp</TableHead>
                        <TableHead>Action</TableHead>
                        <TableHead>Entity</TableHead>
                        <TableHead>Project</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {activityData.activities.map((activity: any) => (
                        <TableRow key={activity.id}>
                          <TableCell className="text-muted-foreground">
                            {format(new Date(activity.timestamp), 'MMM d, yyyy HH:mm')}
                          </TableCell>
                          <TableCell>
                            <Badge variant="outline">{activity.type}</Badge>
                          </TableCell>
                          <TableCell>
                            {activity.entityName ? (
                              <span>{activity.entityType}: {activity.entityName}</span>
                            ) : (
                              <span className="text-muted-foreground">{activity.entityType}</span>
                            )}
                          </TableCell>
                          <TableCell>
                            {activity.projectName ? (
                              <Link
                                to={`/projects/${activity.projectId}`}
                                className="text-primary hover:underline"
                              >
                                {activity.projectName}
                              </Link>
                            ) : (
                              <span className="text-muted-foreground">-</span>
                            )}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              ) : (
                <div className="text-center py-8 text-muted-foreground">
                  <History className="h-12 w-12 mx-auto mb-4 opacity-50" />
                  <p>No activity history found.</p>
                </div>
              )}
            </TabsContent>
          </Tabs>
        </div>
      </DialogContent>
    </Dialog>
  );
}

