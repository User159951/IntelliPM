import { useEffect, useState } from 'react';
import { useParams, useNavigate, Navigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
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

export default function AcceptInvite() {
  const { t } = useTranslation('auth');
  const { token } = useParams<{ token: string }>();
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [isValidating, setIsValidating] = useState(true);
  const [inviteData, setInviteData] = useState<{ email: string; organizationName: string } | null>(null);
  const [error, setError] = useState<string | null>(null);

  const acceptInviteSchema = z.object({
    username: z
      .string()
      .min(1, t('acceptInvite.validation.usernameRequired'))
      .min(3, t('acceptInvite.validation.usernameMinLength'))
      .max(50, t('acceptInvite.validation.usernameMaxLength'))
      .regex(/^[a-zA-Z0-9_]+$/, t('acceptInvite.validation.usernameFormat')),
    password: z
      .string()
      .min(1, t('acceptInvite.validation.passwordRequired'))
      .min(8, t('acceptInvite.validation.passwordMinLength'))
      .regex(/[A-Z]/, t('acceptInvite.validation.passwordUppercase'))
      .regex(/[a-z]/, t('acceptInvite.validation.passwordLowercase'))
      .regex(/[0-9]/, t('acceptInvite.validation.passwordNumber')),
    confirmPassword: z
      .string()
      .min(1, t('acceptInvite.validation.confirmRequired')),
  }).refine((data) => data.password === data.confirmPassword, {
    message: t('acceptInvite.validation.mismatch'),
    path: ['confirmPassword'],
  });

  type AcceptInviteFormValues = z.infer<typeof acceptInviteSchema>;

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
      setError(t('acceptInvite.errors.invalidToken'));
      setIsValidating(false);
      return;
    }

    const validateToken = async () => {
      try {
        const data = await authApi.validateInviteToken(token);
        setInviteData(data);
      } catch (err: unknown) {
        const apiError = err as { response?: { data?: { detail?: string } }; message?: string };
        const errorMessage = apiError.response?.data?.detail || apiError.message || t('acceptInvite.errors.invalidOrExpired');
        setError(errorMessage);
      } finally {
        setIsValidating(false);
      }
    };

    validateToken();
  }, [token, t]);

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  if (isValidating) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-background p-4">
        <div className="flex flex-col items-center space-y-4">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
          <p className="text-sm text-muted-foreground">{t('acceptInvite.validating')}</p>
        </div>
      </div>
    );
  }

  if (error || !inviteData) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-background p-4">
        <Card className="w-full max-w-md">
          <CardHeader>
            <CardTitle className="text-xl text-destructive">{t('acceptInvite.invalidTitle')}</CardTitle>
            <CardDescription>{error || t('acceptInvite.invalidMessage')}</CardDescription>
          </CardHeader>
          <CardContent>
            <Button onClick={() => navigate('/login')} className="w-full">
              {t('acceptInvite.goToLogin')}
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

      showToast(t('acceptInvite.success.toast'), 'success');

      // Redirect to dashboard after a short delay to show success message
      setTimeout(() => {
        navigate('/dashboard');
      }, 1000);
    } catch (err: unknown) {
      const apiError = err as { response?: { data?: { detail?: string; error?: string } }; message?: string };
      const errorMessage = apiError.response?.data?.detail || apiError.response?.data?.error || apiError.message || t('acceptInvite.success.errorTitle');
      showError(t('acceptInvite.success.errorTitle'), errorMessage);
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
          <h1 className="text-2xl font-bold text-foreground">{t('acceptInvite.title')}</h1>
          <p className="text-sm text-muted-foreground">{t('acceptInvite.subtitle')}</p>
        </div>

        <Card className="w-full max-w-md border-border">
          <CardHeader>
            <CardTitle>{t('acceptInvite.cardTitle')}</CardTitle>
            <CardDescription>
              {t('acceptInvite.cardDescription', { organizationName: inviteData.organizationName })}
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
                  <Label htmlFor="email">{t('acceptInvite.emailLabel')}</Label>
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
                      <FormLabel>{t('acceptInvite.usernameLabel')}</FormLabel>
                      <FormControl>
                        <Input
                          type="text"
                          autoComplete="username"
                          placeholder={t('acceptInvite.usernamePlaceholder')}
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
                      <FormLabel>{t('acceptInvite.passwordLabel')}</FormLabel>
                      <FormControl>
                        <Input
                          type="password"
                          autoComplete="new-password"
                          placeholder={t('acceptInvite.passwordPlaceholder')}
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
                      <FormLabel>{t('acceptInvite.confirmPasswordLabel')}</FormLabel>
                      <FormControl>
                        <Input
                          type="password"
                          autoComplete="new-password"
                          placeholder={t('acceptInvite.confirmPasswordPlaceholder')}
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <Button type="submit" className="w-full" disabled={isLoading}>
                  {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                  {isLoading ? t('acceptInvite.submitButtonLoading') : t('acceptInvite.submitButton')}
                </Button>
              </form>
            </Form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
