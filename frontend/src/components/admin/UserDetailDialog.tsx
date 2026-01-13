import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { usersApi, type UserListDto } from '@/api/users';
import type { ProjectListDto } from '@/api/projects';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Skeleton } from '@/components/ui/skeleton';
import { Button } from '@/components/ui/button';
import { Mail, Calendar, Briefcase, History, Clock } from 'lucide-react';
import { useLanguage } from '@/contexts/LanguageContext';
import { useTranslation } from 'react-i18next';
import { formatDate, DateFormats } from '@/utils/dateFormat';
import { Link } from 'react-router-dom';

interface UserDetailDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user: UserListDto;
}

export function UserDetailDialog({ open, onOpenChange, user }: UserDetailDialogProps) {
  const { t } = useTranslation('admin');
  const { language } = useLanguage();
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
          <DialogTitle>{t('dialogs.userDetail.title')}</DialogTitle>
          <DialogDescription>{t('dialogs.userDetail.description')}</DialogDescription>
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
              <TabsTrigger value="details">{t('dialogs.userDetail.tabs.details')}</TabsTrigger>
              <TabsTrigger value="projects">{t('dialogs.userDetail.tabs.projects', { count: user.projectCount })}</TabsTrigger>
              <TabsTrigger value="history">{t('dialogs.userDetail.tabs.history')}</TabsTrigger>
            </TabsList>

            {/* Details Tab */}
            <TabsContent value="details" className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1">
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Mail className="h-4 w-4" />
                    {t('dialogs.userDetail.fields.email')}
                  </div>
                  <p className="font-medium">{user.email}</p>
                </div>
                <div className="space-y-1">
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Calendar className="h-4 w-4" />
                    {t('dialogs.userDetail.fields.memberSince')}
                  </div>
                  <p className="font-medium">
                    {formatDate(user.createdAt, DateFormats.LONG(language), language)}
                  </p>
                </div>
                <div className="space-y-1">
                  <div className="text-sm text-muted-foreground">{t('dialogs.userDetail.fields.organization')}</div>
                  <p className="font-medium">{user.organizationName}</p>
                </div>
                <div className="space-y-1">
                  <div className="text-sm text-muted-foreground">{t('dialogs.userDetail.fields.projects')}</div>
                  <p className="font-medium">{t('dialogs.userDetail.fields.projectsCount', { count: user.projectCount })}</p>
                </div>
                {user.lastLoginAt && (
                  <div className="space-y-1">
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                      <Clock className="h-4 w-4" />
                      {t('dialogs.userDetail.fields.lastLogin')}
                    </div>
                    <p className="font-medium">
                      {formatDate(user.lastLoginAt, DateFormats.DATETIME(language), language)}
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
                  <p>{t('dialogs.userDetail.projects.none')}</p>
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
                        <TableHead>{t('dialogs.userDetail.projects.headers.name')}</TableHead>
                        <TableHead>{t('dialogs.userDetail.projects.headers.type')}</TableHead>
                        <TableHead>{t('dialogs.userDetail.projects.headers.status')}</TableHead>
                        <TableHead>{t('dialogs.userDetail.projects.headers.created')}</TableHead>
                        <TableHead>{t('dialogs.userDetail.projects.headers.actions')}</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {projectsData.items.map((project: ProjectListDto) => (
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
                            {formatDate(project.createdAt, DateFormats.LONG(language), language)}
                          </TableCell>
                          <TableCell>
                            <Button variant="ghost" size="sm" asChild>
                              <Link to={`/projects/${project.id}`}>{t('dialogs.userDetail.projects.view')}</Link>
                            </Button>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                  {projectsData && projectsData.totalPages && projectsData.totalPages > 1 && (
                    <div className="flex items-center justify-between">
                      <p className="text-sm text-muted-foreground">
                        {t('dialogs.userDetail.projects.showing', {
                          start: ((projectsPage - 1) * pageSize) + 1,
                          end: Math.min(projectsPage * pageSize, projectsData.totalCount ?? 0),
                          total: projectsData.totalCount ?? 0
                        })}
                      </p>
                      <div className="flex gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setProjectsPage(p => Math.max(1, p - 1))}
                          disabled={projectsPage === 1}
                        >
                          {t('dialogs.userDetail.projects.previous')}
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setProjectsPage(p => p + 1)}
                          disabled={projectsPage >= (projectsData.totalPages ?? 1)}
                        >
                          {t('dialogs.userDetail.projects.next')}
                        </Button>
                      </div>
                    </div>
                  )}
                </div>
              ) : (
                <div className="text-center py-8 text-muted-foreground">
                  <p>{t('dialogs.userDetail.projects.noProjects')}</p>
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
              ) : activityData && activityData.length > 0 ? (
                <div className="space-y-4">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>{t('dialogs.userDetail.activity.headers.timestamp')}</TableHead>
                        <TableHead>{t('dialogs.userDetail.activity.headers.action')}</TableHead>
                        <TableHead>{t('dialogs.userDetail.activity.headers.entity')}</TableHead>
                        <TableHead>{t('dialogs.userDetail.activity.headers.project')}</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {activityData.map((activity: { type: string; description: string; timestamp: string; id?: number; entityType?: string; entityName?: string; projectName?: string; projectId?: number }, index: number) => (
                        <TableRow key={activity.id ?? index}>
                          <TableCell className="text-muted-foreground">
                            {formatDate(activity.timestamp, DateFormats.DATETIME(language), language)}
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
                  <p>{t('dialogs.userDetail.activity.noActivity')}</p>
                </div>
              )}
            </TabsContent>
          </Tabs>
        </div>
      </DialogContent>
    </Dialog>
  );
}

