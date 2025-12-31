import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Download, LayoutDashboard, Brain, Gauge } from 'lucide-react';
import { AIGovernanceOverview } from './components/AIGovernanceOverview';
import { AIDecisionsList } from './components/AIDecisionsList';
import { AIQuotasList } from './components/AIQuotasList';

export default function AIGovernance() {
  const [selectedTab, setSelectedTab] = useState<'overview' | 'decisions' | 'quotas'>('overview');

  return (
    <div className="container mx-auto p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">AI Governance</h1>
          <p className="text-muted-foreground mt-1">
            Manage AI usage, quotas, and decisions across all organizations
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
            Export Decisions
          </Button>
        </div>
      </div>

      <Tabs value={selectedTab} onValueChange={(v) => setSelectedTab(v as 'overview' | 'quotas' | 'decisions')}>
        <TabsList>
          <TabsTrigger value="overview">
            <LayoutDashboard className="h-4 w-4 mr-2" />
            Overview
          </TabsTrigger>
          <TabsTrigger value="decisions">
            <Brain className="h-4 w-4 mr-2" />
            AI Decisions
          </TabsTrigger>
          <TabsTrigger value="quotas">
            <Gauge className="h-4 w-4 mr-2" />
            Quotas
          </TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-6">
          <AIGovernanceOverview />
        </TabsContent>

        <TabsContent value="decisions" className="space-y-6">
          <AIDecisionsList />
        </TabsContent>

        <TabsContent value="quotas" className="space-y-6">
          <AIQuotasList />
        </TabsContent>
      </Tabs>
    </div>
  );
}

