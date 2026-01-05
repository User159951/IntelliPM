import * as React from "react";
import * as DialogPrimitive from "@radix-ui/react-dialog";
import { X } from "lucide-react";

import { cn } from "@/lib/utils";

const Dialog = DialogPrimitive.Root;

const DialogTrigger = DialogPrimitive.Trigger;

const DialogPortal = DialogPrimitive.Portal;

const DialogClose = DialogPrimitive.Close;

const DialogOverlay = React.forwardRef<
  React.ElementRef<typeof DialogPrimitive.Overlay>,
  React.ComponentPropsWithoutRef<typeof DialogPrimitive.Overlay>
>(({ className, ...props }, ref) => (
  <DialogPrimitive.Overlay
    ref={ref}
    className={cn(
      "fixed inset-0 z-50 bg-black/80 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0",
      className,
    )}
    {...props}
  />
));
DialogOverlay.displayName = DialogPrimitive.Overlay.displayName;

const DialogContent = React.forwardRef<
  React.ElementRef<typeof DialogPrimitive.Content>,
  React.ComponentPropsWithoutRef<typeof DialogPrimitive.Content>
>(({ className, children, onOpenAutoFocus, ...props }, ref) => {
  const contentRef = React.useRef<HTMLDivElement>(null);

  // Handle focus to prevent aria-hidden warning
  // The warning occurs when an element outside the dialog is focused while the root has aria-hidden
  const handleOpenAutoFocus = React.useCallback((e: Event) => {
    // Blur any focused elements outside the dialog content before opening
    const contentElement = contentRef.current;
    const root = document.getElementById('root');
    const activeElement = document.activeElement as HTMLElement;
    
    if (root && activeElement && contentElement) {
      // If the active element is in root but not in dialog content, blur it
      if (root.contains(activeElement) && !contentElement.contains(activeElement)) {
        activeElement.blur();
      }
    }
    
    // Call custom handler if provided, otherwise let Radix handle default focus
    if (onOpenAutoFocus) {
      onOpenAutoFocus(e);
    }
  }, [onOpenAutoFocus]);

  // Use effect to blur elements outside dialog when dialog state changes to open
  React.useEffect(() => {
    const contentElement = contentRef.current;
    if (!contentElement) return;

    // Check if dialog is open by checking data-state attribute
    const checkDialogState = () => {
      const isOpen = contentElement.getAttribute('data-state') === 'open';
      if (isOpen) {
        const root = document.getElementById('root');
        const activeElement = document.activeElement as HTMLElement;
        
        // If there's a focused element outside the dialog, blur it
        if (root && activeElement && root.contains(activeElement) && !contentElement.contains(activeElement)) {
          activeElement.blur();
        }
      }
    };

    // Use MutationObserver to watch for data-state changes
    const observer = new MutationObserver(checkDialogState);
    observer.observe(contentElement, {
      attributes: true,
      attributeFilter: ['data-state']
    });

    // Also check immediately
    checkDialogState();
    
    return () => {
      observer.disconnect();
    };
  }, []);

  return (
  <DialogPortal>
    <DialogOverlay />
    <DialogPrimitive.Content
      ref={(node) => {
        if (typeof ref === 'function') {
          ref(node);
        } else if (ref) {
          (ref as React.MutableRefObject<HTMLDivElement | null>).current = node;
        }
        if (contentRef) {
          (contentRef as React.MutableRefObject<HTMLDivElement | null>).current = node;
        }
      }}
      className={cn(
        "fixed left-[50%] top-[50%] z-50 grid w-full max-w-lg translate-x-[-50%] translate-y-[-50%] gap-4 border bg-background p-6 shadow-lg duration-200 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95 data-[state=closed]:slide-out-to-left-1/2 data-[state=closed]:slide-out-to-top-[48%] data-[state=open]:slide-in-from-left-1/2 data-[state=open]:slide-in-from-top-[48%] sm:rounded-lg",
        className,
      )}
        onOpenAutoFocus={handleOpenAutoFocus}
      {...props}
    >
      {children}
      <DialogPrimitive.Close className="absolute right-4 top-4 rounded-sm opacity-70 ring-offset-background transition-opacity data-[state=open]:bg-accent data-[state=open]:text-muted-foreground hover:opacity-100 focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:pointer-events-none">
        <X className="h-4 w-4" />
        <span className="sr-only">Close</span>
      </DialogPrimitive.Close>
    </DialogPrimitive.Content>
  </DialogPortal>
  );
});
DialogContent.displayName = DialogPrimitive.Content.displayName;

const DialogHeader = ({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) => (
  <div className={cn("flex flex-col space-y-1.5 text-center sm:text-left", className)} {...props} />
);
DialogHeader.displayName = "DialogHeader";

const DialogFooter = ({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) => (
  <div className={cn("flex flex-col-reverse sm:flex-row sm:justify-end sm:space-x-2", className)} {...props} />
);
DialogFooter.displayName = "DialogFooter";

const DialogTitle = React.forwardRef<
  React.ElementRef<typeof DialogPrimitive.Title>,
  React.ComponentPropsWithoutRef<typeof DialogPrimitive.Title>
>(({ className, ...props }, ref) => (
  <DialogPrimitive.Title
    ref={ref}
    className={cn("text-lg font-semibold leading-none tracking-tight", className)}
    {...props}
  />
));
DialogTitle.displayName = DialogPrimitive.Title.displayName;

const DialogDescription = React.forwardRef<
  React.ElementRef<typeof DialogPrimitive.Description>,
  React.ComponentPropsWithoutRef<typeof DialogPrimitive.Description>
>(({ className, ...props }, ref) => (
  <DialogPrimitive.Description ref={ref} className={cn("text-sm text-muted-foreground", className)} {...props} />
));
DialogDescription.displayName = DialogPrimitive.Description.displayName;

export {
  Dialog,
  DialogPortal,
  DialogOverlay,
  DialogClose,
  DialogTrigger,
  DialogContent,
  DialogHeader,
  DialogFooter,
  DialogTitle,
  DialogDescription,
};
