import { Link } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';

export default function Register() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-purple-50 to-blue-50">
      <Card className="w-full max-w-md p-8">
        <CardHeader>
          <CardTitle>🔒 Inscription fermée</CardTitle>
          </CardHeader>
        <CardContent>
          <p className="text-muted-foreground mb-4">
            Les inscriptions publiques sont désactivées pour des raisons de sécurité.
          </p>
          <p className="text-muted-foreground">
            Pour accéder à IntelliPM, contactez votre administrateur pour recevoir une invitation par email.
          </p>
          <Button asChild className="w-full mt-6">
            <Link to="/login">← Retour à la connexion</Link>
          </Button>
            </CardContent>
        </Card>
    </div>
  );
}
