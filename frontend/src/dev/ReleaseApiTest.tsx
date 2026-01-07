// NOTE: This is a development/debug page for testing Release API endpoints.
// This file is located in /dev folder and is excluded from production builds.
// It is not routed in App.tsx and should only be used during development.
// To use it, temporarily add a route in App.tsx (with import.meta.env.DEV check) or access via direct import.
import React, { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { CheckCircle2, XCircle, MinusCircle, Loader2 } from 'lucide-react';
import { ReleaseApiConnectivityTester } from '@/utils/testReleaseApiConnectivity';

interface ResultItem {
  name: string;
  status: string;
  message?: string;
  endpoint?: string;
  method?: string;
  statusCode?: number;
  error?: string;
}

export const ReleaseApiTestPage: React.FC = () => {
  const [testing, setTesting] = useState(false);
  const [results, setResults] = useState<ResultItem[]>([]);
  const [summary, setSummary] = useState<{ success: number; failed: number; skipped: number; total: number } | null>(null);

  const runTests = async (createTestData: boolean) => {
    setTesting(true);
    setResults([]);
    setSummary(null);

    try {
      const tester = new ReleaseApiConnectivityTester();
      await tester.runAllTests(createTestData);
      const testResults = tester.getResults();

      setResults(testResults.map((r): ResultItem => ({
        name: r.endpoint || r.message,
        status: r.status,
        message: r.message,
        endpoint: r.endpoint,
        method: r.method,
        statusCode: r.statusCode,
        error: r.error,
      })));
      setSummary({
        total: testResults.length,
        success: testResults.filter(r => r.status === 'SUCCESS').length,
        failed: testResults.filter(r => r.status === 'FAIL').length,
        skipped: testResults.filter(r => r.status === 'SKIP').length
      });
    } catch (error) {
      // Test execution failed
    } finally {
      setTesting(false);
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'SUCCESS':
        return <CheckCircle2 className="h-4 w-4 text-green-500" />;
      case 'FAIL':
        return <XCircle className="h-4 w-4 text-red-500" />;
      default:
        return <MinusCircle className="h-4 w-4 text-gray-400" />;
    }
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'SUCCESS':
        return <Badge variant="default" className="bg-green-500">SUCCESS</Badge>;
      case 'FAIL':
        return <Badge variant="destructive">FAIL</Badge>;
      default:
        return <Badge variant="secondary">SKIP</Badge>;
    }
  };

  return (
    <div className="container mx-auto p-6 max-w-7xl">
      <div className="mb-6">
        <h1 className="text-3xl font-bold mb-2">🧪 Release API Connectivity Test</h1>
        <p className="text-muted-foreground">
          This page tests all 17 Release API endpoints to verify connectivity between frontend and backend.
          Open the browser console for detailed logs.
        </p>
      </div>

      <div className="flex gap-4 mb-6">
        <Button 
          onClick={() => runTests(false)} 
          disabled={testing}
          variant="default"
        >
          {testing ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Running Tests...
            </>
          ) : (
            'Run Read-Only Tests'
          )}
        </Button>
        <Button 
          onClick={() => runTests(true)} 
          disabled={testing}
          variant="destructive"
        >
          {testing ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Running Tests...
            </>
          ) : (
            'Run All Tests (Including Mutations)'
          )}
        </Button>
      </div>

      {summary && (
        <Card className="mb-6">
          <CardHeader>
            <CardTitle>Test Summary</CardTitle>
            <CardDescription>Overall test results</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="flex gap-6 items-center mb-4">
              <div>
                <div className="text-2xl font-bold">{summary.total}</div>
                <div className="text-sm text-muted-foreground">Total</div>
              </div>
              <div>
                <div className="text-2xl font-bold text-green-500">{summary.success}</div>
                <div className="text-sm text-muted-foreground">Success</div>
              </div>
              <div>
                <div className="text-2xl font-bold text-red-500">{summary.failed}</div>
                <div className="text-sm text-muted-foreground">Failed</div>
              </div>
              <div>
                <div className="text-2xl font-bold text-gray-400">{summary.skipped}</div>
                <div className="text-sm text-muted-foreground">Skipped</div>
              </div>
            </div>

            {summary.failed === 0 && summary.success > 0 && (
              <Alert>
                <CheckCircle2 className="h-4 w-4" />
                <AlertTitle>Success</AlertTitle>
                <AlertDescription>
                  All tested endpoints are working correctly!
                </AlertDescription>
              </Alert>
            )}

            {summary.failed > 0 && (
              <Alert variant="destructive">
                <XCircle className="h-4 w-4" />
                <AlertTitle>Some Tests Failed</AlertTitle>
                <AlertDescription>
                  {summary.failed} endpoint(s) failed. Check the table below for details.
                </AlertDescription>
              </Alert>
            )}
          </CardContent>
        </Card>
      )}

      {results.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Test Results</CardTitle>
            <CardDescription>Detailed results for each endpoint</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-[100px]">Status</TableHead>
                    <TableHead className="w-[80px]">Method</TableHead>
                    <TableHead>Endpoint</TableHead>
                    <TableHead className="w-[100px]">Status Code</TableHead>
                    <TableHead>Message</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {results.map((result, index) => (
                    <TableRow key={`${result.endpoint}-${index}`}>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          {getStatusIcon(result.status)}
                          {getStatusBadge(result.status)}
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">{result.method}</Badge>
                      </TableCell>
                      <TableCell>
                        <code className="text-xs bg-muted px-2 py-1 rounded">
                          {result.endpoint}
                        </code>
                      </TableCell>
                      <TableCell>
                        {result.statusCode ? (
                          <Badge variant="outline">{result.statusCode}</Badge>
                        ) : (
                          <span className="text-muted-foreground">-</span>
                        )}
                      </TableCell>
                      <TableCell>
                        <div>
                          <div>{result.message}</div>
                          {result.error && (
                            <div className="text-xs text-red-500 mt-1">
                              Error: {result.error}
                            </div>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          </CardContent>
        </Card>
      )}

      {results.length === 0 && !testing && (
        <Card>
          <CardContent className="py-12 text-center">
            <p className="text-muted-foreground">
              Click a button above to start testing the Release API endpoints.
            </p>
          </CardContent>
        </Card>
      )}
    </div>
  );
};

export default ReleaseApiTestPage;

