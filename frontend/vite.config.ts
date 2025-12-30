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
    host: "::",
    port: 8080,
    hmr: {
      // Configure HMR to work with port mapping (e.g., Docker 3001:5173)
      // If running in Docker, the client port might be different from server port
      clientPort: process.env.VITE_HMR_CLIENT_PORT 
        ? parseInt(process.env.VITE_HMR_CLIENT_PORT) 
        : undefined, // Auto-detect if not set
    },
  },
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  define: {
    'import.meta.env.VITE_APP_VERSION': JSON.stringify(version),
    'import.meta.env.VITE_BUILD_DATE': JSON.stringify(buildDate),
  },
  build: {
    rollupOptions: {
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
