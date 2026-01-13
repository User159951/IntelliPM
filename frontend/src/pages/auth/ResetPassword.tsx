import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { PasswordStrengthIndicator } from '@/components/auth/PasswordStrengthIndicator';
import { showToast, showError } from "@/lib/sweetalert";
import { authApi } from '@/api/auth';
import { Loader2, CheckCircle2, ArrowLeft } from 'lucide-react';

export default function ResetPassword() {
  const { t } = useTranslation('auth');
  const { token } = useParams<{ token: string }>();
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [success, setSuccess] = useState(false);

  const resetPasswordSchema = z.object({
    newPassword: z
      .string()
      .min(1, t('resetPassword.validation.required'))
      .min(8, t('resetPassword.validation.minLength'))
      .regex(/[A-Z]/, t('resetPassword.validation.uppercase'))
      .regex(/[a-z]/, t('resetPassword.validation.lowercase'))
      .regex(/[0-9]/, t('resetPassword.validation.number'))
      .regex(/[^a-zA-Z0-9]/, t('resetPassword.validation.special')),
    confirmPassword: z
      .string()
      .min(1, t('resetPassword.validation.confirmRequired')),
  }).refine((data) => data.newPassword === data.confirmPassword, {
    message: t('resetPassword.validation.mismatch'),
    path: ['confirmPassword'],
  });

  type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>;

  const form = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: {
      newPassword: '',
      confirmPassword: '',
    },
  });

  const watchedPassword = form.watch('newPassword');

  useEffect(() => {
    if (!token) {
      showError(t('resetPassword.errors.invalidLink'), t('resetPassword.errors.invalidLinkMessage'));
      navigate('/forgot-password');
    }
  }, [token, navigate, t]);

  const onSubmit = async (values: ResetPasswordFormValues) => {
    if (!token) return;

    setIsLoading(true);
    try {
      const result = await authApi.resetPassword({
        token,
        newPassword: values.newPassword,
        confirmPassword: values.confirmPassword,
      });

      if (result.success) {
        setSuccess(true);
        showToast(t('resetPassword.success.toast'), "success");

        // Redirect to login after 3 seconds
        setTimeout(() => {
          navigate('/login');
        }, 3000);
      } else {
        showError(t('resetPassword.errors.error'));
      }
    } catch {
      showError(t('resetPassword.errors.error'));
    } finally {
      setIsLoading(false);
    }
  };

  if (success) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-purple-50 to-blue-50">
        <Card className="w-full max-w-md p-8">
          <CardHeader>
            <div className="flex items-center justify-center mb-4">
              <CheckCircle2 className="h-16 w-16 text-green-600" />
            </div>
            <CardTitle className="text-2xl text-center">{t('resetPassword.success.title')}</CardTitle>
            <CardDescription className="text-center">
              {t('resetPassword.success.description')}
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className="text-center text-sm text-muted-foreground">
              {t('resetPassword.success.redirectMessage')}
            </p>
          </CardContent>
          <CardFooter>
            <Button asChild className="w-full">
              <Link to="/login">{t('resetPassword.success.goToLogin')}</Link>
            </Button>
          </CardFooter>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-purple-50 to-blue-50">
      <Card className="w-full max-w-md p-8">
        <CardHeader>
          <CardTitle className="text-2xl">{t('resetPassword.title')}</CardTitle>
          <CardDescription>
            {t('resetPassword.description')}
          </CardDescription>
        </CardHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <CardContent className="space-y-4">
              <FormField
                control={form.control}
                name="newPassword"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('resetPassword.newPasswordLabel')}</FormLabel>
                    <FormControl>
                      <Input
                        type="password"
                        placeholder={t('resetPassword.newPasswordPlaceholder')}
                        {...field}
                        disabled={isLoading}
                        autoComplete="new-password"
                      />
                    </FormControl>
                    <PasswordStrengthIndicator password={watchedPassword} />
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="confirmPassword"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('resetPassword.confirmPasswordLabel')}</FormLabel>
                    <FormControl>
                      <Input
                        type="password"
                        placeholder={t('resetPassword.confirmPasswordPlaceholder')}
                        {...field}
                        disabled={isLoading}
                        autoComplete="new-password"
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </CardContent>
            <CardFooter className="flex flex-col space-y-4">
              <Button type="submit" className="w-full" disabled={isLoading}>
                {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                {t('resetPassword.submitButton')}
              </Button>
              <Button asChild variant="ghost" className="w-full">
                <Link to="/login">
                  <ArrowLeft className="mr-2 h-4 w-4" />
                  {t('resetPassword.backToLogin')}
                </Link>
              </Button>
            </CardFooter>
          </form>
        </Form>
      </Card>
    </div>
  );
}

