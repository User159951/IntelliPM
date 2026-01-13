import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '@/contexts/AuthContext';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { Eye, EyeOff, Loader2 } from 'lucide-react';
import { showError } from '@/lib/sweetalert';

export default function LoginForm() {
  const { t } = useTranslation('auth');
  const { login } = useAuth();
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [formData, setFormData] = useState({
    username: '',
    password: '',
    rememberMe: false,
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);

    try {
      const user = await login({
        username: formData.username,
        password: formData.password,
      });
      // Redirect based on user role
      if (user.globalRole === 'Admin') {
        navigate('/admin/dashboard');
      } else {
        navigate('/dashboard');
      }
    } catch (error) {
      showError(
        t('login.errors.loginFailed'),
        error instanceof Error ? error.message : t('login.errors.invalidCredentials')
      );
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="space-y-2 opacity-0 animate-fade-in-up animation-delay-100" style={{ animationFillMode: 'forwards' }}>
        <Label htmlFor="username" className="text-sm font-medium text-foreground">
          {t('login.usernameLabel')}
        </Label>
        <Input
          id="username"
          name="username"
          type="text"
          autoComplete="username"
          placeholder={t('login.usernamePlaceholder')}
          value={formData.username}
          onChange={(e) => setFormData({ ...formData, username: e.target.value })}
          className="h-12 bg-secondary/50 border-border transition-all duration-200 focus:bg-card focus:shadow-input-focus focus:border-primary"
          required
        />
      </div>

      <div className="space-y-2 opacity-0 animate-fade-in-up animation-delay-200" style={{ animationFillMode: 'forwards' }}>
        <Label htmlFor="password" className="text-sm font-medium text-foreground">
          {t('login.passwordLabel')}
        </Label>
        <div className="relative">
          <Input
            id="password"
            name="password"
            type={showPassword ? 'text' : 'password'}
            autoComplete="current-password"
            placeholder={t('login.passwordPlaceholder')}
            value={formData.password}
            onChange={(e) => setFormData({ ...formData, password: e.target.value })}
            className="h-12 bg-secondary/50 border-border pr-12 transition-all duration-200 focus:bg-card focus:shadow-input-focus focus:border-primary"
            required
          />
          <button
            type="button"
            onClick={() => setShowPassword(!showPassword)}
            className="absolute right-4 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
            tabIndex={-1}
            aria-label={showPassword ? t('login.hidePassword') : t('login.showPassword')}
          >
            {showPassword ? (
              <EyeOff className="h-5 w-5" />
            ) : (
              <Eye className="h-5 w-5" />
            )}
          </button>
        </div>
      </div>

      <div className="flex items-center justify-between opacity-0 animate-fade-in-up animation-delay-300" style={{ animationFillMode: 'forwards' }}>
        <div className="flex items-center space-x-2">
          <Checkbox
            id="remember"
            checked={formData.rememberMe}
            onCheckedChange={(checked) =>
              setFormData({ ...formData, rememberMe: checked as boolean })
            }
            className="border-border data-[state=checked]:bg-primary data-[state=checked]:border-primary"
          />
          <Label
            htmlFor="remember"
            className="text-sm font-normal text-muted-foreground cursor-pointer"
          >
            {t('login.rememberMe')}
          </Label>
        </div>
        <Link
          to="/forgot-password"
          className="text-sm font-medium text-primary hover:text-primary/80 transition-colors hover:underline"
        >
          {t('login.forgotPassword')}
        </Link>
      </div>

      <div className="opacity-0 animate-fade-in-up animation-delay-400" style={{ animationFillMode: 'forwards' }}>
        <Button
          type="submit"
          className="w-full h-12 text-base font-semibold gradient-primary hover:opacity-90 transition-all duration-200 shadow-lg shadow-primary/25 hover:shadow-xl hover:shadow-primary/30"
          disabled={isLoading}
        >
          {isLoading ? (
            <>
              <Loader2 className="mr-2 h-5 w-5 animate-spin" />
              {t('login.loading')}
            </>
          ) : (
            t('login.loginButton')
          )}
        </Button>
      </div>
    </form>
  );
}

