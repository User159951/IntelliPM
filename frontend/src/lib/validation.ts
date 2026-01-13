/**
 * Validation helper utilities for Zod schemas with i18n support
 */
import i18next from 'i18next';

/**
 * Get translated validation error messages
 */
export const getValidationMessage = {
  required: () => i18next.t('errors.validation.required', { defaultValue: 'This field is required' }),
  minLength: (min: number) => i18next.t('errors.validation.minLength', { min, defaultValue: `Must be at least ${min} characters` }),
  maxLength: (max: number) => i18next.t('errors.validation.maxLength', { max, defaultValue: `Must not exceed ${max} characters` }),
  email: () => i18next.t('errors.validation.email', { defaultValue: 'Invalid email address' }),
  nameRequired: () => i18next.t('errors.validation.nameRequired', { defaultValue: 'Name is required' }),
  nameMaxLength: (max: number) => i18next.t('errors.validation.nameMaxLength', { max, defaultValue: `Name cannot exceed ${max} characters` }),
  titleRequired: () => i18next.t('errors.validation.titleRequired', { defaultValue: 'Title is required' }),
  titleMaxLength: (max: number) => i18next.t('errors.validation.titleMaxLength', { max, defaultValue: `Title must be ${max} characters or less` }),
  descriptionMaxLength: (max: number) => i18next.t('errors.validation.descriptionMaxLength', { max, defaultValue: `Description cannot exceed ${max} characters` }),
  versionRequired: () => i18next.t('errors.validation.versionRequired', { defaultValue: 'Version is required' }),
  semanticVersion: () => i18next.t('errors.validation.semanticVersion', { defaultValue: 'Must be semantic version (e.g., 2.1.0)' }),
  dueDateRequired: () => i18next.t('errors.validation.dueDateRequired', { defaultValue: 'Due date is required' }),
  dueDateInvalid: () => i18next.t('errors.validation.dueDateInvalid', { defaultValue: 'Due date must be today or in the future' }),
  plannedDateRequired: () => i18next.t('errors.validation.plannedDateRequired', { defaultValue: 'Planned date is required' }),
  tagNameMaxLength: (max: number) => i18next.t('errors.validation.tagNameMaxLength', { max, defaultValue: `Tag name cannot exceed ${max} characters` }),
  futureDate: () => i18next.t('errors.validation.futureDate', { defaultValue: 'Date must be in the future' }),
};

