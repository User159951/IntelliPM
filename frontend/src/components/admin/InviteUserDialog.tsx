import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { showToast, showError } from '@/lib/sweetalert';
import { useTranslation } from 'react-i18next';
import { Loader2, Copy, Check } from 'lucide-react';
import { usersApi, type InviteOrganizationUserResponse } from '@/api/users';

interface InviteUserResponseWithEmailStatus extends InviteOrganizationUserResponse {
  emailSent?: boolean;
  emailFailed?: boolean;
}

interface InviteUserDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess?: () => void;
}

export function InviteUserDialog({ open, onOpenChange, onSuccess }: InviteUserDialogProps) {
  const { t } = useTranslation('admin');
  const queryClient = useQueryClient();
  const [email, setEmail] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [role, setRole] = useState<'Admin' | 'User'>('User');
  const [invitationLink, setInvitationLink] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);

  const [emailFailed, setEmailFailed] = useState(false);

  const inviteMutation = useMutation({
    mutationFn: async (data: {
      email: string;
      role: 'Admin' | 'User';
      firstName: string;
      lastName: string;
    }) => {
      const response = await usersApi.invite({
        email: data.email,
        role: data.role,
        firstName: data.firstName,
        lastName: data.lastName,
      });
      return response;
    },
    onSuccess: (data) => {
      setInvitationLink(data.invitationLink);
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
      
      // Check if response indicates email failure
      // Note: Backend may not return email status, so we check for email-related errors
      const responseWithEmailStatus = data as InviteUserResponseWithEmailStatus;
      const emailFailed = responseWithEmailStatus.emailSent === false || responseWithEmailStatus.emailFailed === true;
      setEmailFailed(emailFailed);
      
      if (emailFailed) {
        showToast(t('dialogs.invite.emailFailed'), 'warning');
      } else {
        showToast(t('dialogs.invite.inviteSuccess'), 'success');
      }
      
      if (onSuccess) {
        onSuccess();
      }
    },
    onError: (error: unknown) => {
      const apiError = error as { response?: { data?: { detail?: string; error?: string } }; message?: string };
      const errorMessage =
        apiError.response?.data?.detail ||
        apiError.response?.data?.error ||
        apiError.message ||
        t('dialogs.invite.inviteErrorMessage');
      
      // Check if error is related to email/SMTP failure
      const isEmailError = errorMessage.toLowerCase().includes('email') || 
                          errorMessage.toLowerCase().includes('smtp') ||
                          errorMessage.toLowerCase().includes('mail');
      
      if (isEmailError) {
        showError(t('dialogs.invite.emailError'), t('dialogs.invite.emailErrorMessage'));
      } else {
        showError(t('dialogs.invite.inviteError'), errorMessage);
      }
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!email.trim() || !firstName.trim() || !lastName.trim()) return;

    inviteMutation.mutate({
      email: email.trim(),
      role,
      firstName: firstName.trim(),
      lastName: lastName.trim(),
    });
  };

  const handleCopyLink = async () => {
    if (!invitationLink) return;

    try {
      await navigator.clipboard.writeText(invitationLink);
      setCopied(true);
      showToast(t('dialogs.invite.linkCopied'), 'success');
      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      showError(t('dialogs.invite.copyFailed'), t('dialogs.invite.copyFailedMessage'));
    }
  };

  const handleClose = () => {
    setEmail('');
    setFirstName('');
    setLastName('');
    setRole('User');
    setInvitationLink(null);
    setCopied(false);
    setEmailFailed(false);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>{t('dialogs.invite.title')}</DialogTitle>
          <DialogDescription>
            {t('dialogs.invite.description')}
          </DialogDescription>
        </DialogHeader>

        {invitationLink ? (
          <div className="space-y-4">
            {emailFailed && (
              <div className="p-3 rounded-lg bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800">
                <p className="text-sm text-yellow-800 dark:text-yellow-200 font-medium">
                  ⚠️ {t('dialogs.invite.emailFailedWarning')}
                </p>
                <p className="text-xs text-yellow-700 dark:text-yellow-300 mt-1">
                  {t('dialogs.invite.emailFailedMessage')}
                </p>
              </div>
            )}
            <div className="space-y-2">
              <Label>{t('dialogs.invite.invitationLink')}</Label>
              <div className="flex gap-2">
                <Input 
                  id="invitation-link"
                  name="invitation-link"
                  value={invitationLink} 
                  readOnly 
                  className="font-mono text-sm" 
                />
                <Button
                  type="button"
                  variant="outline"
                  size="icon"
                  onClick={handleCopyLink}
                  title={t('dialogs.invite.copyLink')}
                >
                  {copied ? (
                    <Check className="h-4 w-4 text-green-600" />
                  ) : (
                    <Copy className="h-4 w-4" />
                  )}
                </Button>
              </div>
              <p className="text-xs text-muted-foreground">
                {t('dialogs.invite.linkExpires')}
              </p>
            </div>
            <DialogFooter>
              <Button onClick={handleClose}>{t('dialogs.invite.done')}</Button>
            </DialogFooter>
          </div>
        ) : (
          <form onSubmit={handleSubmit}>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="email">{t('dialogs.invite.email')}</Label>
                <Input
                  id="email"
                  name="email"
                  type="email"
                  autoComplete="email"
                  placeholder="user@example.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                  disabled={inviteMutation.isPending}
                />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="firstName">{t('dialogs.invite.firstName')}</Label>
                  <Input
                    id="firstName"
                    name="firstName"
                    type="text"
                    autoComplete="given-name"
                    placeholder="John"
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    required
                    disabled={inviteMutation.isPending}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="lastName">{t('dialogs.invite.lastName')}</Label>
                  <Input
                    id="lastName"
                    name="lastName"
                    type="text"
                    autoComplete="family-name"
                    placeholder="Doe"
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    required
                    disabled={inviteMutation.isPending}
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="role">{t('dialogs.invite.role')}</Label>
                <Select
                  value={role}
                  onValueChange={(value) => {
                    if (value === 'Admin' || value === 'User') {
                      setRole(value);
                    }
                  }}
                  disabled={inviteMutation.isPending}
                >
                  <SelectTrigger id="role">
                    <SelectValue placeholder={t('dialogs.invite.rolePlaceholder')} />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="User">{t('dialogs.invite.roleUser')}</SelectItem>
                    <SelectItem value="Admin">{t('dialogs.invite.roleAdmin')}</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={handleClose}
                disabled={inviteMutation.isPending}
              >
                {t('dialogs.invite.cancel')}
              </Button>
              <Button
                type="submit"
                disabled={
                  inviteMutation.isPending ||
                  !email.trim() ||
                  !firstName.trim() ||
                  !lastName.trim()
                }
              >
                {inviteMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                {t('dialogs.invite.send')}
              </Button>
            </DialogFooter>
          </form>
        )}
      </DialogContent>
    </Dialog>
  );
}
