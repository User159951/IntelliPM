import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { AlertCircle } from 'lucide-react';

export default function Register() {
  const { t } = useTranslation('auth');
  
  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-purple-50 to-blue-50">
      <Card className="w-full max-w-md p-8">
        <CardHeader>
          <CardTitle>{t('register.title')}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertTitle>{t('register.alertTitle')}</AlertTitle>
            <AlertDescription>
              {t('register.alertDescription')}
            </AlertDescription>
          </Alert>
          
          <p className="text-muted-foreground">
            {t('register.message1')}
          </p>
          <p className="text-muted-foreground">
            {t('register.message2')}
          </p>
          <Button asChild className="w-full mt-6">
            <Link to="/login">{t('register.backToLogin')}</Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
