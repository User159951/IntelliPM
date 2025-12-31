import { useEffect, useState } from 'react';
import { useParams, useNavigate, Navigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useAuth } from '@/contexts/AuthContext';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { PasswordStrength } from '@/components/ui/password-strength';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { showToast, showError } from '@/lib/sweetalert';
import { authApi } from '@/api/auth';
import { Zap, Loader2, Mail } from 'lucide-react';

const acceptInviteSchema = z.object({
  username: z
    .string()
    .min(1, 'Le nom d\'utilisateur est requis')
    .min(3, 'Le nom d\'utilisateur doit contenir au moins 3 caractères')
    .max(50, 'Le nom d\'utilisateur ne doit pas dépasser 50 caractères')
    .regex(/^[a-zA-Z0-9_]+$/, 'Le nom d\'utilisateur ne peut contenir que des lettres, chiffres et underscores'),
  password: z
    .string()
    .min(1, 'Le mot de passe est requis')
    .min(8, 'Le mot de passe doit contenir au moins 8 caractères')
    .regex(/[A-Z]/, 'Le mot de passe doit contenir au moins une majuscule')
    .regex(/[a-z]/, 'Le mot de passe doit contenir au moins une minuscule')
    .regex(/[0-9]/, 'Le mot de passe doit contenir au moins un chiffre'),
  confirmPassword: z
    .string()
    .min(1, 'La confirmation du mot de passe est requise'),
}).refine((data) => data.password === data.confirmPassword, {
  message: 'Les mots de passe ne correspondent pas',
  path: ['confirmPassword'],
});

type AcceptInviteFormValues = z.infer<typeof acceptInviteSchema>;

export default function AcceptInvite() {
  const { token } = useParams<{ token: string }>();
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [isValidating, setIsValidating] = useState(true);
  const [inviteData, setInviteData] = useState<{ email: string; organizationName: string } | null>(null);
  const [error, setError] = useState<string | null>(null);

  const form = useForm<AcceptInviteFormValues>({
    resolver: zodResolver(acceptInviteSchema),
    defaultValues: {
      username: '',
      password: '',
      confirmPassword: '',
    },
  });

  const watchedPassword = form.watch('password');

  useEffect(() => {
    if (!token) {
      setError('Lien d\'invitation invalide');
      setIsValidating(false);
      return;
    }

    const validateToken = async () => {
      try {
        const data = await authApi.validateInviteToken(token);
        setInviteData(data);
      } catch (err: unknown) {
        const apiError = err as { response?: { data?: { detail?: string } }; message?: string };
        const errorMessage = apiError.response?.data?.detail || apiError.message || 'Invitation invalide ou expirée';
        setError(errorMessage);
      } finally {
        setIsValidating(false);
      }
    };

    validateToken();
  }, [token]);

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  if (isValidating) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-background p-4">
        <div className="flex flex-col items-center space-y-4">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
          <p className="text-sm text-muted-foreground">Validation de l'invitation...</p>
        </div>
      </div>
    );
  }

  if (error || !inviteData) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-background p-4">
        <Card className="w-full max-w-md">
          <CardHeader>
            <CardTitle className="text-xl text-destructive">Invitation invalide</CardTitle>
            <CardDescription>{error || 'Ce lien d\'invitation est invalide ou a expiré.'}</CardDescription>
          </CardHeader>
          <CardContent>
            <Button onClick={() => navigate('/login')} className="w-full">
              Aller à la connexion
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const onSubmit = async (values: AcceptInviteFormValues) => {
    if (!token) return;

    setIsLoading(true);

    try {
      await authApi.acceptOrganizationInvite({
        token,
        username: values.username,
        password: values.password,
        confirmPassword: values.confirmPassword,
      });

      showToast('Votre compte a été créé. Redirection...', 'success');

      // Redirect to dashboard after a short delay to show success message
      setTimeout(() => {
        navigate('/dashboard');
      }, 1000);
    } catch (err: unknown) {
      const apiError = err as { response?: { data?: { detail?: string; error?: string } }; message?: string };
      const errorMessage = apiError.response?.data?.detail || apiError.response?.data?.error || apiError.message || 'Échec de l\'acceptation de l\'invitation';
      showError('Échec de l\'acceptation de l\'invitation', errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-background p-4">
      <div className="w-full max-w-md space-y-8">
        <div className="flex flex-col items-center space-y-2">
          <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary">
            <Zap className="h-7 w-7 text-primary-foreground" />
          </div>
          <h1 className="text-2xl font-bold text-foreground">IntelliPM</h1>
          <p className="text-sm text-muted-foreground">Accepter l'invitation</p>
        </div>

        <Card className="w-full max-w-md border-border">
          <CardHeader>
            <CardTitle>✨ Créer votre compte</CardTitle>
            <CardDescription>
              Vous avez été invité à rejoindre <strong>{inviteData.organizationName}</strong>
            </CardDescription>
            <div className="flex items-center gap-2 pt-2">
              <Mail className="h-4 w-4 text-muted-foreground" />
              <span className="text-sm text-muted-foreground">{inviteData.email}</span>
            </div>
          </CardHeader>
          <CardContent>
            <Form {...form}>
              <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                {/* Email (read-only, from invitation) */}
                <div className="space-y-2">
                  <Label htmlFor="email">Email</Label>
                  <Input
                    id="email"
                    name="email"
                    type="email"
                    autoComplete="email"
                    disabled
                    value={inviteData.email}
                    className="bg-muted"
                  />
                </div>

                {/* Username (user chooses) */}
                <FormField
                  control={form.control}
                  name="username"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Nom d'utilisateur</FormLabel>
                      <FormControl>
                        <Input
                          type="text"
                          autoComplete="username"
                          placeholder="Nom d'utilisateur"
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Password */}
                <FormField
                  control={form.control}
                  name="password"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Mot de passe</FormLabel>
                      <FormControl>
                        <Input
                          type="password"
                          autoComplete="new-password"
                          placeholder="Mot de passe"
                          {...field}
                        />
                      </FormControl>
                      {watchedPassword && (
                        <PasswordStrength password={watchedPassword} />
                      )}
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Confirm Password */}
                <FormField
                  control={form.control}
                  name="confirmPassword"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Confirmer le mot de passe</FormLabel>
                      <FormControl>
                        <Input
                          type="password"
                          autoComplete="new-password"
                          placeholder="Confirmer le mot de passe"
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <Button type="submit" className="w-full" disabled={isLoading}>
                  {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                  {isLoading ? 'Création...' : 'Créer mon compte'}
                </Button>
              </form>
            </Form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
