import { Languages } from 'lucide-react';
import { useLanguage } from '@/contexts/LanguageContext';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';

const LANGUAGE_OPTIONS = [
  { code: 'en', label: 'English', flag: '🇬🇧' },
  { code: 'fr', label: 'Français', flag: '🇫🇷' },
] as const;

export function LanguageToggle() {
  const { language, changeLanguage, isLoading } = useLanguage();

  const currentLanguage = LANGUAGE_OPTIONS.find((lang) => lang.code === language) || LANGUAGE_OPTIONS[0];

  const handleLanguageChange = async (langCode: string) => {
    if (langCode !== language && !isLoading) {
      await changeLanguage(langCode);
    }
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          variant="ghost"
          size="icon"
          className="h-9 w-9"
          disabled={isLoading}
          aria-label={`Current language: ${currentLanguage.label}. Change language`}
        >
          <Languages className="h-4 w-4" />
          <span className="sr-only">Change language</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-40">
        <DropdownMenuRadioGroup value={language} onValueChange={handleLanguageChange}>
          {LANGUAGE_OPTIONS.map((lang) => (
            <DropdownMenuRadioItem
              key={lang.code}
              value={lang.code}
              className="cursor-pointer"
              disabled={isLoading}
            >
              <span className="mr-2">{lang.flag}</span>
              <span className={language === lang.code ? 'font-semibold' : ''}>{lang.label}</span>
            </DropdownMenuRadioItem>
          ))}
        </DropdownMenuRadioGroup>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

