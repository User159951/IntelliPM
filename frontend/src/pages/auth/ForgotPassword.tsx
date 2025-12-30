import { useState } from 'react';
import { Link } from 'react-router-dom';
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

const forgotPasswordSchema = z.object({
  emailOrUsername: z
    .string()
    .min(1, 'L\'adresse email ou le nom d\'utilisateur est requis')
    .max(255, 'L\'adresse email ou le nom d\'utilisateur ne doit pas dépasser 255 caractères'),
});

type ForgotPasswordFormValues = z.infer<typeof forgotPasswordSchema>;

export default function ForgotPassword() {
  const [isLoading, setIsLoading] = useState(false);
  const [emailSent, setEmailSent] = useState(false);
  const [isEmailMode, setIsEmailMode] = useState(true);

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
      showToast(result.message || 'Email envoyé', 'success');
    } catch (error) {
      showError(
        'Erreur',
        error instanceof Error ? error.message : 'Une erreur est survenue'
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
            <CardTitle className="text-xl">Mot de passe oublié</CardTitle>
            <CardDescription>
            {emailSent
              ? 'Vérifiez votre boîte email pour réinitialiser votre mot de passe'
              : `Entrez votre ${isEmailMode ? 'adresse email' : 'nom d\'utilisateur'} pour recevoir un lien de réinitialisation`}
          </CardDescription>
        </CardHeader>
        {emailSent ? (
          <CardContent className="space-y-4">
            <div className="flex items-center justify-center p-6 bg-green-50 rounded-lg border border-green-200">
              <Mail className="h-12 w-12 text-green-600" />
            </div>
            <p className="text-center text-sm text-muted-foreground">
              Si un compte existe avec cet email ou nom d'utilisateur, un lien de réinitialisation de mot de passe vous a été envoyé.
            </p>
            <p className="text-center text-xs text-muted-foreground">
              Le lien expire dans 1 heure. Vérifiez aussi votre dossier spam.
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
                        <FormLabel>{isEmailMode ? 'Adresse email' : 'Nom d\'utilisateur'}</FormLabel>
                        <FormControl>
                          <Input
                            placeholder={isEmailMode ? 'Entrez votre adresse email' : 'Entrez votre nom d\'utilisateur'}
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
                            {isEmailMode ? 'Réinitialiser avec nom d\'utilisateur' : 'Réinitialiser avec adresse email'}
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
                    Envoyer le lien de réinitialisation
                  </Button>
                  <Button asChild variant="ghost" className="w-full">
                    <Link to="/login">
                      <ArrowLeft className="mr-2 h-4 w-4" />
                      Retour à la connexion
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

