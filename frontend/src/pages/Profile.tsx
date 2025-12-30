import { useAuth } from '@/contexts/AuthContext';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';

export default function Profile() {
  const { user } = useAuth();

  if (!user) {
    return null;
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-foreground">Profile</h1>
      <Card className="max-w-xl">
        <CardHeader>
          <CardTitle>Account details</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3 text-sm">
          <div>
            <p className="text-muted-foreground">Name</p>
            <p className="font-medium">
              {user.firstName} {user.lastName}
            </p>
          </div>
          <div>
            <p className="text-muted-foreground">Username</p>
            <p className="font-medium">{user.username}</p>
          </div>
          <div>
            <p className="text-muted-foreground">Email</p>
            <p className="font-medium">{user.email}</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

