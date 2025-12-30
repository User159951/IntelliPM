import { useMemo } from 'react';
import { cn } from '@/lib/utils';

export type PasswordStrength = 'weak' | 'medium' | 'strong' | 'empty';

interface PasswordStrengthProps {
  password: string;
  className?: string;
}

export function PasswordStrength({ password, className }: PasswordStrengthProps) {
  const strength = useMemo<PasswordStrength>(() => {
    if (!password) return 'empty';

    const hasMinLength = password.length >= 8;
    const hasUppercase = /[A-Z]/.test(password);
    const hasLowercase = /[a-z]/.test(password);
    const hasNumber = /[0-9]/.test(password);
    const hasSpecialChar = /[^a-zA-Z0-9]/.test(password);

    const criteriaMet = [
      hasMinLength,
      hasUppercase,
      hasLowercase,
      hasNumber,
      hasSpecialChar,
    ].filter(Boolean).length;

    if (!hasMinLength) {
      return 'weak';
    }

    if (criteriaMet === 5) {
      return 'strong';
    }

    if (criteriaMet >= 3) {
      return 'medium';
    }

    return 'weak';
  }, [password]);

  const strengthConfig = {
    empty: {
      label: '',
      color: 'bg-muted',
      textColor: 'text-muted-foreground',
      width: 'w-0',
    },
    weak: {
      label: 'Weak',
      color: 'bg-red-500',
      textColor: 'text-red-500',
      width: 'w-1/3',
    },
    medium: {
      label: 'Medium',
      color: 'bg-orange-500',
      textColor: 'text-orange-500',
      width: 'w-2/3',
    },
    strong: {
      label: 'Strong',
      color: 'bg-green-500',
      textColor: 'text-green-500',
      width: 'w-full',
    },
  };

  const config = strengthConfig[strength];

  if (strength === 'empty') {
    return null;
  }

  return (
    <div className={cn('space-y-1.5', className)}>
      <div className="flex items-center justify-between text-xs">
        <span className="text-muted-foreground">Password strength</span>
        <span className={cn('font-medium', config.textColor)}>{config.label}</span>
      </div>
      <div className="h-1.5 w-full overflow-hidden rounded-full bg-muted">
        <div
          className={cn(
            'h-full transition-all duration-300 ease-in-out',
            config.color,
            config.width
          )}
        />
      </div>
    </div>
  );
}
