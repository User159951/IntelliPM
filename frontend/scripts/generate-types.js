#!/usr/bin/env node
/**
 * Script to generate TypeScript types from backend OpenAPI/Swagger spec
 * 
 * This script:
 * 1. Fetches the OpenAPI spec from the backend
 * 2. Generates TypeScript types using openapi-typescript CLI
 * 3. Outputs to src/types/generated/api.ts
 */

import { execSync } from 'child_process';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

// Get API base URL from environment or use default
const API_BASE_URL = process.env.VITE_API_BASE_URL || 'http://localhost:5001';
const SWAGGER_URL = `${API_BASE_URL}/swagger/v1/swagger.json`;
const OUTPUT_FILE = join(__dirname, '../src/types/generated/api.ts');

try {
  console.log(`Fetching OpenAPI spec from ${SWAGGER_URL}...`);
  console.log('Generating TypeScript types...');
  
  // Use the openapi-typescript CLI
  execSync(`npx openapi-typescript "${SWAGGER_URL}" -o "${OUTPUT_FILE}"`, {
    stdio: 'inherit',
    cwd: join(__dirname, '..'),
  });
  
  console.log(`✅ Types generated successfully to ${OUTPUT_FILE}`);
  
} catch (error) {
  console.error('❌ Error generating types:', error.message);
  if (error.message.includes('fetch') || error.message.includes('ECONNREFUSED')) {
    console.error('\n💡 Make sure the backend API is running at', SWAGGER_URL);
    console.error('   You can start it with: cd backend/IntelliPM.API && dotnet run');
  }
  process.exit(1);
}

