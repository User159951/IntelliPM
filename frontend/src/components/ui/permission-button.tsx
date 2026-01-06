import React from 'react';
import { Button, ButtonProps } from '@/components/ui/button';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';

interface PermissionButtonProps extends ButtonProps {
  hasPermission: boolean;
  permissionName?: string;
  disabledReason?: string;
  showTooltipWhenEnabled?: boolean;
}

export const PermissionButton = React.forwardRef<
  HTMLButtonElement,
  PermissionButtonProps
>(
  (
    {
      hasPermission,
      permissionName,
      disabledReason,
      showTooltipWhenEnabled = false,
      children,
      disabled,
      ...props
    },
    ref
  ) => {
    const isDisabled = !hasPermission || disabled;

    const defaultReason = permissionName
      ? `You don't have permission to perform this action. Required: ${permissionName}`
      : "You don't have permission to perform this action";

    const tooltipContent = disabledReason || defaultReason;

    if (hasPermission && !disabled && !showTooltipWhenEnabled) {
      return (
        <Button ref={ref} {...props}>
          {children}
        </Button>
      );
    }

    return (
      <TooltipProvider>
        <Tooltip>
          <TooltipTrigger asChild>
            <span className="inline-block">
              <Button ref={ref} disabled={isDisabled} {...props}>
                {children}
              </Button>
            </span>
          </TooltipTrigger>
          {isDisabled && (
            <TooltipContent>
              <p className="max-w-xs">{tooltipContent}</p>
            </TooltipContent>
          )}
        </Tooltip>
      </TooltipProvider>
    );
  }
);

PermissionButton.displayName = 'PermissionButton';

