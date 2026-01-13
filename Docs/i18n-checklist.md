# i18n Implementation Checklist

**Version:** 1.0.0  
**Last Updated:** January 2025

Use this checklist to ensure proper i18n implementation when adding new features or components.

---

## Component Translation Checklist

When adding a new component that needs translation:

### Before Development
- [ ] Identify which namespace(s) the component will use
- [ ] Check if translation keys already exist
- [ ] Plan the key structure following naming conventions

### During Development
- [ ] Use `useTranslation` hook instead of hardcoded strings
- [ ] Use appropriate namespace (common, feature-specific, etc.)
- [ ] Add translation keys to English file first
- [ ] Use interpolation for dynamic values: `{{variable}}`
- [ ] Handle pluralization if needed
- [ ] Use `safeT` for fallback values when appropriate

### After Development
- [ ] Add translations to all supported languages
- [ ] Run `npm run i18n:check` to validate
- [ ] Test component with all languages
- [ ] Verify text doesn't overflow in different languages
- [ ] Check for missing translations (keys showing instead of text)

### Example Checklist for a New Component

```typescript
// ✅ Good: Using translations
import { useTranslation } from 'react-i18next';

function MyComponent() {
  const { t } = useTranslation('projects');
  
  return (
    <div>
      <h1>{t('title')}</h1>
      <p>{t('description')}</p>
      <button>{t('create.button')}</button>
    </div>
  );
}
```

**Translation Files:**
- [ ] `public/locales/en/projects.json` - Added keys
- [ ] `public/locales/fr/projects.json` - Added translations
- [ ] Keys follow naming conventions
- [ ] No empty values
- [ ] JSON is valid

---

## Page Translation Checklist

When adding a new page:

### Planning
- [ ] Identify all text that needs translation
- [ ] Group related translations by namespace
- [ ] Plan key structure (page.section.item)

### Implementation
- [ ] Replace all hardcoded strings with `t()` calls
- [ ] Use appropriate namespaces
- [ ] Handle form labels, placeholders, and validation messages
- [ ] Translate error messages
- [ ] Translate empty states
- [ ] Translate confirmation dialogs
- [ ] Translate tooltips and help text

### Translation Files
- [ ] All keys added to English file
- [ ] All keys translated to French
- [ ] Keys organized logically
- [ ] No duplicate keys
- [ ] Consistent naming

### Testing
- [ ] Page renders correctly in English
- [ ] Page renders correctly in French
- [ ] All text is translated (no keys showing)
- [ ] Dynamic content (interpolation) works
- [ ] Pluralization works (if applicable)
- [ ] Text doesn't overflow layout
- [ ] Form validation messages are translated

---

## Testing Checklist

### Manual Testing
- [ ] Change language using language toggle
- [ ] Navigate through all pages
- [ ] Check all components render correctly
- [ ] Verify no translation keys are visible (e.g., `common.save`)
- [ ] Test with different screen sizes
- [ ] Test in different browsers

### Automated Testing
- [ ] Run `npm run i18n:check`
- [ ] All checks pass (no missing keys, no empty values)
- [ ] No JSON syntax errors
- [ ] All namespaces validated

### Edge Cases
- [ ] Long translations (check for overflow)
- [ ] Short translations (check layout)
- [ ] Dynamic content with interpolation
- [ ] Pluralization (0, 1, 2, 5+ items)
- [ ] Special characters (accents, symbols)
- [ ] Empty states
- [ ] Error states

---

## Launch Checklist

Before deploying i18n changes:

### Validation
- [ ] Run `npm run i18n:check` - all checks pass
- [ ] No missing translation keys
- [ ] No empty values
- [ ] All JSON files are valid
- [ ] All namespaces have translations for all languages

### Testing
- [ ] Tested in development environment
- [ ] Tested language switching
- [ ] Tested all pages and components
- [ ] Tested date/number formatting
- [ ] Tested in multiple browsers
- [ ] Tested on mobile devices

### Documentation
- [ ] Updated component documentation (if needed)
- [ ] Updated translation guide (if new patterns)
- [ ] Updated main i18n documentation (if needed)

### Code Review
- [ ] Code follows i18n best practices
- [ ] Translation keys follow naming conventions
- [ ] No hardcoded strings
- [ ] Proper use of namespaces
- [ ] Proper use of interpolation

### Deployment
- [ ] Translation files included in build
- [ ] Language toggle works in production
- [ ] Backend language sync works (if applicable)
- [ ] No console errors related to i18n

---

## Adding a New Language Checklist

When adding support for a new language:

### Configuration
- [ ] Add language code to `supportedLngs` in `i18n/config.ts`
- [ ] Add language to `AVAILABLE_LANGUAGES` in `LanguageContext.tsx`
- [ ] Add language option to `LanguageToggle.tsx`
- [ ] Update date formatting in `dateFormat.ts`
- [ ] Update number formatting in `numberFormat.ts`

### Translation Files
- [ ] Create language directory: `public/locales/{lang}/`
- [ ] Create translation files for all namespaces
- [ ] Translate all keys from English reference
- [ ] Validate JSON structure
- [ ] Run `npm run i18n:check`

### Testing
- [ ] Test language switching
- [ ] Test all pages and components
- [ ] Test date formatting
- [ ] Test number formatting
- [ ] Test pluralization rules
- [ ] Test special characters

### Documentation
- [ ] Update README.md with new language
- [ ] Update main i18n documentation
- [ ] Add language-specific style guide (if needed)

---

## Quick Reference

### Common Namespaces
- `common` - Generic UI elements
- `auth` - Authentication
- `projects` - Project management
- `tasks` - Task management
- `admin` - Administration
- `errors` - Error messages
- `notifications` - Notifications

### Common Patterns
```typescript
// Basic usage
const { t } = useTranslation('common');
t('save')

// With namespace
const { t } = useTranslation('projects');
t('create.button')

// With interpolation
t('showing', { count: 5, total: 10 })

// Safe translation with fallback
const { safeT } = useTranslation();
safeT('common.newKey', 'Fallback Text')
```

### Validation Command
```bash
npm run i18n:check
```

### File Locations
- Translation files: `frontend/public/locales/{lang}/{ns}.json`
- Config: `frontend/src/i18n/config.ts`
- Context: `frontend/src/contexts/LanguageContext.tsx`
- Hook: `frontend/src/hooks/useTranslation.ts`

---

## Troubleshooting Quick Checks

If translations aren't working:

1. **Key not found?**
   - Check key exists in translation file
   - Verify namespace is correct
   - Check for typos

2. **Language not changing?**
   - Verify language code is in `supportedLngs`
   - Check `LanguageProvider` wraps component
   - Clear browser cache

3. **Missing translations?**
   - Run `npm run i18n:check`
   - Check file exists in `public/locales/{lang}/`
   - Verify JSON is valid

4. **Formatting issues?**
   - Check date/number formatting utilities
   - Verify locale is supported
   - Check language code is passed correctly

---

**Last Updated:** January 2025

