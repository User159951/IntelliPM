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
import { Loader2, Copy, Check } from 'lucide-react';
import { usersApi } from '@/api/users';
import type { GlobalRole } from '@/types';

interface InviteUserDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess?: () => void;
}

export function InviteUserDialog({ open, onOpenChange, onSuccess }: InviteUserDialogProps) {
  const queryClient = useQueryClient();
  const [email, setEmail] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [role, setRole] = useState<GlobalRole>('User');
  const [invitationLink, setInvitationLink] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);

  const inviteMutation = useMutation({
    mutationFn: async (data: {
      email: string;
      role: GlobalRole;
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
      showToast('Invitation envoyée', 'success');
      if (onSuccess) {
        onSuccess();
      }
    },
    onError: (error: any) => {
      const errorMessage =
        error.response?.data?.detail ||
        error.response?.data?.error ||
        error.message ||
        'Échec de l\'envoi de l\'invitation';
      showError('Échec de l\'invitation', errorMessage);
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
      showToast('Le lien d\'invitation a été copié dans le presse-papiers.', 'success');
      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      showError('Échec de la copie', 'Veuillez copier le lien manuellement.');
    }
  };

  const handleClose = () => {
    setEmail('');
    setFirstName('');
    setLastName('');
    setRole('User');
    setInvitationLink(null);
    setCopied(false);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Inviter un utilisateur</DialogTitle>
          <DialogDescription>
            Envoyez une invitation à un nouvel utilisateur. Il recevra un lien par email pour créer son compte.
          </DialogDescription>
        </DialogHeader>

        {invitationLink ? (
          <div className="space-y-4">
            <div className="space-y-2">
              <Label>Lien d'invitation</Label>
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
                  title="Copier le lien"
                >
                  {copied ? (
                    <Check className="h-4 w-4 text-green-600" />
                  ) : (
                    <Copy className="h-4 w-4" />
                  )}
                </Button>
              </div>
              <p className="text-xs text-muted-foreground">
                Partagez ce lien avec l'utilisateur. Il expirera dans 72 heures.
              </p>
            </div>
            <DialogFooter>
              <Button onClick={handleClose}>Terminé</Button>
            </DialogFooter>
          </div>
        ) : (
          <form onSubmit={handleSubmit}>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="email">Email *</Label>
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
                  <Label htmlFor="firstName">Prénom *</Label>
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
                  <Label htmlFor="lastName">Nom *</Label>
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
                <Label htmlFor="role">Rôle *</Label>
                <Select
                  value={role}
                  onValueChange={(value) => setRole(value as GlobalRole)}
                  disabled={inviteMutation.isPending}
                >
                  <SelectTrigger id="role">
                    <SelectValue placeholder="Sélectionner un rôle" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="User">Utilisateur</SelectItem>
                    <SelectItem value="Admin">Administrateur</SelectItem>
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
                Annuler
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
                Envoyer l'invitation
              </Button>
            </DialogFooter>
          </form>
        )}
      </DialogContent>
    </Dialog>
  );
}
