import { Zap } from 'lucide-react';

interface LogoProps {
  variant?: 'light' | 'dark';
  size?: 'sm' | 'md' | 'lg';
}

export default function Logo({ variant = 'dark', size = 'md' }: LogoProps) {
  const sizes = {
    sm: { icon: 'h-8 w-8', iconInner: 'h-4 w-4', text: 'text-lg', tagline: 'text-xs' },
    md: { icon: 'h-12 w-12', iconInner: 'h-6 w-6', text: 'text-2xl', tagline: 'text-sm' },
    lg: { icon: 'h-16 w-16', iconInner: 'h-8 w-8', text: 'text-3xl', tagline: 'text-base' },
  };

  const colors = {
    light: {
      icon: 'bg-white/20 backdrop-blur-sm',
      iconInner: 'text-white',
      text: 'text-white',
      tagline: 'text-white/70',
    },
    dark: {
      icon: 'gradient-primary shadow-lg shadow-primary/25',
      iconInner: 'text-white',
      text: 'text-foreground',
      tagline: 'text-muted-foreground',
    },
  };

  const s = sizes[size];
  const c = colors[variant];

  return (
    <div className="flex flex-col items-center space-y-3">
      <div className={`${s.icon} rounded-xl flex items-center justify-center ${c.icon}`}>
        <Zap className={`${s.iconInner} ${c.iconInner}`} />
      </div>
      <div className="text-center">
        <h1 className={`${s.text} font-bold ${c.text}`}>IntelliPM</h1>
        <p className={`${s.tagline} ${c.tagline} mt-1`}>Intelligent Project Management</p>
      </div>
    </div>
  );
}
