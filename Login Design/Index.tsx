import LoginForm from '@/components/LoginForm';
import Logo from '@/components/Logo';
import GeometricShapes from '@/components/GeometricShapes';

const Index = () => {
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
              Gérez vos projets avec intelligence.
              <br />
              <span className="text-white font-medium">Simplifiez. Automatisez. Réussissez.</span>
            </p>
          </div>

          {/* Feature highlights */}
          <div className="mt-16 grid grid-cols-3 gap-8 opacity-0 animate-fade-in-up animation-delay-300" style={{ animationFillMode: 'forwards' }}>
            <div className="text-center">
              <div className="text-3xl font-bold text-white">50+</div>
              <div className="text-sm text-white/60 mt-1">Entreprises</div>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-white">1000+</div>
              <div className="text-sm text-white/60 mt-1">Projets gérés</div>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-white">99%</div>
              <div className="text-sm text-white/60 mt-1">Satisfaction</div>
            </div>
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
              <h2 className="text-2xl font-bold text-foreground">Bienvenue</h2>
              <p className="text-muted-foreground mt-2">
                Accédez à votre espace IntelliPM
              </p>
            </div>

            <LoginForm />
          </div>

          {/* Footer */}
          <div className="mt-8 text-center opacity-0 animate-fade-in animation-delay-400" style={{ animationFillMode: 'forwards' }}>
            <p className="text-sm text-muted-foreground">
              © 2025 IntelliPM. Tous droits réservés.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Index;
