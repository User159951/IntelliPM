import Swal from 'sweetalert2';
import withReactContent from 'sweetalert2-react-content';

export const MySwal = withReactContent(Swal);

// Success Alert
export const showSuccess = (title: string, text?: string) => {
  return MySwal.fire({
    icon: 'success',
    title,
    text,
    confirmButtonText: 'OK',
    confirmButtonColor: '#10b981',
  });
};

// Error Alert
export const showError = (title: string, text?: string) => {
  return MySwal.fire({
    icon: 'error',
    title,
    text,
    confirmButtonText: 'OK',
    confirmButtonColor: '#ef4444',
  });
};

// Warning Alert
export const showWarning = (title: string, text?: string) => {
  return MySwal.fire({
    icon: 'warning',
    title,
    text,
    confirmButtonText: 'OK',
    confirmButtonColor: '#f59e0b',
  });
};

// Info Alert
export const showInfo = (title: string, text?: string) => {
  return MySwal.fire({
    icon: 'info',
    title,
    text,
    confirmButtonText: 'OK',
    confirmButtonColor: '#3b82f6',
  });
};

// Confirmation Dialog (replaces window.confirm)
export const showConfirm = async (
  title: string,
  text?: string,
  confirmText = 'Yes',
  cancelText = 'Cancel'
): Promise<boolean> => {
  const result = await MySwal.fire({
    icon: 'warning',
    title,
    text,
    showCancelButton: true,
    confirmButtonText: confirmText,
    cancelButtonText: cancelText,
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
  const result = await MySwal.fire({
    title,
    input: inputType,
    inputPlaceholder,
    showCancelButton: true,
    confirmButtonText: 'Submit',
    cancelButtonText: 'Cancel',
    confirmButtonColor: '#3b82f6',
    inputValidator: (value) => {
      if (!value) {
        return 'This field is required!';
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
  return MySwal.fire({
    toast: true,
    position: 'bottom-end',
    icon,
    title,
    showConfirmButton: false,
    timer: duration,
    timerProgressBar: true,
  });
};

// Loading Alert
export const showLoading = (title = 'Loading...') => {
  MySwal.fire({
    title,
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

