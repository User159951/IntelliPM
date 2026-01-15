import { Languages } from 'lucide-react';
import { useLanguage } from '@/contexts/LanguageContext';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';

// Flag mapping for languages
const LANGUAGE_FLAGS: Record<string, string> = {
  en: '🇬🇧',
  fr: '🇫🇷',
  ar: '🇸🇦',
};

export function LanguageToggle() {
  const { language, changeLanguage, isLoading, availableLanguages } = useLanguage();

  const currentLanguage = availableLanguages.find((lang) => lang.code === language) || availableLanguages[0];

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
          {availableLanguages.map((lang) => (
            <DropdownMenuRadioItem
              key={lang.code}
              value={lang.code}
              className="cursor-pointer"
              disabled={isLoading}
            >
              <span className="mr-2">{LANGUAGE_FLAGS[lang.code] || '🌐'}</span>
              <span className={language === lang.code ? 'font-semibold' : ''}>{lang.label}</span>
            </DropdownMenuRadioItem>
          ))}
        </DropdownMenuRadioGroup>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

