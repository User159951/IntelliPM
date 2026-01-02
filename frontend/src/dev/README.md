# Dev Tools

Ce dossier contient des outils et pages de développement qui ne doivent pas être inclus dans la production.

## Fichiers

- `ReleaseApiTest.tsx` - Page de test pour vérifier la connectivité de l'API Releases

## Utilisation

Ces fichiers sont exclus automatiquement du build de production via la configuration Vite. Pour les utiliser en développement :

1. Importer le composant dans `App.tsx` (uniquement en mode dev)
2. Ajouter une route conditionnelle avec `import.meta.env.DEV`

Exemple :

```typescript
{import.meta.env.DEV && (
  <Route path="/test/release-api" element={<ReleaseApiTestPage />} />
)}
```

## Note

Ces outils sont destinés uniquement au développement et au débogage. Ils ne doivent jamais être accessibles en production.

