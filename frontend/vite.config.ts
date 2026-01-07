import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";
import path from "path";
import { readFileSync } from "fs";

// Read version from package.json
const packageJson = JSON.parse(readFileSync('./package.json', 'utf-8'));
const version = packageJson.version || '1.0.0';

// Generate build date in YYYY.MM.DD format
const buildDate = new Date().toISOString().split('T')[0].replace(/-/g, '.');

// https://vitejs.dev/config/
export default defineConfig(() => ({
  server: {
    host: "0.0.0.0", // Listen on all interfaces for Docker compatibility
    port: 5173, // Standard Vite port (matches docker-compose.yml)
    hmr: {
      // Configure HMR to work with port mapping (e.g., Docker 3001:5173)
      // When running in Docker, the browser connects to host port 3001, but HMR needs to know
      clientPort: process.env.VITE_HMR_CLIENT_PORT 
        ? parseInt(process.env.VITE_HMR_CLIENT_PORT) 
        : 3001, // Default to 3001 (host port from docker-compose)
    },
  },
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  optimizeDeps: {
    include: ['react-window'],
  },
  define: {
    'import.meta.env.VITE_APP_VERSION': JSON.stringify(version),
    'import.meta.env.VITE_BUILD_DATE': JSON.stringify(buildDate),
  },
  build: {
    rollupOptions: {
      // Note: Files in /src/dev/ are automatically excluded from production builds
      // because they are not imported anywhere in the application code.
      output: {
        manualChunks: (id) => {
          // Vendor chunks
          if (id.includes('node_modules')) {
            // React core
            if (id.includes('react') || id.includes('react-dom') || id.includes('react-router')) {
              return 'vendor-react';
            }
            // Radix UI components
            if (id.includes('@radix-ui')) {
              return 'vendor-radix';
            }
            // Data libraries
            if (id.includes('@tanstack/react-query') || id.includes('recharts')) {
              return 'vendor-data';
            }
            // Sentry
            if (id.includes('@sentry')) {
              return 'vendor-sentry';
            }
            // Other large dependencies
            if (id.includes('date-fns') || id.includes('lucide-react')) {
              return 'vendor-utils';
            }
            // All other node_modules
            return 'vendor';
          }
        },
      },
    },
    chunkSizeWarningLimit: 1000, // Increase limit to 1MB (default is 500KB)
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
    css: true,
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: [
        'node_modules/',
        'src/test/',
        'src/dev/', // Exclude dev tools from test coverage
        '**/*.d.ts',
        '**/*.config.*',
        '**/mockServiceWorker.js',
      ],
      thresholds: {
        lines: 70,
        functions: 70,
        branches: 70,
        statements: 70,
      },
    },
  },
}));
