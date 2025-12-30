import { cn } from '@/lib/utils';

interface PasswordStrengthIndicatorProps {
  password: string;
}

function calculatePasswordStrength(password: string): number {
  if (!password) return 0;

  let strength = 0;
  if (password.length >= 8) strength++;
  if (password.length >= 12) strength++;
  if (/[a-z]/.test(password) && /[A-Z]/.test(password)) strength++;
  if (/[0-9]/.test(password)) strength++;
  if (/[^a-zA-Z0-9]/.test(password)) strength++;

  return Math.min(strength, 4);
}

export function PasswordStrengthIndicator({ password }: PasswordStrengthIndicatorProps) {
  // Calculate strength (0-4)
  const strength = calculatePasswordStrength(password);

  const strengthLabels = ['', 'Faible', 'Moyen', 'Bon', 'Excellent'];
  const strengthColors = ['', 'bg-red-500', 'bg-orange-500', 'bg-yellow-500', 'bg-green-500'];

  return (
    <div className="mt-2">
      <div className="flex gap-1 mb-1">
        {[1, 2, 3, 4].map((level) => (
          <div
            key={level}
            className={cn(
              'h-1 flex-1 rounded',
              level <= strength ? strengthColors[strength] : 'bg-gray-200'
            )}
          />
        ))}
      </div>
      {strength > 0 && (
        <p className="text-xs text-muted-foreground">
          {strengthLabels[strength]}
        </p>
      )}
    </div>
  );
}

