# Translation Contribution Guide

**Version:** 1.0.0  
**Last Updated:** January 2025

This guide is for contributors who want to add or improve translations for IntelliPM.

---

## Table of Contents

1. [Getting Started](#getting-started)
2. [Key Naming Conventions](#key-naming-conventions)
3. [Translation Workflow](#translation-workflow)
4. [French Style Guide](#french-style-guide)
5. [Testing Translations](#testing-translations)
6. [Common Pitfalls](#common-pitfalls)
7. [Submitting Translations](#submitting-translations)

---

## 1. Getting Started

### 1.1 Prerequisites

- Basic understanding of JSON
- Familiarity with the language you're translating to
- Access to the codebase (fork/clone the repository)

### 1.2 Translation File Locations

Translation files are located in:
```
frontend/public/locales/{language}/{namespace}.json
```

Where:
- `{language}` is the language code (e.g., `en`, `fr`)
- `{namespace}` is the feature namespace (e.g., `common`, `projects`, `tasks`)

### 1.3 Supported Languages

Currently supported:
- **English (en)** - Reference language
- **French (fr)** - In progress

---

## 2. Key Naming Conventions

### 2.1 Structure

Keys follow a hierarchical structure:
```
{namespace}.{section}.{item}
```

Examples:
- `common.save` - Common save button
- `projects.create.button` - Project create button
- `tasks.form.name` - Task form name field

### 2.2 Naming Patterns

#### Buttons
- Use `{action}Button` or `{action}.button`
  - ✅ `createButton` or `create.button`
  - ✅ `saveButton` or `save.button`
  - ❌ `btn1`, `button_create`

#### Form Fields
- Use `{field}` or `form.{field}`
  - ✅ `name` or `form.name`
  - ✅ `email` or `form.email`
  - ✅ `namePlaceholder` or `form.namePlaceholder`

#### Messages
- Use `{type}Message` or `{type}.message`
  - ✅ `successMessage` or `success.message`
  - ✅ `errorMessage` or `error.message`

#### Titles and Labels
- Use `{item}Title` or `{item}.title`
- Use `{item}Label` or `{item}.label`
  - ✅ `pageTitle` or `page.title`
  - ✅ `sectionLabel` or `section.label`

### 2.3 Common Suffixes

- `Placeholder` - Input placeholders
- `Required` - Required field indicators
- `Tooltip` - Tooltip text
- `AriaLabel` - Accessibility labels
- `Description` - Descriptions/help text
- `Empty` - Empty state messages

---

## 3. Translation Workflow

### 3.1 Step 1: Identify Missing Translations

Run the validation script to find missing keys:
```bash
npm run i18n:check
```

This will show:
- Missing keys in each language
- Empty values
- JSON structure errors

### 3.2 Step 2: Find the Correct Namespace

Determine which namespace the translation belongs to:
- **common** - Generic UI elements (buttons, labels, messages)
- **auth** - Authentication and login
- **projects** - Project management
- **tasks** - Task management
- **admin** - Administration
- **errors** - Error messages
- **notifications** - Notification messages
- And more...

### 3.3 Step 3: Add Translation

1. Open the translation file:
   ```
   frontend/public/locales/{language}/{namespace}.json
   ```

2. Find the corresponding key from the English version

3. Add the translation maintaining the same structure:

   **English (`en/common.json`):**
   ```json
   {
     "welcome": "Welcome",
     "form": {
       "name": "Name",
       "email": "Email"
     }
   }
   ```

   **French (`fr/common.json`):**
   ```json
   {
     "welcome": "Bienvenue",
     "form": {
       "name": "Nom",
       "email": "Courriel"
     }
   }
   ```

### 3.4 Step 4: Validate

Run the validation script again:
```bash
npm run i18n:check
```

Ensure:
- ✅ No missing keys
- ✅ No empty values
- ✅ Valid JSON structure

### 3.5 Step 5: Test

1. Start the development server:
   ```bash
   npm run dev
   ```

2. Change language using the language toggle
3. Navigate to the page/component using the translation
4. Verify the translation appears correctly

---

## 4. French Style Guide

### 4.1 Formality

IntelliPM uses **formal French** (vous form) for user-facing text:
- ✅ "Vous pouvez créer un projet"
- ❌ "Tu peux créer un projet"

### 4.2 Capitalization

- **Titles**: Capitalize first letter only
  - ✅ "Gérer les projets"
  - ❌ "GÉRER LES PROJETS"
- **Buttons**: Capitalize first letter only
  - ✅ "Créer un projet"
  - ❌ "CRÉER UN PROJET"

### 4.3 Punctuation

- Use proper French punctuation:
  - Space before colons: "Projet :"
  - Space before exclamation marks: "Attention !"
  - Space before question marks: "Êtes-vous sûr ?"

### 4.4 Technical Terms

- Keep technical terms in English when appropriate:
  - ✅ "Sprint", "Backlog", "Scrum Master"
  - ❌ "Sprint", "Arriéré", "Maître Scrum" (unless widely accepted)

### 4.5 Common Translations

| English | French |
|---------|--------|
| Create | Créer |
| Edit | Modifier |
| Delete | Supprimer |
| Save | Enregistrer |
| Cancel | Annuler |
| Submit | Soumettre |
| Search | Rechercher |
| Filter | Filtrer |
| Sort | Trier |
| Loading... | Chargement... |
| No items found | Aucun élément trouvé |
| Are you sure? | Êtes-vous sûr ? |
| This field is required | Ce champ est requis |
| Invalid value | Valeur invalide |

### 4.6 Pluralization

French has more complex pluralization rules:

```json
{
  "task": "{{count}} tâche",
  "task_plural": "{{count}} tâches",
  "task_zero": "Aucune tâche"
}
```

### 4.7 Gender Agreement

French nouns have gender. Ensure adjectives agree:
- ✅ "Projet créé" (masculine)
- ✅ "Tâche créée" (feminine)

---

## 5. Testing Translations

### 5.1 Manual Testing

1. **Change Language**:
   - Click the language toggle in the header
   - Select the target language

2. **Navigate Through App**:
   - Check all pages and components
   - Verify translations appear correctly
   - Check for missing translations (keys showing instead of text)

3. **Test Edge Cases**:
   - Long text (check for overflow)
   - Short text (check for layout)
   - Dynamic content (interpolation)
   - Pluralization

### 5.2 Automated Testing

Run the validation script:
```bash
npm run i18n:check
```

This checks:
- All keys exist in all languages
- No empty values
- Valid JSON structure

### 5.3 Browser Testing

Test in different browsers:
- Chrome/Edge
- Firefox
- Safari

Check for:
- Font rendering
- Text overflow
- Layout issues

---

## 6. Common Pitfalls

### 6.1 Missing Keys

**Problem:** Translation key shows as `common.newKey` instead of translated text

**Solution:**
1. Check the key exists in the translation file
2. Verify the namespace is correct
3. Ensure JSON syntax is valid
4. Run `npm run i18n:check`

### 6.2 Incorrect Structure

**Problem:** Translation not found due to wrong nesting

**Solution:**
- Match the exact structure from the English file
- Use the same nesting levels
- Verify JSON syntax (commas, brackets)

### 6.3 Empty Values

**Problem:** Empty string `""` in translation file

**Solution:**
- Never use empty strings
- Use a placeholder if translation is pending: `"[TODO: Translate]"`
- Or use the English text temporarily

### 6.4 Interpolation Issues

**Problem:** Variables not replaced in translations

**Solution:**
- Use `{{variable}}` syntax (double curly braces)
- Ensure variable names match exactly
- Check for typos in variable names

Example:
```json
{
  "welcome": "Welcome, {{name}}!"
}
```

### 6.5 Special Characters

**Problem:** Special characters not displaying correctly

**Solution:**
- Use UTF-8 encoding
- Escape special characters if needed
- Test with accented characters (é, è, à, etc.)

### 6.6 Pluralization

**Problem:** Pluralization not working correctly

**Solution:**
- Use `_plural` suffix for plural forms
- Use `_zero` for zero count (if needed)
- Test with different counts (0, 1, 2, 5, etc.)

---

## 7. Submitting Translations

### 7.1 Before Submitting

- [ ] Run `npm run i18n:check` - all checks pass
- [ ] Test translations in the browser
- [ ] Verify no console errors
- [ ] Check for typos and grammar
- [ ] Ensure consistency with existing translations

### 7.2 Commit Message

Use a clear commit message:
```
feat(i18n): Add French translations for projects namespace

- Translate all project-related keys
- Add missing form labels
- Update empty state messages
```

### 7.3 Pull Request

When creating a PR:
1. **Title**: Clear description of what was translated
2. **Description**: 
   - List of namespaces/files changed
   - Number of keys translated
   - Any notes about style choices
3. **Screenshots**: Show translations in action (optional but helpful)

### 7.4 Review Process

Translations will be reviewed for:
- ✅ Accuracy
- ✅ Consistency
- ✅ Style compliance
- ✅ Completeness

---

## Additional Resources

- [Main i18n Documentation](./i18n.md)
- [i18n Checklist](./i18n-checklist.md)
- [French Language Resources](https://www.lepointdufle.net/)
- [react-i18next Documentation](https://react.i18next.com/)

---

## Questions?

If you have questions about translations:
1. Check the [main i18n documentation](./i18n.md)
2. Review existing translations for patterns
3. Open an issue or discussion on GitHub

---

**Thank you for contributing to IntelliPM translations!** 🌍

