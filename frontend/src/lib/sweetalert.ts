import Swal from 'sweetalert2';
import withReactContent from 'sweetalert2-react-content';
import i18next from 'i18next';

export const MySwal = withReactContent(Swal);

// Success Alert
export const showSuccess = (title: string, text?: string) => {
  // Translate title and text if they are translation keys, otherwise use as-is
  const translatedTitle = title.startsWith('notifications.') || title.startsWith('errors.') 
    ? i18next.t(title, { defaultValue: title })
    : title;
  const translatedText = text && (text.startsWith('notifications.') || text.startsWith('errors.'))
    ? i18next.t(text, { defaultValue: text })
    : text;

  return MySwal.fire({
    icon: 'success',
    title: translatedTitle,
    text: translatedText,
    confirmButtonText: i18next.t('buttons.confirm', { ns: 'common', defaultValue: 'OK' }),
    confirmButtonColor: '#10b981',
  });
};

// Error Alert
export const showError = (title: string, text?: string) => {
  // Translate title and text if they are translation keys, otherwise use as-is
  const translatedTitle = title.startsWith('notifications.') || title.startsWith('errors.')
    ? i18next.t(title, { defaultValue: title })
    : title;
  const translatedText = text && (text.startsWith('notifications.') || text.startsWith('errors.'))
    ? i18next.t(text, { defaultValue: text })
    : text;

  return MySwal.fire({
    icon: 'error',
    title: translatedTitle,
    text: translatedText,
    confirmButtonText: i18next.t('buttons.confirm', { ns: 'common', defaultValue: 'OK' }),
    confirmButtonColor: '#ef4444',
  });
};

// Warning Alert
export const showWarning = (title: string, text?: string) => {
  // Translate title and text if they are translation keys, otherwise use as-is
  const translatedTitle = title.startsWith('notifications.') || title.startsWith('errors.')
    ? i18next.t(title, { defaultValue: title })
    : title;
  const translatedText = text && (text.startsWith('notifications.') || text.startsWith('errors.'))
    ? i18next.t(text, { defaultValue: text })
    : text;

  return MySwal.fire({
    icon: 'warning',
    title: translatedTitle,
    text: translatedText,
    confirmButtonText: i18next.t('buttons.confirm', { ns: 'common', defaultValue: 'OK' }),
    confirmButtonColor: '#f59e0b',
  });
};

// Info Alert
export const showInfo = (title: string, text?: string) => {
  // Translate title and text if they are translation keys, otherwise use as-is
  const translatedTitle = title.startsWith('notifications.') || title.startsWith('errors.')
    ? i18next.t(title, { defaultValue: title })
    : title;
  const translatedText = text && (text.startsWith('notifications.') || text.startsWith('errors.'))
    ? i18next.t(text, { defaultValue: text })
    : text;

  return MySwal.fire({
    icon: 'info',
    title: translatedTitle,
    text: translatedText,
    confirmButtonText: i18next.t('buttons.confirm', { ns: 'common', defaultValue: 'OK' }),
    confirmButtonColor: '#3b82f6',
  });
};

// Confirmation Dialog (replaces window.confirm)
export const showConfirm = async (
  title: string,
  text?: string,
  confirmText?: string,
  cancelText?: string
): Promise<boolean> => {
  // Translate title and text if they are translation keys, otherwise use as-is
  const translatedTitle = title.startsWith('notifications.') || title.startsWith('errors.')
    ? i18next.t(title, { defaultValue: title })
    : title;
  const translatedText = text && (text.startsWith('notifications.') || text.startsWith('errors.'))
    ? i18next.t(text, { defaultValue: text })
    : text;

  const result = await MySwal.fire({
    icon: 'warning',
    title: translatedTitle,
    text: translatedText,
    showCancelButton: true,
    confirmButtonText: confirmText 
      ? (confirmText.startsWith('notifications.') || confirmText.startsWith('errors.') || confirmText.startsWith('common.')
          ? i18next.t(confirmText, { defaultValue: confirmText })
          : confirmText)
      : i18next.t('buttons.confirm', { ns: 'common', defaultValue: 'Confirm' }),
    cancelButtonText: cancelText
      ? (cancelText.startsWith('notifications.') || cancelText.startsWith('errors.') || cancelText.startsWith('common.')
          ? i18next.t(cancelText, { defaultValue: cancelText })
          : cancelText)
      : i18next.t('common.buttons.cancel', { defaultValue: 'Cancel' }),
    confirmButtonColor: '#3b82f6',
    cancelButtonColor: '#6b7280',
  });
  return result.isConfirmed;
};

// Input Prompt (replaces window.prompt)
export const showPrompt = async (
  title: string,
  inputPlaceholder?: string,
  inputType: 'text' | 'email' | 'password' = 'text'
): Promise<string | null> => {
  // Translate title if it's a translation key, otherwise use as-is
  const translatedTitle = title.startsWith('notifications.') || title.startsWith('errors.')
    ? i18next.t(title, { defaultValue: title })
    : title;
  const translatedPlaceholder = inputPlaceholder && (inputPlaceholder.startsWith('notifications.') || inputPlaceholder.startsWith('errors.'))
    ? i18next.t(inputPlaceholder, { defaultValue: inputPlaceholder })
    : inputPlaceholder;

  const result = await MySwal.fire({
    title: translatedTitle,
    input: inputType,
    inputPlaceholder: translatedPlaceholder,
    showCancelButton: true,
    confirmButtonText: i18next.t('buttons.submit', { ns: 'common', defaultValue: 'Submit' }),
    cancelButtonText: i18next.t('buttons.cancel', { ns: 'common', defaultValue: 'Cancel' }),
    confirmButtonColor: '#3b82f6',
    inputValidator: (value) => {
      if (!value) {
        return i18next.t('errors.validation.required', { defaultValue: 'This field is required!' });
      }
      return null;
    },
  });
  return result.isConfirmed ? result.value : null;
};

// Toast Notification (replaces sonner/toast)
export const showToast = (
  title: string,
  icon: 'success' | 'error' | 'warning' | 'info' = 'success',
  duration = 3000
) => {
  // Translate title if it's a translation key, otherwise use as-is
  const translatedTitle = title.startsWith('notifications.') || title.startsWith('errors.') || title.startsWith('common.')
    ? i18next.t(title, { defaultValue: title })
    : title;

  return MySwal.fire({
    toast: true,
    position: 'bottom-end',
    icon,
    title: translatedTitle,
    showConfirmButton: false,
    timer: duration,
    timerProgressBar: true,
  });
};

// Loading Alert
export const showLoading = (title?: string) => {
  const defaultTitle = i18next.t('notifications.info.loading', { defaultValue: 'Loading...' });
  const translatedTitle = title
    ? (title.startsWith('notifications.') || title.startsWith('errors.')
        ? i18next.t(title, { defaultValue: title })
        : title)
    : defaultTitle;

  MySwal.fire({
    title: translatedTitle,
    allowOutsideClick: false,
    didOpen: () => {
      MySwal.showLoading();
    },
  });
};

// Close Alert
export const closeAlert = () => {
  MySwal.close();
};

