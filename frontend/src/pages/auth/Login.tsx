import { Navigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '@/contexts/AuthContext';
import LoginForm from '@/components/auth/LoginForm';
import Logo from '@/components/auth/Logo';
import GeometricShapes from '@/components/auth/GeometricShapes';

export default function Login() {
  const { t } = useTranslation('auth');
  const { isAuthenticated, user } = useAuth();
  
  if (isAuthenticated && user) {
    // Redirect based on user role if already authenticated
    if (user.globalRole === 'Admin') {
      return <Navigate to="/admin/dashboard" replace />;
    }
    return <Navigate to="/dashboard" replace />;
  }

  return (
    <div className="min-h-screen flex">
      {/* Left Panel - Visual Side */}
      <div className="hidden lg:flex lg:w-1/2 xl:w-[55%] gradient-dark relative overflow-hidden">
        <GeometricShapes />
        
        {/* Content */}
        <div className="relative z-10 flex flex-col items-center justify-center w-full p-12">
          <div className="animate-fade-in">
            <Logo variant="light" size="lg" />
          </div>
          
          {/* Tagline */}
          <div className="mt-12 text-center max-w-md animate-fade-in-up animation-delay-200" style={{ animationFillMode: 'forwards' }}>
            <p className="text-white/80 text-lg font-light leading-relaxed">
              {t('login.tagline')}
              <br />
              <span className="text-white font-medium">{t('login.taglineBold')}</span>
            </p>
          </div>
        </div>

        {/* Bottom gradient fade */}
        <div className="absolute bottom-0 left-0 right-0 h-32 bg-gradient-to-t from-black/20 to-transparent" />
      </div>

      {/* Right Panel - Login Form */}
      <div className="w-full lg:w-1/2 xl:w-[45%] flex items-center justify-center p-6 sm:p-8 lg:p-12 bg-background">
        <div className="w-full max-w-md">
          {/* Mobile Logo */}
          <div className="lg:hidden mb-10 animate-fade-in">
            <Logo variant="dark" size="md" />
          </div>

          {/* Login Card */}
          <div className="bg-card rounded-2xl p-8 sm:p-10 shadow-card border border-border/50 animate-slide-in-right">
            <div className="mb-8">
              <h2 className="text-2xl font-bold text-foreground">{t('login.title')}</h2>
              <p className="text-muted-foreground mt-2">
                {t('login.subtitle')}
              </p>
            </div>

            <LoginForm />
          </div>

          {/* Footer */}
          <div className="mt-8 text-center opacity-0 animate-fade-in animation-delay-400" style={{ animationFillMode: 'forwards' }}>
            <p className="text-sm text-muted-foreground">
              {t('login.footer')}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
