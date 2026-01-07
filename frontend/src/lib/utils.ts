import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";
import { PERMISSIONS } from "@/hooks/usePermissions";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

/**
 * Get human-readable permission name from permission string
 * @param permission - Permission string (e.g., "projects.create")
 * @returns Human-readable name (e.g., "Create Projects")
 */
export function getPermissionDisplayName(permission: string): string {
  // Try to find in PERMISSIONS constant first
  const permissionEntry = Object.entries(PERMISSIONS).find(
    ([_, value]) => value === permission
  );
  
  if (permissionEntry) {
    // Convert "PROJECTS_CREATE" to "Create Projects"
    const key = permissionEntry[0];
    const parts = key.split('_');
    const action = parts[parts.length - 1].toLowerCase();
    const resource = parts.slice(0, -1).join(' ').toLowerCase();
    
    const actionMap: Record<string, string> = {
      create: 'Create',
      edit: 'Edit',
      delete: 'Delete',
      view: 'View',
      manage: 'Manage',
      invite: 'Invite',
      remove: 'Remove',
      changeRole: 'Change Role',
      assign: 'Assign',
      comment: 'Comment',
      update: 'Update',
    };
    
    const actionDisplay = actionMap[action] || action;
    const resourceDisplay = resource
      .split(' ')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ');
    
    return `${actionDisplay} ${resourceDisplay}`;
  }
  
  // Fallback: format permission string
  const parts = permission.split('.');
  if (parts.length === 2) {
    const [resource, action] = parts;
    const actionMap: Record<string, string> = {
      create: 'Create',
      edit: 'Edit',
      delete: 'Delete',
      view: 'View',
      manage: 'Manage',
      invite: 'Invite',
      remove: 'Remove',
      changerole: 'Change Role',
      assign: 'Assign',
      comment: 'Comment',
      update: 'Update',
    };
    
    const actionDisplay = actionMap[action.toLowerCase()] || action;
    const resourceDisplay = resource
      .split('.')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ');
    
    return `${actionDisplay} ${resourceDisplay}`;
  }
  
  return permission;
}

/**
 * Extract permission error message from API error response
 * @param error - Error object from API call
 * @returns User-friendly error message or null
 */
export function extractPermissionError(error: unknown): string | null {
  if (!error || typeof error !== 'object') {
    return null;
  }

  // Check if it's an Axios/API error with response
  const apiError = error as {
    response?: {
      status?: number;
      data?: {
        message?: string;
        error?: string;
        title?: string;
        detail?: string;
      };
    };
    message?: string;
  };

  // Check for 403 Forbidden status
  if (apiError.response?.status === 403) {
    const data = apiError.response.data;
    if (data?.message) {
      return data.message;
    }
    if (data?.error) {
      return data.error;
    }
    if (data?.detail) {
      return data.detail;
    }
    if (data?.title) {
      return data.title;
    }
    return "You don't have permission to perform this action.";
  }

  // Check error message for permission-related keywords
  const errorMessage = apiError.message || JSON.stringify(error);
  const permissionKeywords = [
    'permission',
    'forbidden',
    'unauthorized',
    'access denied',
    'not allowed',
  ];

  if (permissionKeywords.some(keyword => errorMessage.toLowerCase().includes(keyword))) {
    return errorMessage;
  }

  return null;
}

/**
 * Format permission string for display in error messages
 * @param permission - Permission string (e.g., "projects.create")
 * @returns Formatted string for error message
 */
export function formatPermissionForError(permission: string): string {
  return getPermissionDisplayName(permission);
}
