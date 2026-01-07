import { useState, useEffect, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { settingsApi } from '@/api/settings';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Switch } from '@/components/ui/switch';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Skeleton } from '@/components/ui/skeleton';
import { showToast, showSuccess, showError } from "@/lib/sweetalert";
import { useAuth } from '@/contexts/AuthContext';
import { Loader2, Settings, Lock, Mail, Flag } from 'lucide-react';

const PROJECT_CREATION_KEY = 'ProjectCreation.AllowedRoles';
const CATEGORIES = {
  General: 'General',
  Security: 'Security',
  Email: 'Email',
  FeatureFlags: 'FeatureFlags',
} as const;

type Category = typeof CATEGORIES[keyof typeof CATEGORIES];

// Type for form state
type GeneralFormState = {
  projectCreation: string;
  applicationName: string;
  timezone: string;
  language: string;
  dateFormat: string;
};

type SecurityFormState = {
  tokenExpiration: string;
  passwordMinLength: string;
  passwordRequireUppercase: boolean;
  passwordRequireLowercase: boolean;
  passwordRequireNumber: boolean;
  passwordRequireSpecialChar: boolean;
  maxLoginAttempts: string;
  sessionDuration: string;
  require2FA: boolean;
};

type EmailFormState = {
  smtpHost: string;
  smtpPort: string;
  useSsl: boolean;
  smtpUsername: string;
  smtpPassword: string;
  fromEmail: string;
  fromName: string;
};

export default function AdminSettings() {
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<Category>(CATEGORIES.General);
  const [testEmailAddress, setTestEmailAddress] = useState<string>('');

  // Form states
  const [generalForm, setGeneralForm] = useState<GeneralFormState>({
    projectCreation: 'Admin,User',
    applicationName: 'IntelliPM',
    timezone: 'UTC',
    language: 'en',
    dateFormat: 'MM/dd/yyyy',
  });

  const [securityForm, setSecurityForm] = useState<SecurityFormState>({
    tokenExpiration: '15',
    passwordMinLength: '8',
    passwordRequireUppercase: false,
    passwordRequireLowercase: false,
    passwordRequireNumber: false,
    passwordRequireSpecialChar: false,
    maxLoginAttempts: '5',
    sessionDuration: '24',
    require2FA: false,
  });

  const [emailForm, setEmailForm] = useState<EmailFormState>({
    smtpHost: '',
    smtpPort: '587',
    useSsl: true,
    smtpUsername: '',
    smtpPassword: '',
    fromEmail: '',
    fromName: 'IntelliPM',
  });

  // Original values for comparison
  const [originalGeneral, setOriginalGeneral] = useState<GeneralFormState | null>(null);
  const [originalSecurity, setOriginalSecurity] = useState<SecurityFormState | null>(null);
  const [originalEmail, setOriginalEmail] = useState<EmailFormState | null>(null);

  const { data: generalSettings, isLoading: isLoadingGeneral } = useQuery({
    queryKey: ['settings', CATEGORIES.General],
    queryFn: () => settingsApi.getAll(CATEGORIES.General),
  });

  const { data: securitySettings, isLoading: isLoadingSecurity } = useQuery({
    queryKey: ['settings', CATEGORIES.Security],
    queryFn: () => settingsApi.getAll(CATEGORIES.Security),
  });

  const { data: emailSettings, isLoading: isLoadingEmail } = useQuery({
    queryKey: ['settings', CATEGORIES.Email],
    queryFn: () => settingsApi.getAll(CATEGORIES.Email),
  });

  // Initialize form values from settings
  useEffect(() => {
    if (generalSettings) {
      const newForm: GeneralFormState = {
        projectCreation: generalSettings[PROJECT_CREATION_KEY] || 'Admin,User',
        applicationName: generalSettings['General.ApplicationName'] || 'IntelliPM',
        timezone: generalSettings['General.Timezone'] || 'UTC',
        language: generalSettings['General.Language'] || 'en',
        dateFormat: generalSettings['General.DateFormat'] || 'MM/dd/yyyy',
      };
      setGeneralForm(newForm);
      if (!originalGeneral) {
        setOriginalGeneral(newForm);
      }
    }
  }, [generalSettings, originalGeneral]);

  useEffect(() => {
    if (securitySettings) {
      const newForm: SecurityFormState = {
        tokenExpiration: securitySettings['Security.TokenExpirationMinutes'] || '15',
        passwordMinLength: securitySettings['Security.PasswordMinLength'] || '8',
        passwordRequireUppercase: securitySettings['Security.PasswordRequireUppercase'] === 'true',
        passwordRequireLowercase: securitySettings['Security.PasswordRequireLowercase'] === 'true',
        passwordRequireNumber: securitySettings['Security.PasswordRequireNumber'] === 'true',
        passwordRequireSpecialChar: securitySettings['Security.PasswordRequireSpecialChar'] === 'true',
        maxLoginAttempts: securitySettings['Security.MaxLoginAttempts'] || '5',
        sessionDuration: securitySettings['Security.SessionDurationHours'] || '24',
        require2FA: securitySettings['Security.Require2FA'] === 'true',
      };
      setSecurityForm(newForm);
      if (!originalSecurity) {
        setOriginalSecurity(newForm);
      }
    }
  }, [securitySettings, originalSecurity]);

  useEffect(() => {
    if (emailSettings) {
      const newForm: EmailFormState = {
        smtpHost: emailSettings['Email.SmtpHost'] || '',
        smtpPort: emailSettings['Email.SmtpPort'] || '587',
        useSsl: emailSettings['Email.UseSsl'] !== 'false',
        smtpUsername: emailSettings['Email.SmtpUsername'] || '',
        smtpPassword: emailSettings['Email.SmtpPassword'] || '',
        fromEmail: emailSettings['Email.FromEmail'] || '',
        fromName: emailSettings['Email.FromName'] || 'IntelliPM',
      };
      setEmailForm(newForm);
      if (!originalEmail) {
        setOriginalEmail(newForm);
      }
    }
  }, [emailSettings, originalEmail]);

  // Check for changes
  const hasGeneralChanges = useMemo(() => {
    if (!originalGeneral) return false;
    return JSON.stringify(generalForm) !== JSON.stringify(originalGeneral);
  }, [generalForm, originalGeneral]);

  const hasSecurityChanges = useMemo(() => {
    if (!originalSecurity) return false;
    return JSON.stringify(securityForm) !== JSON.stringify(originalSecurity);
  }, [securityForm, originalSecurity]);

  const hasEmailChanges = useMemo(() => {
    if (!originalEmail) return false;
    return JSON.stringify(emailForm) !== JSON.stringify(originalEmail);
  }, [emailForm, originalEmail]);

  // Batch update mutation
  const batchUpdateMutation = useMutation({
    mutationFn: async (updates: Array<{ key: string; value: string; category: string }>) => {
      const results = await Promise.all(
        updates.map(({ key, value, category }) =>
          settingsApi.update(key, value, category)
        )
      );
      return results;
    },
    onSuccess: (_, variables) => {
      const categories = [...new Set(variables.map(v => v.category))];
      categories.forEach(category => {
        queryClient.invalidateQueries({ queryKey: ['settings', category] });
      });
      showSuccess("Settings updated", "All settings have been successfully saved.");
    },
    onError: (error) => {
      const errorMessage = error instanceof Error ? error.message : 'Failed to update settings';
      showError('Failed to update settings', errorMessage);
    },
  });

  const handleSaveGeneral = async () => {
    if (!originalGeneral) return;

    const updates: Array<{ key: string; value: string; category: string }> = [];

    if (generalForm.projectCreation !== originalGeneral.projectCreation) {
      updates.push({
        key: PROJECT_CREATION_KEY,
        value: generalForm.projectCreation,
        category: CATEGORIES.General,
      });
    }
    if (generalForm.applicationName !== originalGeneral.applicationName) {
      updates.push({
        key: 'General.ApplicationName',
        value: generalForm.applicationName,
        category: CATEGORIES.General,
      });
    }
    if (generalForm.timezone !== originalGeneral.timezone) {
      updates.push({
        key: 'General.Timezone',
        value: generalForm.timezone,
        category: CATEGORIES.General,
      });
    }
    if (generalForm.language !== originalGeneral.language) {
      updates.push({
        key: 'General.Language',
        value: generalForm.language,
        category: CATEGORIES.General,
      });
    }
    if (generalForm.dateFormat !== originalGeneral.dateFormat) {
      updates.push({
        key: 'General.DateFormat',
        value: generalForm.dateFormat,
        category: CATEGORIES.General,
      });
    }

    if (updates.length > 0) {
      await batchUpdateMutation.mutateAsync(updates);
      setOriginalGeneral({ ...generalForm });
    }
  };

  const handleSaveSecurity = async () => {
    if (!originalSecurity) return;

    const updates: Array<{ key: string; value: string; category: string }> = [];

    if (securityForm.tokenExpiration !== originalSecurity.tokenExpiration) {
      updates.push({
        key: 'Security.TokenExpirationMinutes',
        value: securityForm.tokenExpiration,
        category: CATEGORIES.Security,
      });
    }
    if (securityForm.passwordMinLength !== originalSecurity.passwordMinLength) {
      updates.push({
        key: 'Security.PasswordMinLength',
        value: securityForm.passwordMinLength,
        category: CATEGORIES.Security,
      });
    }
    if (securityForm.passwordRequireUppercase !== originalSecurity.passwordRequireUppercase) {
      updates.push({
        key: 'Security.PasswordRequireUppercase',
        value: securityForm.passwordRequireUppercase.toString(),
        category: CATEGORIES.Security,
      });
    }
    if (securityForm.passwordRequireLowercase !== originalSecurity.passwordRequireLowercase) {
      updates.push({
        key: 'Security.PasswordRequireLowercase',
        value: securityForm.passwordRequireLowercase.toString(),
        category: CATEGORIES.Security,
      });
    }
    if (securityForm.passwordRequireNumber !== originalSecurity.passwordRequireNumber) {
      updates.push({
        key: 'Security.PasswordRequireNumber',
        value: securityForm.passwordRequireNumber.toString(),
        category: CATEGORIES.Security,
      });
    }
    if (securityForm.passwordRequireSpecialChar !== originalSecurity.passwordRequireSpecialChar) {
      updates.push({
        key: 'Security.PasswordRequireSpecialChar',
        value: securityForm.passwordRequireSpecialChar.toString(),
        category: CATEGORIES.Security,
      });
    }
    if (securityForm.maxLoginAttempts !== originalSecurity.maxLoginAttempts) {
      updates.push({
        key: 'Security.MaxLoginAttempts',
        value: securityForm.maxLoginAttempts,
        category: CATEGORIES.Security,
      });
    }
    if (securityForm.sessionDuration !== originalSecurity.sessionDuration) {
      updates.push({
        key: 'Security.SessionDurationHours',
        value: securityForm.sessionDuration,
        category: CATEGORIES.Security,
      });
    }
    if (securityForm.require2FA !== originalSecurity.require2FA) {
      updates.push({
        key: 'Security.Require2FA',
        value: securityForm.require2FA.toString(),
        category: CATEGORIES.Security,
      });
    }

    if (updates.length > 0) {
      await batchUpdateMutation.mutateAsync(updates);
      setOriginalSecurity({ ...securityForm });
    }
  };

  const handleSaveEmail = async () => {
    if (!originalEmail) return;

    const updates: Array<{ key: string; value: string; category: string }> = [];

    if (emailForm.smtpHost !== originalEmail.smtpHost) {
      updates.push({
        key: 'Email.SmtpHost',
        value: emailForm.smtpHost,
        category: CATEGORIES.Email,
      });
    }
    if (emailForm.smtpPort !== originalEmail.smtpPort) {
      updates.push({
        key: 'Email.SmtpPort',
        value: emailForm.smtpPort,
        category: CATEGORIES.Email,
      });
    }
    if (emailForm.useSsl !== originalEmail.useSsl) {
      updates.push({
        key: 'Email.UseSsl',
        value: emailForm.useSsl.toString(),
        category: CATEGORIES.Email,
      });
    }
    if (emailForm.smtpUsername !== originalEmail.smtpUsername) {
      updates.push({
        key: 'Email.SmtpUsername',
        value: emailForm.smtpUsername,
        category: CATEGORIES.Email,
      });
    }
    if (emailForm.smtpPassword !== originalEmail.smtpPassword) {
      updates.push({
        key: 'Email.SmtpPassword',
        value: emailForm.smtpPassword,
        category: CATEGORIES.Email,
      });
    }
    if (emailForm.fromEmail !== originalEmail.fromEmail) {
      updates.push({
        key: 'Email.FromEmail',
        value: emailForm.fromEmail,
        category: CATEGORIES.Email,
      });
    }
    if (emailForm.fromName !== originalEmail.fromName) {
      updates.push({
        key: 'Email.FromName',
        value: emailForm.fromName,
        category: CATEGORIES.Email,
      });
    }

    if (updates.length > 0) {
      await batchUpdateMutation.mutateAsync(updates);
      setOriginalEmail({ ...emailForm });
    }
  };

  const isSaving = batchUpdateMutation.isPending;

  return (
    <div className="container mx-auto p-6 space-y-6">
      <div>
        <h1 className="text-3xl font-bold mb-1">Settings</h1>
        <p className="text-muted-foreground">Configure global application settings.</p>
      </div>

      <Tabs value={activeTab} onValueChange={(v) => setActiveTab(v as Category)} className="space-y-4">
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value={CATEGORIES.General} className="flex items-center gap-2">
            <Settings className="h-4 w-4" />
            General
          </TabsTrigger>
          <TabsTrigger value={CATEGORIES.Security} className="flex items-center gap-2">
            <Lock className="h-4 w-4" />
            Security
          </TabsTrigger>
          <TabsTrigger value={CATEGORIES.Email} className="flex items-center gap-2">
            <Mail className="h-4 w-4" />
            Email
          </TabsTrigger>
          <TabsTrigger value={CATEGORIES.FeatureFlags} className="flex items-center gap-2">
            <Flag className="h-4 w-4" />
            Feature Flags
          </TabsTrigger>
        </TabsList>

        {/* General Settings */}
        <TabsContent value={CATEGORIES.General} className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Project Creation</CardTitle>
              <CardDescription>Control who can create new projects in the system.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {isLoadingGeneral ? (
                <div className="space-y-4">
                  <Skeleton className="h-4 w-32" />
                  <Skeleton className="h-10 w-full" />
                </div>
              ) : (
                <>
                  <div className="space-y-4">
                    <Label id="project-creation-roles-label" className="text-base font-semibold">Allowed Roles</Label>
                    <RadioGroup
                      id="project-creation-roles"
                      aria-labelledby="project-creation-roles-label"
                      value={generalForm.projectCreation}
                      onValueChange={(value) => setGeneralForm({ ...generalForm, projectCreation: value })}
                      disabled={isSaving}
                    >
                      <div className="flex items-center space-x-2">
                        <RadioGroupItem value="Admin" id="admin-only" />
                        <Label htmlFor="admin-only" className="font-normal cursor-pointer">
                          Admin Only
                        </Label>
                      </div>
                      <div className="flex items-center space-x-2">
                        <RadioGroupItem value="Admin,User" id="all-users" />
                        <Label htmlFor="all-users" className="font-normal cursor-pointer">
                          All Users
                        </Label>
                      </div>
                    </RadioGroup>
                    <p className="text-sm text-muted-foreground">
                      {generalForm.projectCreation === 'Admin'
                        ? 'Only administrators can create new projects.'
                        : 'All authenticated users can create new projects.'}
                    </p>
                  </div>
                </>
              )}
            </CardContent>
          </Card>

          {/* General Application Settings */}
          <Card>
            <CardHeader>
              <CardTitle>Application Settings</CardTitle>
              <CardDescription>Configure general application settings.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {isLoadingGeneral ? (
                <div className="space-y-4">
                  <Skeleton className="h-4 w-32" />
                  <Skeleton className="h-10 w-full" />
                </div>
              ) : (
                <>
                  <div className="space-y-6">
                    <div className="space-y-2">
                      <Label htmlFor="appName">Application Name</Label>
                      <Input
                        id="appName"
                        name="applicationName"
                        type="text"
                        placeholder="IntelliPM"
                        value={generalForm.applicationName}
                        onChange={(e) => setGeneralForm({ ...generalForm, applicationName: e.target.value })}
                        disabled={isSaving}
                      />
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="timezone">Default Timezone</Label>
                      <Input
                        id="timezone"
                        name="timezone"
                        type="text"
                        placeholder="UTC"
                        value={generalForm.timezone}
                        onChange={(e) => setGeneralForm({ ...generalForm, timezone: e.target.value })}
                        disabled={isSaving}
                      />
                      <p className="text-sm text-muted-foreground">
                        Default timezone for the application (e.g., UTC, America/New_York)
                      </p>
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="language">Default Language</Label>
                      <Input
                        id="language"
                        name="language"
                        type="text"
                        placeholder="en"
                        value={generalForm.language}
                        onChange={(e) => setGeneralForm({ ...generalForm, language: e.target.value })}
                        disabled={isSaving}
                      />
                      <p className="text-sm text-muted-foreground">
                        Default language code (e.g., en, fr, es)
                      </p>
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="dateFormat">Date Format</Label>
                      <Input
                        id="dateFormat"
                        name="dateFormat"
                        type="text"
                        placeholder="MM/dd/yyyy"
                        value={generalForm.dateFormat}
                        onChange={(e) => setGeneralForm({ ...generalForm, dateFormat: e.target.value })}
                        disabled={isSaving}
                      />
                      <p className="text-sm text-muted-foreground">
                        Default date format (e.g., MM/dd/yyyy, dd/MM/yyyy, yyyy-MM-dd)
                      </p>
                    </div>
                  </div>

                  <div className="flex justify-end pt-4 border-t">
                    <Button
                      onClick={handleSaveGeneral}
                      disabled={!hasGeneralChanges || isSaving}
                    >
                      {isSaving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                      Save Changes
                    </Button>
                  </div>
                </>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Security Settings */}
        <TabsContent value={CATEGORIES.Security} className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Security Settings</CardTitle>
              <CardDescription>Configure security-related settings.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {isLoadingSecurity ? (
                <div className="space-y-4">
                  <Skeleton className="h-4 w-32" />
                  <Skeleton className="h-10 w-full" />
                </div>
              ) : (
                <>
                  <div className="space-y-6">
                    {/* Token Expiration */}
                    <div className="space-y-2">
                      <Label htmlFor="tokenExpiration">Access Token Expiration (minutes)</Label>
                      <Input
                        id="tokenExpiration"
                        name="tokenExpiration"
                        type="number"
                        min="5"
                        max="1440"
                        value={securityForm.tokenExpiration}
                        onChange={(e) => setSecurityForm({ ...securityForm, tokenExpiration: e.target.value })}
                        disabled={isSaving}
                      />
                      <p className="text-sm text-muted-foreground">
                        Duration in minutes before access tokens expire (default: 15 minutes)
                      </p>
                    </div>

                    {/* Password Policy */}
                    <div className="space-y-4" role="group" aria-labelledby="password-policy-label">
                      <Label id="password-policy-label" className="text-base font-semibold">Password Policy</Label>
                      
                      <div className="space-y-2">
                        <Label htmlFor="minPasswordLength">Minimum Password Length</Label>
                        <Input
                          id="minPasswordLength"
                          name="passwordMinLength"
                          type="number"
                          min="6"
                          max="128"
                          value={securityForm.passwordMinLength}
                          onChange={(e) => setSecurityForm({ ...securityForm, passwordMinLength: e.target.value })}
                          disabled={isSaving}
                        />
                      </div>

                      <div className="flex items-center space-x-2">
                        <Switch
                          id="requireUppercase"
                          name="passwordRequireUppercase"
                          checked={securityForm.passwordRequireUppercase}
                          onCheckedChange={(checked) => setSecurityForm({ ...securityForm, passwordRequireUppercase: checked })}
                          disabled={isSaving}
                        />
                        <Label htmlFor="requireUppercase" className="cursor-pointer">
                          Require uppercase letter
                        </Label>
                      </div>

                      <div className="flex items-center space-x-2">
                        <Switch
                          id="requireLowercase"
                          name="passwordRequireLowercase"
                          checked={securityForm.passwordRequireLowercase}
                          onCheckedChange={(checked) => setSecurityForm({ ...securityForm, passwordRequireLowercase: checked })}
                          disabled={isSaving}
                        />
                        <Label htmlFor="requireLowercase" className="cursor-pointer">
                          Require lowercase letter
                        </Label>
                      </div>

                      <div className="flex items-center space-x-2">
                        <Switch
                          id="requireNumber"
                          name="passwordRequireNumber"
                          checked={securityForm.passwordRequireNumber}
                          onCheckedChange={(checked) => setSecurityForm({ ...securityForm, passwordRequireNumber: checked })}
                          disabled={isSaving}
                        />
                        <Label htmlFor="requireNumber" className="cursor-pointer">
                          Require number
                        </Label>
                      </div>

                      <div className="flex items-center space-x-2">
                        <Switch
                          id="requireSpecialChar"
                          name="passwordRequireSpecialChar"
                          checked={securityForm.passwordRequireSpecialChar}
                          onCheckedChange={(checked) => setSecurityForm({ ...securityForm, passwordRequireSpecialChar: checked })}
                          disabled={isSaving}
                        />
                        <Label htmlFor="requireSpecialChar" className="cursor-pointer">
                          Require special character
                        </Label>
                      </div>
                    </div>

                    {/* Max Login Attempts */}
                    <div className="space-y-2">
                      <Label htmlFor="maxLoginAttempts">Max Login Attempts</Label>
                      <Input
                        id="maxLoginAttempts"
                        name="maxLoginAttempts"
                        type="number"
                        min="3"
                        max="10"
                        value={securityForm.maxLoginAttempts}
                        onChange={(e) => setSecurityForm({ ...securityForm, maxLoginAttempts: e.target.value })}
                        disabled={isSaving}
                      />
                      <p className="text-sm text-muted-foreground">
                        Maximum number of failed login attempts before account lockout
                      </p>
                    </div>

                    {/* Session Duration */}
                    <div className="space-y-2">
                      <Label htmlFor="sessionDuration">Session Duration (hours)</Label>
                      <Input
                        id="sessionDuration"
                        name="sessionDuration"
                        type="number"
                        min="1"
                        max="168"
                        value={securityForm.sessionDuration}
                        onChange={(e) => setSecurityForm({ ...securityForm, sessionDuration: e.target.value })}
                        disabled={isSaving}
                      />
                      <p className="text-sm text-muted-foreground">
                        Duration in hours before user session expires
                      </p>
                    </div>

                    {/* 2FA Required */}
                    <div className="flex items-center space-x-2">
                      <Switch
                        id="require2FA"
                        name="require2FA"
                        checked={securityForm.require2FA}
                        onCheckedChange={(checked) => setSecurityForm({ ...securityForm, require2FA: checked })}
                        disabled={isSaving}
                      />
                      <Label htmlFor="require2FA" className="cursor-pointer">
                        Require Two-Factor Authentication
                      </Label>
                    </div>
                  </div>

                  <div className="flex justify-end pt-4 border-t">
                    <Button
                      onClick={handleSaveSecurity}
                      disabled={!hasSecurityChanges || isSaving}
                    >
                      {isSaving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                      Save Changes
                    </Button>
                  </div>
                </>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Email Settings */}
        <TabsContent value={CATEGORIES.Email} className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Email Settings</CardTitle>
              <CardDescription>Configure email server and notification settings.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {isLoadingEmail ? (
                <div className="space-y-4">
                  <Skeleton className="h-4 w-32" />
                  <Skeleton className="h-10 w-full" />
                </div>
              ) : (
                <>
                  <div className="space-y-6">
                    {/* SMTP Configuration */}
                    <div className="space-y-4" role="group" aria-labelledby="smtp-config-label">
                      <Label id="smtp-config-label" className="text-base font-semibold">SMTP Configuration</Label>
                      
                      <div className="space-y-2">
                        <Label htmlFor="smtpHost">SMTP Host</Label>
                        <Input
                          id="smtpHost"
                          name="smtpHost"
                          type="text"
                          placeholder="smtp.gmail.com"
                          value={emailForm.smtpHost}
                          onChange={(e) => setEmailForm({ ...emailForm, smtpHost: e.target.value })}
                          disabled={isSaving}
                        />
                      </div>

                      <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                          <Label htmlFor="smtpPort">SMTP Port</Label>
                          <Input
                            id="smtpPort"
                            name="smtpPort"
                            type="number"
                            min="1"
                            max="65535"
                            placeholder="587"
                            value={emailForm.smtpPort}
                            onChange={(e) => setEmailForm({ ...emailForm, smtpPort: e.target.value })}
                            disabled={isSaving}
                          />
                        </div>

                        <div className="space-y-2">
                          <div className="flex items-center space-x-2">
                            <Switch
                              id="useSsl"
                              name="useSsl"
                              checked={emailForm.useSsl}
                              onCheckedChange={(checked) => setEmailForm({ ...emailForm, useSsl: checked })}
                              disabled={isSaving}
                            />
                            <Label htmlFor="useSsl" className="cursor-pointer">
                              Use SSL/TLS ({emailForm.useSsl ? 'Enabled' : 'Disabled'})
                            </Label>
                          </div>
                        </div>
                      </div>

                      <div className="space-y-2">
                        <Label htmlFor="smtpUsername">SMTP Username</Label>
                        <Input
                          id="smtpUsername"
                          name="smtpUsername"
                          type="text"
                          placeholder="your-email@gmail.com"
                          value={emailForm.smtpUsername}
                          onChange={(e) => setEmailForm({ ...emailForm, smtpUsername: e.target.value })}
                          disabled={isSaving}
                        />
                      </div>

                      <div className="space-y-2">
                        <Label htmlFor="smtpPassword">SMTP Password</Label>
                        <Input
                          id="smtpPassword"
                          name="smtpPassword"
                          type="password"
                          placeholder="Enter SMTP password"
                          value={emailForm.smtpPassword}
                          onChange={(e) => setEmailForm({ ...emailForm, smtpPassword: e.target.value })}
                          disabled={isSaving}
                        />
                        <p className="text-sm text-muted-foreground">
                          For Gmail, use an App Password instead of your regular password
                        </p>
                      </div>

                      <div className="space-y-2">
                        <Label htmlFor="fromEmail">From Email</Label>
                        <Input
                          id="fromEmail"
                          name="fromEmail"
                          type="email"
                          placeholder="noreply@intellipm.com"
                          value={emailForm.fromEmail}
                          onChange={(e) => setEmailForm({ ...emailForm, fromEmail: e.target.value })}
                          disabled={isSaving}
                        />
                      </div>

                      <div className="space-y-2">
                        <Label htmlFor="fromName">From Name</Label>
                        <Input
                          id="fromName"
                          name="fromName"
                          type="text"
                          placeholder="IntelliPM"
                          value={emailForm.fromName}
                          onChange={(e) => setEmailForm({ ...emailForm, fromName: e.target.value })}
                          disabled={isSaving}
                        />
                      </div>
                    </div>

                    {/* Test Email */}
                    <div className="pt-4 border-t">
                      <div className="flex items-center justify-between">
                        <div>
                          <h3 className="text-base font-semibold">Test Email Configuration</h3>
                          <p className="text-sm text-muted-foreground">
                            Send a test email to verify your SMTP settings
                          </p>
                        </div>
                        <div className="flex flex-col gap-2">
                          <Label htmlFor="test-email-address" className="sr-only">Email address for test email</Label>
                          <div className="flex items-center gap-2">
                            <Input
                              id="test-email-address"
                              name="testEmail"
                              type="email"
                              placeholder={user?.email || 'test@example.com'}
                              value={testEmailAddress}
                              onChange={(e) => setTestEmailAddress(e.target.value)}
                              className="w-64"
                            />
                            <Button
                              variant="outline"
                              onClick={async () => {
                                const email = testEmailAddress || user?.email;
                                if (!email) {
                                  showError("Email required", "Please enter an email address or use your account email.");
                                  return;
                                }
                                try {
                                  const result = await settingsApi.sendTestEmail(email);
                                  if (result.success) {
                                    showToast('Test email sent', "success");
                                  } else {
                                    showError('Failed to send test email', result.message || 'Unknown error');
                                  }
                                } catch (error) {
                                  const errorMessage = error instanceof Error ? error.message : 'Failed to send test email. Please check SMTP configuration.';
                                  showError('Error sending test email', errorMessage);
                                }
                              }}
                              disabled={isSaving}
                            >
                              <Mail className="mr-2 h-4 w-4" />
                              Send Test Email
                            </Button>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>

                  <div className="flex justify-end pt-4 border-t">
                    <Button
                      onClick={handleSaveEmail}
                      disabled={!hasEmailChanges || isSaving}
                    >
                      {isSaving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                      Save Changes
                    </Button>
                  </div>
                </>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Feature Flags */}
        <TabsContent value={CATEGORIES.FeatureFlags} className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Feature Flags</CardTitle>
              <CardDescription>Manage feature flags from the Feature Flags admin page.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="text-center py-8 text-muted-foreground">
                <p>Use the Feature Flags page to manage feature flags.</p>
                <Button
                  variant="link"
                  className="p-0 h-auto mt-4"
                  onClick={() => navigate('/admin/feature-flags')}
                >
                  Feature Flags
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
