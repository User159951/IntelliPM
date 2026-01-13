import { createRoot } from "react-dom/client";
import * as Sentry from "@sentry/react";
import "./i18n/config";
import App from "./App.tsx";
import "./index.css";

// Load API connectivity tester for console usage (development only)
if (import.meta.env.DEV) {
  import('./utils/testReleaseApiConnectivity');
}

// Initialize Sentry if DSN is configured
if (import.meta.env.VITE_SENTRY_DSN) {
  Sentry.init({
    dsn: import.meta.env.VITE_SENTRY_DSN,
    environment: import.meta.env.VITE_SENTRY_ENVIRONMENT || "development",
    integrations: [
      Sentry.browserTracingIntegration(),
      Sentry.replayIntegration({
        maskAllText: true,
        blockAllMedia: true,
      }),
    ],
    tracesSampleRate: import.meta.env.PROD ? 0.2 : 1.0,
    replaysSessionSampleRate: 0.1,
    replaysOnErrorSampleRate: 1.0,
  });
}

createRoot(document.getElementById("root")!).render(<App />);
