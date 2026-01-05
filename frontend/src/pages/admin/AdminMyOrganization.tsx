import { useQuery } from '@tanstack/react-query';
import { format } from 'date-fns';
import { organizationsApi } from '@/api/organizations';
import { Skeleton } from '@/components/ui/skeleton';
import { Label } from '@/components/ui/label';
import { Building2, Users, Calendar } from 'lucide-react';
import { Badge } from '@/components/ui/badge';

export default function AdminMyOrganization() {
  const { data: organization, isLoading } = useQuery({
    queryKey: ['admin-my-organization'],
    queryFn: () => organizationsApi.getMyOrganization(),
  });

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-10 w-64" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  if (!organization) {
    return (
      <div className="text-center py-12">
        <p className="text-muted-foreground">Organization not found</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">My Organization</h1>
        <p className="text-muted-foreground mt-1">View your organization details</p>
      </div>

      <div className="border rounded-lg p-6 space-y-6">
        <div className="flex items-center gap-3">
          <Building2 className="h-8 w-8 text-muted-foreground" />
          <div>
            <h2 className="text-xl font-semibold">{organization.name}</h2>
            <p className="text-sm text-muted-foreground">Organization Information</p>
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 pt-4">
          <div className="space-y-2">
            <Label className="text-muted-foreground flex items-center gap-2">
              <Building2 className="h-4 w-4" />
              Name
            </Label>
            <p className="text-lg font-medium">{organization.name}</p>
          </div>

          <div className="space-y-2">
            <Label className="text-muted-foreground">Code</Label>
            <div>
              <Badge variant="outline" className="text-lg px-3 py-1">
                {organization.code}
              </Badge>
            </div>
          </div>

          <div className="space-y-2">
            <Label className="text-muted-foreground flex items-center gap-2">
              <Users className="h-4 w-4" />
              Members
            </Label>
            <p className="text-lg font-medium">{organization.userCount}</p>
          </div>

          <div className="space-y-2">
            <Label className="text-muted-foreground flex items-center gap-2">
              <Calendar className="h-4 w-4" />
              Created
            </Label>
            <p className="text-lg font-medium">
              {format(new Date(organization.createdAt), 'MMM dd, yyyy')}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}

