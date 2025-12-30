import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { settingsApi } from '@/api/settings';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Switch } from '@/components/ui/switch';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Skeleton } from '@/components/ui/skeleton';
import { showToast, showSuccess, showError, showWarning } from "@/lib/sweetalert";
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

export default function AdminSettings() {
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<Category>(CATEGORIES.General);
  const [projectCreationValue, setProjectCreationValue] = useState<string>('Admin,User');
  const [testEmailAddress, setTestEmailAddress] = useState<string>('');

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

  const updateMutation = useMutation({
    mutationFn: ({ key, value, category }: { key: string; value: string; category?: string }) =>
      settingsApi.update(key, value, category),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['settings', variables.category] });
      showSuccess("Settings updated", "Settings have been successfully updated.");
    },
    onError: (error: any) => {
      const errorMessage = error.response?.data?.error || error.message || 'Failed to update settings';
      showError('Failed to update settings');
    },
  });

  const handleSaveProjectCreation = () => {
    updateMutation.mutate({
      key: PROJECT_CREATION_KEY,
      value: projectCreationValue,
      category: CATEGORIES.General,
    });
  };

  // Initialize form values from settings
  useEffect(() => {
    if (generalSettings?.[PROJECT_CREATION_KEY]) {
      setProjectCreationValue(generalSettings[PROJECT_CREATION_KEY]);
    }
  }, [generalSettings]);

  const hasProjectCreationChanges =
    generalSettings && generalSettings[PROJECT_CREATION_KEY] !== projectCreationValue;

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
                    <Label className="text-base font-semibold">Allowed Roles</Label>
                    <RadioGroup
                      value={projectCreationValue}
                      onValueChange={setProjectCreationValue}
                      disabled={updateMutation.isPending}
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
                      {projectCreationValue === 'Admin'
                        ? 'Only administrators can create new projects.'
                        : 'All authenticated users can create new projects.'}
                    </p>
                  </div>

                  <div className="flex justify-end">
                    <Button
                      onClick={handleSaveProjectCreation}
                      disabled={!hasProjectCreationChanges || updateMutation.isPending}
                    >
                      {updateMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                      Save Changes
                    </Button>
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
                <div className="space-y-6">
                  <div className="space-y-2">
                    <Label htmlFor="appName">Application Name</Label>
                    <Input
                      id="appName"
                      type="text"
                      placeholder="IntelliPM"
                      defaultValue={generalSettings?.['General.ApplicationName'] || 'IntelliPM'}
                      onChange={(e) => {
                        const value = e.target.value;
                        if (value) {
                          updateMutation.mutate({
                            key: 'General.ApplicationName',
                            value,
                            category: CATEGORIES.General,
                          });
                        }
                      }}
                      disabled={updateMutation.isPending}
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="timezone">Default Timezone</Label>
                    <Input
                      id="timezone"
                      type="text"
                      placeholder="UTC"
                      defaultValue={generalSettings?.['General.Timezone'] || 'UTC'}
                      onChange={(e) => {
                        const value = e.target.value;
                        if (value) {
                          updateMutation.mutate({
                            key: 'General.Timezone',
                            value,
                            category: CATEGORIES.General,
                          });
                        }
                      }}
                      disabled={updateMutation.isPending}
                    />
                    <p className="text-sm text-muted-foreground">
                      Default timezone for the application (e.g., UTC, America/New_York)
                    </p>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="language">Default Language</Label>
                    <Input
                      id="language"
                      type="text"
                      placeholder="en"
                      defaultValue={generalSettings?.['General.Language'] || 'en'}
                      onChange={(e) => {
                        const value = e.target.value;
                        if (value) {
                          updateMutation.mutate({
                            key: 'General.Language',
                            value,
                            category: CATEGORIES.General,
                          });
                        }
                      }}
                      disabled={updateMutation.isPending}
                    />
                    <p className="text-sm text-muted-foreground">
                      Default language code (e.g., en, fr, es)
                    </p>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="dateFormat">Date Format</Label>
                    <Input
                      id="dateFormat"
                      type="text"
                      placeholder="MM/dd/yyyy"
                      defaultValue={generalSettings?.['General.DateFormat'] || 'MM/dd/yyyy'}
                      onChange={(e) => {
                        const value = e.target.value;
                        if (value) {
                          updateMutation.mutate({
                            key: 'General.DateFormat',
                            value,
                            category: CATEGORIES.General,
                          });
                        }
                      }}
                      disabled={updateMutation.isPending}
                    />
                    <p className="text-sm text-muted-foreground">
                      Default date format (e.g., MM/dd/yyyy, dd/MM/yyyy, yyyy-MM-dd)
                    </p>
                  </div>
                </div>
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
                <div className="space-y-6">
                  {/* Token Expiration */}
                  <div className="space-y-2">
                    <Label htmlFor="tokenExpiration">Access Token Expiration (minutes)</Label>
                    <Input
                      id="tokenExpiration"
                      type="number"
                      min="5"
                      max="1440"
                      defaultValue={securitySettings?.['Security.TokenExpirationMinutes'] || '15'}
                      onChange={(e) => {
                        const value = e.target.value;
                        if (value) {
                          updateMutation.mutate({
                            key: 'Security.TokenExpirationMinutes',
                            value,
                            category: CATEGORIES.Security,
                          });
                        }
                      }}
                      disabled={updateMutation.isPending}
                    />
                    <p className="text-sm text-muted-foreground">
                      Duration in minutes before access tokens expire (default: 15 minutes)
                    </p>
                  </div>

                  {/* Password Policy */}
                  <div className="space-y-4">
                    <Label className="text-base font-semibold">Password Policy</Label>
                    
                    <div className="space-y-2">
                      <Label htmlFor="minPasswordLength">Minimum Password Length</Label>
                      <Input
                        id="minPasswordLength"
                        type="number"
                        min="6"
                        max="128"
                        defaultValue={securitySettings?.['Security.PasswordMinLength'] || '8'}
                        onChange={(e) => {
                          const value = e.target.value;
                          if (value) {
                            updateMutation.mutate({
                              key: 'Security.PasswordMinLength',
                              value,
                              category: CATEGORIES.Security,
                            });
                          }
                        }}
                        disabled={updateMutation.isPending}
                      />
                    </div>

                    <div className="flex items-center space-x-2">
                      <Switch
                        id="requireUppercase"
                        defaultChecked={securitySettings?.['Security.PasswordRequireUppercase'] === 'true'}
                        onCheckedChange={(checked) => {
                          updateMutation.mutate({
                            key: 'Security.PasswordRequireUppercase',
                            value: checked.toString(),
                            category: CATEGORIES.Security,
                          });
                        }}
                        disabled={updateMutation.isPending}
                      />
                      <Label htmlFor="requireUppercase" className="cursor-pointer">
                        Require uppercase letter
                      </Label>
                    </div>

                    <div className="flex items-center space-x-2">
                      <Switch
                        id="requireLowercase"
                        defaultChecked={securitySettings?.['Security.PasswordRequireLowercase'] === 'true'}
                        onCheckedChange={(checked) => {
                          updateMutation.mutate({
                            key: 'Security.PasswordRequireLowercase',
                            value: checked.toString(),
                            category: CATEGORIES.Security,
                          });
                        }}
                        disabled={updateMutation.isPending}
                      />
                      <Label htmlFor="requireLowercase" className="cursor-pointer">
                        Require lowercase letter
                      </Label>
                    </div>

                    <div className="flex items-center space-x-2">
                      <Switch
                        id="requireNumber"
                        defaultChecked={securitySettings?.['Security.PasswordRequireNumber'] === 'true'}
                        onCheckedChange={(checked) => {
                          updateMutation.mutate({
                            key: 'Security.PasswordRequireNumber',
                            value: checked.toString(),
                            category: CATEGORIES.Security,
                          });
                        }}
                        disabled={updateMutation.isPending}
                      />
                      <Label htmlFor="requireNumber" className="cursor-pointer">
                        Require number
                      </Label>
                    </div>

                    <div className="flex items-center space-x-2">
                      <Switch
                        id="requireSpecialChar"
                        defaultChecked={securitySettings?.['Security.PasswordRequireSpecialChar'] === 'false'}
                        onCheckedChange={(checked) => {
                          updateMutation.mutate({
                            key: 'Security.PasswordRequireSpecialChar',
                            value: checked.toString(),
                            category: CATEGORIES.Security,
                          });
                        }}
                        disabled={updateMutation.isPending}
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
                      type="number"
                      min="3"
                      max="10"
                      defaultValue={securitySettings?.['Security.MaxLoginAttempts'] || '5'}
                      onChange={(e) => {
                        const value = e.target.value;
                        if (value) {
                          updateMutation.mutate({
                            key: 'Security.MaxLoginAttempts',
                            value,
                            category: CATEGORIES.Security,
                          });
                        }
                      }}
                      disabled={updateMutation.isPending}
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
                      type="number"
                      min="1"
                      max="168"
                      defaultValue={securitySettings?.['Security.SessionDurationHours'] || '24'}
                      onChange={(e) => {
                        const value = e.target.value;
                        if (value) {
                          updateMutation.mutate({
                            key: 'Security.SessionDurationHours',
                            value,
                            category: CATEGORIES.Security,
                          });
                        }
                      }}
                      disabled={updateMutation.isPending}
                    />
                    <p className="text-sm text-muted-foreground">
                      Duration in hours before user session expires
                    </p>
                  </div>

                  {/* 2FA Required */}
                  <div className="flex items-center space-x-2">
                    <Switch
                      id="require2FA"
                      defaultChecked={securitySettings?.['Security.Require2FA'] === 'true'}
                      onCheckedChange={(checked) => {
                        updateMutation.mutate({
                          key: 'Security.Require2FA',
                          value: checked.toString(),
                          category: CATEGORIES.Security,
                        });
                      }}
                      disabled={updateMutation.isPending}
                    />
                    <Label htmlFor="require2FA" className="cursor-pointer">
                      Require Two-Factor Authentication
                    </Label>
                  </div>
                </div>
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
                <div className="space-y-6">
                  {/* SMTP Configuration */}
                  <div className="space-y-4">
                    <Label className="text-base font-semibold">SMTP Configuration</Label>
                    
                    <div className="space-y-2">
                      <Label htmlFor="smtpHost">SMTP Host</Label>
                      <Input
                        id="smtpHost"
                        type="text"
                        placeholder="smtp.gmail.com"
                        defaultValue={emailSettings?.['Email.SmtpHost'] || ''}
                        onChange={(e) => {
                          const value = e.target.value;
                          if (value) {
                            updateMutation.mutate({
                              key: 'Email.SmtpHost',
                              value,
                              category: CATEGORIES.Email,
                            });
                          }
                        }}
                        disabled={updateMutation.isPending}
                      />
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                      <div className="space-y-2">
                        <Label htmlFor="smtpPort">SMTP Port</Label>
                        <Input
                          id="smtpPort"
                          type="number"
                          min="1"
                          max="65535"
                          placeholder="587"
                          defaultValue={emailSettings?.['Email.SmtpPort'] || '587'}
                          onChange={(e) => {
                            const value = e.target.value;
                            if (value) {
                              updateMutation.mutate({
                                key: 'Email.SmtpPort',
                                value,
                                category: CATEGORIES.Email,
                              });
                            }
                          }}
                          disabled={updateMutation.isPending}
                        />
                      </div>

                      <div className="space-y-2">
                        <Label htmlFor="useSsl">Use SSL/TLS</Label>
                        <div className="flex items-center space-x-2 pt-2">
                          <Switch
                            id="useSsl"
                            defaultChecked={emailSettings?.['Email.UseSsl'] !== 'false'}
                            onCheckedChange={(checked) => {
                              updateMutation.mutate({
                                key: 'Email.UseSsl',
                                value: checked.toString(),
                                category: CATEGORIES.Email,
                              });
                            }}
                            disabled={updateMutation.isPending}
                          />
                          <Label htmlFor="useSsl" className="cursor-pointer">
                            {emailSettings?.['Email.UseSsl'] !== 'false' ? 'Enabled' : 'Disabled'}
                          </Label>
                        </div>
                      </div>
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="smtpUsername">SMTP Username</Label>
                      <Input
                        id="smtpUsername"
                        type="text"
                        placeholder="your-email@gmail.com"
                        defaultValue={emailSettings?.['Email.SmtpUsername'] || ''}
                        onChange={(e) => {
                          const value = e.target.value;
                          if (value) {
                            updateMutation.mutate({
                              key: 'Email.SmtpUsername',
                              value,
                              category: CATEGORIES.Email,
                            });
                          }
                        }}
                        disabled={updateMutation.isPending}
                      />
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="smtpPassword">SMTP Password</Label>
                      <Input
                        id="smtpPassword"
                        type="password"
                        placeholder="Enter SMTP password"
                        defaultValue={emailSettings?.['Email.SmtpPassword'] || ''}
                        onChange={(e) => {
                          const value = e.target.value;
                          if (value) {
                            updateMutation.mutate({
                              key: 'Email.SmtpPassword',
                              value,
                              category: CATEGORIES.Email,
                            });
                          }
                        }}
                        disabled={updateMutation.isPending}
                      />
                      <p className="text-sm text-muted-foreground">
                        For Gmail, use an App Password instead of your regular password
                      </p>
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="fromEmail">From Email</Label>
                      <Input
                        id="fromEmail"
                        type="email"
                        placeholder="noreply@intellipm.com"
                        defaultValue={emailSettings?.['Email.FromEmail'] || ''}
                        onChange={(e) => {
                          const value = e.target.value;
                          if (value) {
                            updateMutation.mutate({
                              key: 'Email.FromEmail',
                              value,
                              category: CATEGORIES.Email,
                            });
                          }
                        }}
                        disabled={updateMutation.isPending}
                      />
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="fromName">From Name</Label>
                      <Input
                        id="fromName"
                        type="text"
                        placeholder="IntelliPM"
                        defaultValue={emailSettings?.['Email.FromName'] || 'IntelliPM'}
                        onChange={(e) => {
                          const value = e.target.value;
                          if (value) {
                            updateMutation.mutate({
                              key: 'Email.FromName',
                              value,
                              category: CATEGORIES.Email,
                            });
                          }
                        }}
                        disabled={updateMutation.isPending}
                      />
                    </div>
                  </div>

                  {/* Test Email */}
                  <div className="pt-4 border-t">
                    <div className="flex items-center justify-between">
                      <div>
                        <Label className="text-base font-semibold">Test Email Configuration</Label>
                        <p className="text-sm text-muted-foreground">
                          Send a test email to verify your SMTP settings
                        </p>
                      </div>
                      <div className="flex items-center gap-2">
                        <Input
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
                                showError('Failed to send test email');
                              }
                            } catch (error: any) {
                              showError('Error');
                            }
                          }}
                          disabled={updateMutation.isPending}
                        >
                          <Mail className="mr-2 h-4 w-4" />
                          Send Test Email
                        </Button>
                      </div>
                    </div>
                  </div>
                </div>
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
                  variant="outline"
                  className="mt-4"
                  onClick={() => window.location.href = '/admin/feature-flags'}
                >
                  Go to Feature Flags
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
