import { defineConfig, type Plugin } from "vite";
import react from "@vitejs/plugin-react-swc";
import path from "path";
import { readFileSync } from "fs";

// Read version from package.json
const packageJson = JSON.parse(readFileSync('./package.json', 'utf-8'));
const version = packageJson.version || '1.0.0';

// Generate build date in YYYY.MM.DD format
const buildDate = new Date().toISOString().split('T')[0].replace(/-/g, '.');

/**
 * Vite plugin to prevent mock imports in production builds
 * This plugin will fail the build if mocks are imported outside of test files
 */
function preventMockImportsInProduction(): Plugin {
  return {
    name: 'prevent-mock-imports-in-production',
    enforce: 'pre',
    load(id) {
      // Only check in production builds (not in test mode)
      const isProductionBuild = 
        process.env.NODE_ENV === 'production' || 
        (process.env.VITE_BUILD === 'production' && !process.env.VITEST);
      
      if (isProductionBuild) {
        // Check if the file being loaded is a mock file
        if (id.includes('/mocks/') && !id.includes('node_modules')) {
          // This should never happen in production as mocks should be excluded
          // But if it does, fail the build
          throw new Error(
            `❌ Mock files cannot be loaded in production builds!\n` +
            `   File: ${id}\n` +
            `   Mock files are excluded from production builds via tsconfig.app.json.`
          );
        }
      }
      return null;
    },
    resolveId(id, importer) {
      // Only check in production builds (not in test mode)
      const isProductionBuild = 
        process.env.NODE_ENV === 'production' || 
        (process.env.VITE_BUILD === 'production' && !process.env.VITEST);
      
      if (isProductionBuild && importer) {
        // Check if the import is from @/mocks
        if (id.includes('@/mocks') || (id.includes('/mocks/') && !id.includes('node_modules'))) {
          // Allow imports from test files (though these shouldn't be in production build)
          const isTestFile = 
            importer.includes('.test.') ||
            importer.includes('.spec.') ||
            importer.includes('test/setup') ||
            importer.includes('vitest.config') ||
            importer.includes('vite.config');
          
          if (!isTestFile) {
            // Fail the build if mocks are imported outside test files
            throw new Error(
              `❌ Mock imports are not allowed in production builds!\n` +
              `   File: ${importer}\n` +
              `   Import: ${id}\n` +
              `   Mocks can only be imported in test files (*.test.ts, *.test.tsx, *.spec.ts, *.spec.tsx) ` +
              `or test setup files (test/setup.ts).`
            );
          }
        }
      }
      return null;
    },
  };
}

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
  plugins: [
    react(),
    preventMockImportsInProduction(),
  ],
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
