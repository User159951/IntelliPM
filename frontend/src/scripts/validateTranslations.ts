#!/usr/bin/env tsx
/**
 * Translation Validation Script
 * 
 * Validates that all translation files have matching keys across all languages.
 * Checks for:
 * - Missing keys in any language
 * - Empty values
 * - Invalid JSON structure
 * 
 * Usage: npm run i18n:check
 */

import * as fs from 'fs';
import * as path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Get project root (frontend directory)
const projectRoot = path.resolve(__dirname, '../..');
const localesDir = path.join(projectRoot, 'public', 'locales');

// Supported languages
const SUPPORTED_LANGUAGES = ['en', 'fr'];

// Namespaces from i18n config
const NAMESPACES = [
  'common',
  'auth',
  'projects',
  'tasks',
  'admin',
  'navigation',
  'notifications',
  'errors',
  'dashboard',
  'sprints',
  'teams',
  'backlog',
  'defects',
  'metrics',
  'insights',
  'agents',
  'milestones',
  'releases',
];

interface ValidationResult {
  success: boolean;
  errors: string[];
  warnings: string[];
  stats: {
    languages: Record<string, { files: number; keys: number }>;
    totalKeys: number;
    missingKeys: string[];
    emptyValues: string[];
  };
}

/**
 * Recursively get all keys from a translation object
 */
function getAllKeys(obj: Record<string, unknown>, prefix = ''): string[] {
  const keys: string[] = [];
  
  for (const [key, value] of Object.entries(obj)) {
    const fullKey = prefix ? `${prefix}.${key}` : key;
    
    if (typeof value === 'object' && value !== null && !Array.isArray(value)) {
      keys.push(...getAllKeys(value as Record<string, unknown>, fullKey));
    } else {
      keys.push(fullKey);
    }
  }
  
  return keys;
}

/**
 * Get nested value from object using dot notation
 */
function getNestedValue(obj: Record<string, unknown>, key: string): unknown {
  const keys = key.split('.');
  let current = obj;
  
  for (const k of keys) {
    if (current && typeof current === 'object' && k in current) {
      current = (current as Record<string, unknown>)[k] as Record<string, unknown>;
    } else {
      return undefined;
    }
  }
  
  return current;
}

/**
 * Validate a single translation file
 */
function validateFile(filePath: string): { valid: boolean; data: Record<string, unknown> | null; error?: string } {
  try {
    if (!fs.existsSync(filePath)) {
      return { valid: false, data: null, error: `File not found: ${filePath}` };
    }
    
    const content = fs.readFileSync(filePath, 'utf-8');
    
    if (!content.trim()) {
      return { valid: false, data: null, error: `File is empty: ${filePath}` };
    }
    
    const data = JSON.parse(content);
    
    if (typeof data !== 'object' || data === null || Array.isArray(data)) {
      return { valid: false, data: null, error: `Invalid JSON structure: ${filePath}` };
    }
    
    return { valid: true, data };
  } catch (error) {
    return {
      valid: false,
      data: null,
      error: `Error parsing JSON: ${error instanceof Error ? error.message : String(error)}`,
    };
  }
}

/**
 * Find empty values in translation object
 */
function findEmptyValues(obj: Record<string, unknown>, prefix = ''): string[] {
  const empty: string[] = [];
  
  for (const [key, value] of Object.entries(obj)) {
    const fullKey = prefix ? `${prefix}.${key}` : key;
    
    if (typeof value === 'object' && value !== null && !Array.isArray(value)) {
      empty.push(...findEmptyValues(value as Record<string, unknown>, fullKey));
    } else if (value === '' || value === null || value === undefined) {
      empty.push(fullKey);
    }
  }
  
  return empty;
}

/**
 * Main validation function
 */
function validateTranslations(): ValidationResult {
  const result: ValidationResult = {
    success: true,
    errors: [],
    warnings: [],
    stats: {
      languages: {},
      totalKeys: 0,
      missingKeys: [],
      emptyValues: [],
    },
  };
  
  console.log('✓ Checking translation files...\n');
  
  // Collect all translation data
  const translations: Record<string, Record<string, Record<string, unknown>>> = {};
  
  // Initialize structure
  for (const lang of SUPPORTED_LANGUAGES) {
    translations[lang] = {};
    result.stats.languages[lang] = { files: 0, keys: 0 };
  }
  
  // Load all translation files
  for (const lang of SUPPORTED_LANGUAGES) {
    const langDir = path.join(localesDir, lang);
    
    if (!fs.existsSync(langDir)) {
      result.success = false;
      result.errors.push(`Language directory not found: ${langDir}`);
      continue;
    }
    
    for (const ns of NAMESPACES) {
      const filePath = path.join(langDir, `${ns}.json`);
      const validation = validateFile(filePath);
      
      if (!validation.valid) {
        result.success = false;
        result.errors.push(`${lang}/${ns}.json: ${validation.error}`);
        continue;
      }
      
      if (validation.data) {
        translations[lang][ns] = validation.data;
        result.stats.languages[lang].files++;
        
        const keys = getAllKeys(validation.data);
        result.stats.languages[lang].keys += keys.length;
        
        // Check for empty values
        const empty = findEmptyValues(validation.data, ns);
        if (empty.length > 0) {
          result.stats.emptyValues.push(...empty.map(key => `${lang}.${key}`));
        }
      }
    }
  }
  
  // Use English as the reference language
  const referenceLang = 'en';
  const referenceKeys: Record<string, string[]> = {};
  
  // Collect all keys from reference language
  for (const ns of NAMESPACES) {
    if (translations[referenceLang][ns]) {
      referenceKeys[ns] = getAllKeys(translations[referenceLang][ns], ns);
    }
  }
  
  // Check all languages against reference
  for (const lang of SUPPORTED_LANGUAGES) {
    if (lang === referenceLang) continue;
    
    for (const ns of NAMESPACES) {
      const refKeys = referenceKeys[ns] || [];
      const langData = translations[lang][ns];
      
      if (!langData) {
        // File doesn't exist for this language
        result.stats.missingKeys.push(...refKeys.map(key => `${lang}.${key}`));
        continue;
      }
      
      // Check each reference key exists in this language
      for (const refKey of refKeys) {
        const keyWithoutNs = refKey.replace(`${ns}.`, '');
        const value = getNestedValue(langData, keyWithoutNs);
        
        if (value === undefined) {
          result.stats.missingKeys.push(`${lang}.${refKey}`);
        }
      }
    }
  }
  
  // Check reference language has all keys from other languages (reverse check)
  for (const lang of SUPPORTED_LANGUAGES) {
    if (lang === referenceLang) continue;
    
    for (const ns of NAMESPACES) {
      const langData = translations[lang][ns];
      if (!langData) continue;
      
      const langKeys = getAllKeys(langData, ns);
      const refData = translations[referenceLang][ns];
      
      if (!refData) continue;
      
      for (const langKey of langKeys) {
        const keyWithoutNs = langKey.replace(`${ns}.`, '');
        const value = getNestedValue(refData, keyWithoutNs);
        
        if (value === undefined) {
          result.warnings.push(`Key exists in ${lang} but not in ${referenceLang}: ${langKey}`);
        }
      }
    }
  }
  
  // Calculate total keys (from reference language)
  result.stats.totalKeys = Object.values(referenceKeys).reduce((sum, keys) => sum + keys.length, 0);
  
  // Set success flag
  if (result.stats.missingKeys.length > 0 || result.stats.emptyValues.length > 0 || result.errors.length > 0) {
    result.success = false;
  }
  
  return result;
}

/**
 * Print validation results
 */
function printResults(result: ValidationResult): void {
  // Print statistics
  console.log('📊 Statistics:');
  for (const [lang, stats] of Object.entries(result.stats.languages)) {
    console.log(`  ✓ ${lang.toUpperCase()}: ${stats.files} files, ${stats.keys} keys`);
  }
  console.log(`  Total keys: ${result.stats.totalKeys}\n`);
  
  // Print missing keys
  if (result.stats.missingKeys.length > 0) {
    console.log('✗ Missing keys:');
    const grouped = result.stats.missingKeys.reduce((acc, key) => {
      const [lang] = key.split('.');
      if (!acc[lang]) acc[lang] = [];
      acc[lang].push(key);
      return acc;
    }, {} as Record<string, string[]>);
    
    for (const [lang, keys] of Object.entries(grouped)) {
      console.log(`  ${lang.toUpperCase()}:`);
      for (const key of keys.slice(0, 10)) {
        console.log(`    - ${key}`);
      }
      if (keys.length > 10) {
        console.log(`    ... and ${keys.length - 10} more`);
      }
    }
    console.log();
  }
  
  // Print empty values
  if (result.stats.emptyValues.length > 0) {
    console.log('⚠ Empty values found:');
    for (const key of result.stats.emptyValues.slice(0, 10)) {
      console.log(`  - ${key}`);
    }
    if (result.stats.emptyValues.length > 10) {
      console.log(`  ... and ${result.stats.emptyValues.length - 10} more`);
    }
    console.log();
  }
  
  // Print errors
  if (result.errors.length > 0) {
    console.log('✗ Errors:');
    for (const error of result.errors) {
      console.log(`  - ${error}`);
    }
    console.log();
  }
  
  // Print warnings
  if (result.warnings.length > 0) {
    console.log('⚠ Warnings:');
    for (const warning of result.warnings.slice(0, 5)) {
      console.log(`  - ${warning}`);
    }
    if (result.warnings.length > 5) {
      console.log(`  ... and ${result.warnings.length - 5} more`);
    }
    console.log();
  }
  
  // Print final result
  if (result.success) {
    console.log('✓ PASSED: All translations are valid');
    process.exit(0);
  } else {
    const missingCount = result.stats.missingKeys.length;
    const emptyCount = result.stats.emptyValues.length;
    const errorCount = result.errors.length;
    const totalIssues = missingCount + emptyCount + errorCount;
    
    console.log(`✗ FAILED: ${totalIssues} issue(s) found`);
    if (missingCount > 0) console.log(`  - ${missingCount} missing translation(s)`);
    if (emptyCount > 0) console.log(`  - ${emptyCount} empty value(s)`);
    if (errorCount > 0) console.log(`  - ${errorCount} error(s)`);
    
    process.exit(1);
  }
}

// Run validation
try {
  const result = validateTranslations();
  printResults(result);
} catch (error) {
  console.error('✗ Validation script error:', error);
  process.exit(1);
}

