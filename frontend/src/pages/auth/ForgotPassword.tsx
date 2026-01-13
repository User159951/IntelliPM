import { useState } from 'react';
import { Link } from 'react-router-dom';
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
import { showToast, showError } from '@/lib/sweetalert';
import { authApi } from '@/api/auth';
import { Loader2, Mail, ArrowLeft } from 'lucide-react';

export default function ForgotPassword() {
  const { t } = useTranslation('auth');
  const [isLoading, setIsLoading] = useState(false);
  const [emailSent, setEmailSent] = useState(false);
  const [isEmailMode, setIsEmailMode] = useState(true);

  const forgotPasswordSchema = z.object({
    emailOrUsername: z
      .string()
      .min(1, t('forgotPassword.validation.required'))
      .max(255, t('forgotPassword.validation.maxLength')),
  });

  type ForgotPasswordFormValues = z.infer<typeof forgotPasswordSchema>;

  const form = useForm<ForgotPasswordFormValues>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: {
      emailOrUsername: '',
    },
  });

  const onSubmit = async (values: ForgotPasswordFormValues) => {
    setIsLoading(true);
    try {
      const result = await authApi.requestPasswordReset({
        emailOrUsername: values.emailOrUsername,
      });
      
      setEmailSent(true);
      showToast(result.message || t('forgotPassword.success.emailSent'), 'success');
    } catch (error) {
      showError(
        t('forgotPassword.errors.error'),
        error instanceof Error ? error.message : t('forgotPassword.errors.generic')
      );
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-background p-4">
      <div className="w-full max-w-md space-y-8">
        <Card className="border-border">
          <CardHeader className="space-y-1">
            <CardTitle className="text-xl">{t('forgotPassword.title')}</CardTitle>
            <CardDescription>
            {emailSent
              ? t('forgotPassword.descriptionEmailSent')
              : t('forgotPassword.description', { mode: isEmailMode ? t('forgotPassword.emailLabel').toLowerCase() : t('forgotPassword.usernameLabel').toLowerCase() })}
          </CardDescription>
        </CardHeader>
        {emailSent ? (
          <CardContent className="space-y-4">
            <div className="flex items-center justify-center p-6 bg-green-50 rounded-lg border border-green-200">
              <Mail className="h-12 w-12 text-green-600" />
            </div>
            <p className="text-center text-sm text-muted-foreground">
              {t('forgotPassword.emailSent.message')}
            </p>
            <p className="text-center text-xs text-muted-foreground">
              {t('forgotPassword.emailSent.expiry')}
            </p>
          </CardContent>
        ) : (
          <>
            <Form {...form}>
              <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                <CardContent>
                  <FormField
                    control={form.control}
                    name="emailOrUsername"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>{isEmailMode ? t('forgotPassword.emailLabel') : t('forgotPassword.usernameLabel')}</FormLabel>
                        <FormControl>
                          <Input
                            placeholder={isEmailMode ? t('forgotPassword.emailPlaceholder') : t('forgotPassword.usernamePlaceholder')}
                            {...field}
                            disabled={isLoading}
                            autoComplete={isEmailMode ? 'email' : 'username'}
                            type={isEmailMode ? 'email' : 'text'}
                          />
                        </FormControl>
                        <div className="text-sm">
                          <button
                            type="button"
                            onClick={() => {
                              setIsEmailMode(!isEmailMode);
                              form.setValue('emailOrUsername', '');
                            }}
                            className="text-primary hover:underline"
                            disabled={isLoading}
                          >
                            {isEmailMode ? t('forgotPassword.switchToUsername') : t('forgotPassword.switchToEmail')}
                          </button>
                        </div>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </CardContent>
                <CardFooter className="flex flex-col space-y-4">
                  <Button type="submit" className="w-full" disabled={isLoading}>
                    {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                    {t('forgotPassword.submitButton')}
                  </Button>
                  <Button asChild variant="ghost" className="w-full">
                    <Link to="/login">
                      <ArrowLeft className="mr-2 h-4 w-4" />
                      {t('forgotPassword.backToLogin')}
                    </Link>
                  </Button>
                </CardFooter>
              </form>
            </Form>
          </>
        )}
        </Card>
      </div>
    </div>
  );
}

