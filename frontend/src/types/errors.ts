/**
 * API Error type for handling errors from API calls
 */
export interface ApiError extends Error {
  response?: {
    status?: number;
    data?: {
      error?: string;
      errors?: string[] | Record<string, string[]>;
      detail?: string;
      title?: string;
      message?: string;
    };
  };
  message: string;
}

