import { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { auditLogsApi, type AuditLogDto } from '@/api/auditLogs';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { showToast, showError } from "@/lib/sweetalert";
import { Search, Download } from 'lucide-react';
import { format } from 'date-fns';
import { Pagination } from '@/components/ui/pagination';

const ENTITY_TYPES = ['User', 'Project', 'Setting', 'FeatureFlag', 'Permission', 'Organization'];
const ACTIONS = ['create', 'update', 'delete', 'login', 'logout', 'invite', 'activate', 'deactivate'];

export default function AdminAuditLogs() {
  const { t } = useTranslation('admin');
  const [currentPage, setCurrentPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const [actionFilter, setActionFilter] = useState<string>('all');
  const [entityTypeFilter, setEntityTypeFilter] = useState<string>('all');
  const pageSize = 20;

  const { data, isLoading, error } = useQuery({
    queryKey: ['audit-logs', currentPage, pageSize, actionFilter, entityTypeFilter],
    queryFn: () =>
      auditLogsApi.getAll(
        currentPage,
        pageSize,
        actionFilter !== 'all' ? actionFilter : undefined,
        entityTypeFilter !== 'all' ? entityTypeFilter : undefined
      ),
  });

  // Client-side search filter
  const filteredLogs = useMemo(() => {
    if (!data?.items) return [];
    if (!searchQuery.trim()) return data.items;

    const query = searchQuery.toLowerCase();
    return data.items.filter(
      (log: AuditLogDto) =>
        log.action?.toLowerCase().includes(query) ||
        log.entityType?.toLowerCase().includes(query) ||
        log.entityName?.toLowerCase().includes(query) ||
        log.userName?.toLowerCase().includes(query) ||
        log.ipAddress?.toLowerCase().includes(query)
    );
  }, [data?.items, searchQuery]);

  const handleExportCSV = () => {
    if (!data?.items || data.items.length === 0) {
      showError(t('auditLogs.export.noData'), t('auditLogs.export.noDataMessage'));
      return;
    }

    // Enhanced CSV export with more columns
    const headers = [
      'ID',
      'Timestamp',
      'User ID',
      'User Name',
      'Action',
      'Entity Type',
      'Entity ID',
      'Entity Name',
      'IP Address',
      'User Agent',
      'Changes',
    ];
    
    const rows = data.items.map((log: AuditLogDto) => [
      log.id.toString(),
      format(new Date(log.createdAt), 'yyyy-MM-dd HH:mm:ss'),
      log.userId?.toString() || '',
      log.userName || t('auditLogs.table.system'),
      log.action,
      log.entityType,
      log.entityId?.toString() || '',
      log.entityName || '-',
      log.ipAddress || '-',
      log.userAgent || '-',
      log.changes || '-',
    ]);

    // Add BOM for Excel compatibility
    const BOM = '\uFEFF';
    const csvContent = BOM + [
      headers.join(','),
      ...rows.map((row) => row.map((cell) => `"${String(cell).replace(/"/g, '""')}"`).join(',')),
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', `audit_logs_export_${format(new Date(), 'yyyy-MM-dd_HHmmss')}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    showToast(t('auditLogs.export.success', { count: data.items.length }), 'success');
  };

  const getActionBadgeVariant = (action: string) => {
    if (action.toLowerCase().includes('delete') || action.toLowerCase().includes('deactivate')) {
      return 'destructive';
    }
    if (action.toLowerCase().includes('create') || action.toLowerCase().includes('invite')) {
      return 'default';
    }
    return 'secondary';
  };

  if (error) {
    return (
      <div className="container mx-auto p-6 text-center text-destructive">
        <p>{t('auditLogs.errors.loadError', { error: error instanceof Error ? error.message : 'Unknown error' })}</p>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold mb-2">{t('auditLogs.title')}</h1>
        <p className="text-muted-foreground">
          {t('auditLogs.description')}
        </p>
      </div>

      {/* Filters */}
      <div className="mb-4 flex items-center gap-4 flex-wrap">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            id="search-audit-logs"
            name="search"
            placeholder={t('auditLogs.filters.searchPlaceholder')}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-9"
          />
        </div>

        <Select value={actionFilter} onValueChange={setActionFilter}>
          <SelectTrigger className="w-[150px]" id="action-filter" name="action">
            <SelectValue placeholder={t('auditLogs.filters.allActions')} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t('auditLogs.filters.allActions')}</SelectItem>
            {ACTIONS.map((action) => (
              <SelectItem key={action} value={action}>
                {action.charAt(0).toUpperCase() + action.slice(1)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={entityTypeFilter} onValueChange={setEntityTypeFilter}>
          <SelectTrigger className="w-[150px]" id="entity-type-filter" name="entityType">
            <SelectValue placeholder={t('auditLogs.filters.allEntities')} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t('auditLogs.filters.allEntities')}</SelectItem>
            {ENTITY_TYPES.map((type) => (
              <SelectItem key={type} value={type}>
                {type}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Button variant="outline" onClick={handleExportCSV}>
          <Download className="mr-2 h-4 w-4" />
          {t('auditLogs.actions.exportCSV')}
        </Button>
      </div>

      {/* Table */}
      <div className="bg-card rounded-lg border">
        {isLoading ? (
          <div className="p-6 space-y-4">
            {[...Array(5)].map((_, i) => (
              <Skeleton key={i} className="h-12 w-full" />
            ))}
          </div>
        ) : filteredLogs.length === 0 ? (
          <div className="p-12 text-center">
            <p className="text-muted-foreground">
              {searchQuery ? t('auditLogs.table.noLogsMatching') : t('auditLogs.table.noLogs')}
            </p>
          </div>
        ) : (
          <>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>{t('auditLogs.table.headers.timestamp')}</TableHead>
                  <TableHead>{t('auditLogs.table.headers.user')}</TableHead>
                  <TableHead>{t('auditLogs.table.headers.action')}</TableHead>
                  <TableHead>{t('auditLogs.table.headers.entityType')}</TableHead>
                  <TableHead>{t('auditLogs.table.headers.entityName')}</TableHead>
                  <TableHead>{t('auditLogs.table.headers.ipAddress')}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredLogs.map((log: AuditLogDto) => (
                  <TableRow key={log.id}>
                    <TableCell className="text-muted-foreground">
                      {format(new Date(log.createdAt), 'MMM d, yyyy HH:mm:ss')}
                    </TableCell>
                    <TableCell>
                      {log.userName ? (
                        <span className="font-medium">{log.userName}</span>
                      ) : (
                        <span className="text-muted-foreground italic">{t('auditLogs.table.system')}</span>
                      )}
                    </TableCell>
                    <TableCell>
                      <Badge variant={getActionBadgeVariant(log.action)}>
                        {log.action}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Badge variant="outline">{log.entityType}</Badge>
                    </TableCell>
                    <TableCell>{log.entityName || '-'}</TableCell>
                    <TableCell className="text-muted-foreground font-mono text-xs">
                      {log.ipAddress || '-'}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>

            {data && (data.totalPages ?? 0) > 1 && (
              <div className="p-4 border-t">
                <Pagination
                  currentPage={currentPage}
                  totalPages={data.totalPages ?? 1}
                  onPageChange={setCurrentPage}
                />
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}

