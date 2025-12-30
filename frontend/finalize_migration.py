import os
import re
from pathlib import Path
from typing import List, Tuple, Dict

# Configuration
FRONTEND_DIR = Path("src")
DRY_RUN = False  # Mettez False pour appliquer les changements
BACKUP = True   # Créer des backups avant modification

# Liste des fichiers à traiter
FILES_TO_PROCESS = [
    # 🔥 Priorité HAUTE
    "pages/auth/ResetPassword.tsx",
    "pages/admin/AdminUsers.tsx",
    "pages/admin/AdminSettings.tsx",
    "pages/admin/AdminPermissions.tsx",
    "pages/admin/AdminAuditLogs.tsx",
    
    # 🟡 Priorité MOYENNE - Tasks
    "components/tasks/CreateTaskDialog.tsx",
    "components/tasks/TaskDetailSheet.tsx",
    "components/tasks/TaskListView.tsx",
    "components/tasks/AITaskImproverDialog.tsx",
    
    # 🟡 Priorité MOYENNE - Projects
    "components/projects/AddMemberDialog.tsx",
    "components/projects/EditProjectDialog.tsx",
    "components/projects/InviteMemberModal.tsx",
    "components/projects/TeamMembersList.tsx",
    
    # 🟡 Priorité MOYENNE - Sprints
    "components/sprints/AddTasksToSprintDialog.tsx",
    
    # 🟡 Priorité MOYENNE - Pages
    "pages/Sprints.tsx",
    "pages/Tasks.tsx",
    "pages/Backlog.tsx",
    "pages/Agents.tsx",
    "pages/Insights.tsx",
    "pages/ProjectDetail.tsx",
    "pages/ProjectMembers.tsx",
    "pages/Teams.tsx",
    
    # 🟢 Priorité BASSE - Components
    "components/defects/CreateDefectDialog.tsx",
    "components/defects/DefectDetailSheet.tsx",
    "components/agents/ProjectInsightPanel.tsx",
    "components/agents/RiskDetectionPanel.tsx",
    "components/agents/SprintPlanningAssistant.tsx",
    "components/admin/EditUserDialog.tsx",
    "components/guards/PermissionGuard.tsx",
    "components/guards/RequireAdminGuard.tsx",
    "components/notifications/NotificationDropdown.tsx",
]

def replace_toast_multiline(content: str) -> str:
    """
    Remplace les appels toast({ sur plusieurs lignes.
    C'est le plus complexe car il faut gérer les sauts de ligne.
    """
    
    # Pattern pour toast({ ... }) sur plusieurs lignes
    # Capture tout entre toast({ et le }) correspondant
    pattern = r'toast\s*\(\s*\{([^}]+)\}\s*\)'
    
    def replace_callback(match):
        inner = match.group(1)
        
        # Extraire title
        title_match = re.search(r'title:\s*(["\'])((?:[^"\'\\]|\\.)*?)\1', inner)
        title_template_match = re.search(r'title:\s*`([^`]*)`', inner)
        
        # Extraire description
        desc_match = re.search(r'description:\s*(["\'])((?:[^"\'\\]|\\.)*?)\1', inner)
        desc_template_match = re.search(r'description:\s*`([^`]*)`', inner)
        
        # Extraire variant
        variant_match = re.search(r'variant:\s*["\']destructive["\']', inner)
        
        is_error = variant_match is not None
        
        # Construire le remplacement
        if title_template_match:
            title = f'`{title_template_match.group(1)}`'
        elif title_match:
            title = f'{title_match.group(1)}{title_match.group(2)}{title_match.group(1)}'
        else:
            # Pas de title, chercher juste description
            if desc_template_match:
                title = f'`{desc_template_match.group(1)}`'
            elif desc_match:
                title = f'{desc_match.group(1)}{desc_match.group(2)}{desc_match.group(1)}'
            else:
                return match.group(0)  # Pas de changement
        
        # Si description existe séparément
        description = None
        if desc_template_match and not title_template_match:
            description = f'`{desc_template_match.group(1)}`'
        elif desc_match and not title_match:
            description = f'{desc_match.group(1)}{desc_match.group(2)}{desc_match.group(1)}'
        
        # Générer le remplacement
        if is_error:
            if description:
                return f'showError({title}, {description})'
            else:
                return f'showError({title})'
        else:
            if description:
                return f'showSuccess({title}, {description})'
            else:
                return f'showToast({title}, "success")'
    
    return re.sub(pattern, replace_callback, content, flags=re.DOTALL)

def create_backup(file_path: Path) -> None:
    """Crée un backup du fichier."""
    if BACKUP and file_path.exists():
        backup_path = file_path.with_suffix(file_path.suffix + '.bak')
        backup_path.write_text(file_path.read_text(encoding='utf-8'), encoding='utf-8')

def process_file(file_path: Path) -> Tuple[bool, List[str], Dict[str, int]]:
    """Process un fichier et retourne (modifié, warnings, stats)."""
    warnings = []
    stats = {'toasts_replaced': 0}
    
    try:
        if not file_path.exists():
            return False, [f"Fichier non trouvé: {file_path}"], stats
        
        content = file_path.read_text(encoding='utf-8')
        original_content = content
        
        # Compter les toast({ avant
        toast_count_before = len(re.findall(r'toast\s*\(\s*\{', content))
        
        # Appliquer le remplacement multilignes (le plus puissant)
        content = replace_toast_multiline(content)
        
        # Retirer toast des dépendances useEffect/useCallback
        content = re.sub(r',\s*toast\s*\]', ']', content)
        content = re.sub(r'\[\s*toast\s*,', '[', content)
        content = re.sub(r'\[\s*toast\s*\]', '[]', content)
        
        # Compter les toast({ après
        toast_count_after = len(re.findall(r'toast\s*\(\s*\{', content))
        stats['toasts_replaced'] = toast_count_before - toast_count_after
        
        # Vérifier s'il reste des patterns
        if toast_count_after > 0:
            warnings.append(f"⚠️  Il reste {toast_count_after} appel(s) toast({{ - vérification recommandée")
            
            # Essayer d'extraire les lignes concernées
            remaining_toasts = re.finditer(r'toast\s*\(\s*\{[^}]*\}', content)
            for i, match in enumerate(remaining_toasts, 1):
                if i <= 3:  # Montrer max 3 exemples
                    snippet = match.group(0)[:60]
                    warnings.append(f"   Exemple {i}: {snippet}...")
        
        # Si pas de changement
        if content == original_content:
            return False, ["Aucun changement nécessaire"], stats
        
        # Créer backup et écrire
        if not DRY_RUN:
            create_backup(file_path)
            file_path.write_text(content, encoding='utf-8')
        
        return True, warnings, stats
        
    except Exception as e:
        return False, [f"❌ Erreur: {e}"], stats

def update_app_tsx():
    """Met à jour App.tsx pour retirer Toaster et Sonner."""
    app_file = FRONTEND_DIR / "App.tsx"
    
    if not app_file.exists():
        print("⚠️  App.tsx non trouvé")
        return False
    
    try:
        content = app_file.read_text(encoding='utf-8')
        original_content = content
        
        # Retirer les imports
        content = re.sub(
            r'import\s*\{\s*Toaster\s*\}\s*from\s*["\']@/components/ui/toaster["\']\s*;?\s*\n?',
            '',
            content
        )
        content = re.sub(
            r'import\s*\{\s*Toaster\s+as\s+Sonner\s*\}\s*from\s*["\']@/components/ui/sonner["\']\s*;?\s*\n?',
            '',
            content
        )
        
        # Retirer les composants dans le JSX
        content = re.sub(r'<Toaster\s*/>\s*\n?', '', content)
        content = re.sub(r'<Sonner\s*/>\s*\n?', '', content)
        
        if content != original_content:
            if not DRY_RUN:
                create_backup(app_file)
                app_file.write_text(content, encoding='utf-8')
            return True
        
        return False
        
    except Exception as e:
        print(f"❌ Erreur lors de la mise à jour de App.tsx: {e}")
        return False

def main():
    """Fonction principale."""
    if not FRONTEND_DIR.exists():
        print(f"❌ Répertoire non trouvé: {FRONTEND_DIR}")
        print(f"📁 Répertoire actuel: {Path.cwd()}")
        print(f"💡 Assurez-vous d'exécuter ce script depuis frontend/")
        return
    
    print("=" * 80)
    print("🚀 MIGRATION SWEETALERT2 - VERSION AVANCÉE")
    print("=" * 80)
    print(f"\n📂 Répertoire: {FRONTEND_DIR.absolute()}")
    print(f"🔄 Mode: {'DRY RUN (simulation)' if DRY_RUN else 'LIVE (modifications réelles)'}")
    print(f"💾 Backups: {'Activés' if BACKUP else 'Désactivés'}")
    print(f"\n📋 Fichiers à traiter: {len(FILES_TO_PROCESS)}\n")
    
    modified_files = []
    files_with_warnings = []
    total_toasts_replaced = 0
    skipped_files = []
    perfectly_migrated = []
    
    # Traiter chaque fichier
    for relative_path in FILES_TO_PROCESS:
        file_path = FRONTEND_DIR / relative_path
        
        changed, warnings, stats = process_file(file_path)
        
        if not file_path.exists():
            skipped_files.append(relative_path)
            print(f"⏭️  SKIP: {relative_path} (non trouvé)")
            continue
        
        if changed:
            modified_files.append(relative_path)
            total_toasts_replaced += stats['toasts_replaced']
            
            status = "📝 SIMULATION" if DRY_RUN else "✅ MODIFIÉ"
            print(f"{status}: {relative_path}")
            print(f"   └─ {stats['toasts_replaced']} toast({{ remplacés")
            
            has_remaining = any('Il reste' in w for w in warnings)
            
            if has_remaining:
                files_with_warnings.append((relative_path, warnings))
                for warning in warnings:
                    print(f"   └─ {warning}")
            else:
                perfectly_migrated.append(relative_path)
                print(f"   └─ ✅ Migration complète!")
        else:
            if "Aucun changement" not in str(warnings):
                print(f"ℹ️  SKIP: {relative_path} - {warnings[0]}")
    
    # Mise à jour de App.tsx
    print(f"\n{'─' * 80}\n")
    print("🔧 Mise à jour de App.tsx...")
    
    app_updated = update_app_tsx()
    if app_updated:
        status = "📝 SIMULATION" if DRY_RUN else "✅ MODIFIÉ"
        print(f"{status}: App.tsx - Toaster et Sonner retirés")
    else:
        print("ℹ️  App.tsx déjà à jour ou non modifié")
    
    # Résumé final
    print(f"\n{'=' * 80}")
    print("📊 RÉSUMÉ DE LA MIGRATION")
    print(f"{'=' * 80}\n")
    
    print(f"✅ Fichiers modifiés: {len(modified_files)}")
    print(f"🔄 Toasts remplacés: {total_toasts_replaced}")
    print(f"🎯 Migrations parfaites: {len(perfectly_migrated)}")
    print(f"⚠️  Fichiers nécessitant révision: {len(files_with_warnings)}")
    print(f"⏭️  Fichiers ignorés: {len(skipped_files)}")
    
    if perfectly_migrated:
        print(f"\n🎉 Fichiers 100% migrés (plus de toast({{):\n")
        for file_path in perfectly_migrated:
            print(f"  ✅ {file_path}")
    
    if files_with_warnings:
        print(f"\n⚠️  Fichiers avec toast({{ restants:\n")
        for file_path, warnings in files_with_warnings:
            print(f"  • {file_path}")
            for warning in warnings:
                if not warning.startswith("   "):
                    print(f"    {warning}")
    
    # Calcul progression
    total_files = len(FILES_TO_PROCESS)
    migrated_count = len(perfectly_migrated)
    percentage = (migrated_count / total_files * 100) if total_files > 0 else 0
    
    print(f"\n📈 Progression: {migrated_count}/{total_files} fichiers ({percentage:.1f}%)")
    
    # Instructions finales
    print(f"\n{'=' * 80}")
    
    if DRY_RUN:
        print("⚠️  CECI ÉTAIT UNE SIMULATION")
        print("\n📝 Pour appliquer les changements:")
        print("   1. ✅ Les résultats semblent bons")
        print("   2. Modifiez: DRY_RUN = False")
        print("   3. Relancez: python finalize_migration.py")
    else:
        print("✅ MIGRATION APPLIQUÉE!")
        print("\n📋 Prochaines étapes:")
        print("   1. Réviser manuellement les fichiers avec warnings")
        print("   2. Tester: npm run dev")
        print("   3. Vérifier: npm run type-check")
        print("   4. Tester toutes les notifications dans l'app")
        print("   5. git add . && git commit -m 'feat: migrate to SweetAlert2'")
        
        if BACKUP:
            print("\n💾 Backups créés (*.bak) - Supprimez-les après vérification")
    
    # Commandes de vérification
    print(f"\n🔍 Vérification rapide:\n")
    print("   # Combien de toast({ restent ?")
    print('   grep -r "toast({" src/ --include="*.tsx" | grep -v "sweetalert" | wc -l')
    print("\n   # Quels fichiers ont encore des toast({ ?")
    print('   grep -r "toast({" src/ --include="*.tsx" --files-with-matches | grep -v "sweetalert"')
    
    print(f"\n{'=' * 80}\n")

if __name__ == "__main__":
    main()
