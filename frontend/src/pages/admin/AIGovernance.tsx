import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Download, LayoutDashboard, Brain, Gauge } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { AIOverviewDashboard } from '@/components/admin/ai-governance/AIOverviewDashboard';
import { AIDecisionsList } from './components/AIDecisionsList';
import { AIQuotasList } from './components/AIQuotasList';

export default function AIGovernance() {
  const { t } = useTranslation('admin');
  const [selectedTab, setSelectedTab] = useState<'overview' | 'decisions' | 'quotas'>('overview');

  return (
    <div className="container mx-auto p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">{t('aiGovernance.title')}</h1>
          <p className="text-muted-foreground mt-1">
            {t('aiGovernance.description')}
          </p>
        </div>

        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            onClick={() => {
              const url = `${import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001'}/api/admin/ai/decisions/export`;
              window.open(url, '_blank');
            }}
          >
            <Download className="h-4 w-4 mr-2" />
            {t('aiGovernance.actions.exportDecisions')}
          </Button>
        </div>
      </div>

      <Tabs value={selectedTab} onValueChange={(v) => setSelectedTab(v as 'overview' | 'quotas' | 'decisions')}>
        <TabsList>
          <TabsTrigger value="overview">
            <LayoutDashboard className="h-4 w-4 mr-2" />
            {t('aiGovernance.tabs.overview')}
          </TabsTrigger>
          <TabsTrigger value="decisions">
            <Brain className="h-4 w-4 mr-2" />
            {t('aiGovernance.tabs.decisions')}
          </TabsTrigger>
          <TabsTrigger value="quotas">
            <Gauge className="h-4 w-4 mr-2" />
            {t('aiGovernance.tabs.quotas')}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-6">
          <AIOverviewDashboard />
        </TabsContent>

        <TabsContent value="decisions" className="space-y-6">
          <AIDecisionsList />
        </TabsContent>

        <TabsContent value="quotas" className="space-y-6">
          <div className="flex items-center justify-between mb-4">
            <p className="text-sm text-muted-foreground">
              {t('aiGovernance.quotas.description')}{' '}
              <Link
                to="/settings/ai-quota"
                className="text-blue-600 hover:underline dark:text-blue-400"
              >
                {t('aiGovernance.quotas.settingsLink')}
              </Link>
              .
            </p>
          </div>
          <AIQuotasList />
        </TabsContent>
      </Tabs>
    </div>
  );
}

