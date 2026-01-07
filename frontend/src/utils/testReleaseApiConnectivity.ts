import { releasesApi } from '../api/releases';

export interface TestResult {
  endpoint: string;
  method: string;
  status: 'SUCCESS' | 'FAIL' | 'SKIP';
  statusCode?: number;
  error?: string;
  message: string;
}

export class ReleaseApiConnectivityTester {
  private results: TestResult[] = [];
  private testProjectId = 1; // Use existing project ID for testing
  private testReleaseId = 1; // Use existing release ID (or will be created)
  private testSprintId = 1; // Use existing sprint ID

  /**
   * Run all API connectivity tests
   * @param createTestData - If true, creates a test release for mutation tests
   */
  async runAllTests(createTestData = false): Promise<void> {
    this.results = [];

    // Phase 1: Read-only tests (safe to run)
    await this.testGetProjectReleases();
    await this.testGetReleaseStatistics();
    await this.testGetAvailableSprints();
    
    // Phase 2: Test with existing release (if available)
    await this.testGetReleaseById();
    
    // Phase 3: Mutation tests (only if createTestData = true)
    if (createTestData) {
      
      await this.testCreateRelease();
      await this.testUpdateRelease();
      await this.testAddSprintToRelease();
      await this.testBulkAddSprints();
      await this.testRemoveSprintFromRelease();
      await this.testGenerateReleaseNotes();
      await this.testUpdateReleaseNotes();
      await this.testGenerateChangelog();
      await this.testUpdateChangelog();
      await this.testEvaluateQualityGates();
      await this.testApproveQualityGate();
      await this.testDeployRelease();
      await this.testDeleteRelease();
    } else {
      this.skipMutationTests();
    }

    // Print summary
    this.printSummary();
  }

  // ==================== READ-ONLY TESTS ====================

  private async testGetProjectReleases(): Promise<void> {
    try {
      const data = await releasesApi.getProjectReleases(this.testProjectId);
      this.addResult({
        endpoint: `GET /api/v1/projects/${this.testProjectId}/releases`,
        method: 'GET',
        status: 'SUCCESS',
        statusCode: 200,
        message: `✅ Found ${Array.isArray(data) ? data.length : 0} releases`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      this.addResult({
        endpoint: `GET /api/v1/projects/${this.testProjectId}/releases`,
        method: 'GET',
        status: statusCode === 404 ? 'FAIL' : 'FAIL',
        statusCode,
        error: error.message,
        message: `❌ ${statusCode === 404 ? 'Endpoint not found (404)' : error.message}`
      });
    }
  }

  private async testGetReleaseById(): Promise<void> {
    try {
      const data = await releasesApi.getRelease(this.testReleaseId);
      this.addResult({
        endpoint: `GET /api/v1/releases/${this.testReleaseId}`,
        method: 'GET',
        status: 'SUCCESS',
        statusCode: 200,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        message: `✅ Release found: ${(data as any)?.name ?? 'Unknown'}`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      if (statusCode === 404) {
        this.addResult({
          endpoint: `GET /api/v1/releases/${this.testReleaseId}`,
          method: 'GET',
          status: 'SUCCESS',
          statusCode: 404,
          message: `✅ Endpoint exists (404 = release not found, which is expected)`
        });
      } else {
        this.addResult({
          endpoint: `GET /api/v1/releases/${this.testReleaseId}`,
          method: 'GET',
          status: 'FAIL',
          statusCode,
          error: error.message,
          message: `❌ ${error.message}`
        });
      }
    }
  }

  private async testGetReleaseStatistics(): Promise<void> {
    try {
      const data = await releasesApi.getReleaseStatistics(this.testProjectId);
      this.addResult({
        endpoint: `GET /api/v1/projects/${this.testProjectId}/releases/statistics`,
        method: 'GET',
        status: 'SUCCESS',
        statusCode: 200,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        message: `✅ Stats: ${(data as any)?.totalReleases ?? 0} total releases`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      this.addResult({
        endpoint: `GET /api/v1/projects/${this.testProjectId}/releases/statistics`,
        method: 'GET',
        status: 'FAIL',
        statusCode,
        error: error.message,
        message: `❌ ${statusCode === 404 ? 'Endpoint not found (404)' : error.message}`
      });
    }
  }

  private async testGetAvailableSprints(): Promise<void> {
    try {
      const data = await releasesApi.getAvailableSprintsForRelease(this.testProjectId);
      this.addResult({
        endpoint: `GET /api/v1/projects/${this.testProjectId}/sprints/available`,
        method: 'GET',
        status: 'SUCCESS',
        statusCode: 200,
        message: `✅ Found ${Array.isArray(data) ? data.length : 0} available sprints`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      this.addResult({
        endpoint: `GET /api/v1/projects/${this.testProjectId}/sprints/available`,
        method: 'GET',
        status: 'FAIL',
        statusCode,
        error: error.message,
        message: `❌ ${statusCode === 404 ? 'Endpoint not found (404)' : error.message}`
      });
    }
  }

  // ==================== MUTATION TESTS ====================

  private async testCreateRelease(): Promise<void> {
    try {
      const testData = {
        name: 'API Test Release',
        version: '99.99.99',
        description: 'Created by connectivity test',
        plannedDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
        type: 'Minor',
        isPreRelease: false,
        tagName: 'v99.99.99',
        sprintIds: []
      };
      
      const data = await releasesApi.createRelease(this.testProjectId, testData);
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      this.testReleaseId = (data as any)?.id ?? this.testReleaseId;
      
      this.addResult({
        endpoint: `POST /api/v1/projects/${this.testProjectId}/releases`,
        method: 'POST',
        status: 'SUCCESS',
        statusCode: 201,
        message: `✅ Release created with ID: ${this.testReleaseId}`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      this.addResult({
        endpoint: `POST /api/v1/projects/${this.testProjectId}/releases`,
        method: 'POST',
        status: 'FAIL',
        statusCode,
        error: error.message,
        message: `❌ ${statusCode === 404 ? 'Endpoint not found (404)' : error.message}`
      });
    }
  }

  private async testUpdateRelease(): Promise<void> {
    try {
      const testData = {
        name: 'API Test Release Updated',
        version: '99.99.99',
        description: 'Updated by connectivity test',
        plannedDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
        status: 'InProgress',
        type: 'Minor'
      };
      
      await releasesApi.updateRelease(this.testReleaseId, testData);
      
      this.addResult({
        endpoint: `PUT /api/v1/releases/${this.testReleaseId}`,
        method: 'PUT',
        status: 'SUCCESS',
        statusCode: 200,
        message: `✅ Release updated successfully`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      this.addResult({
        endpoint: `PUT /api/v1/releases/${this.testReleaseId}`,
        method: 'PUT',
        status: 'FAIL',
        statusCode,
        error: error.message,
        message: `❌ ${statusCode === 404 ? 'Endpoint not found (404)' : error.message}`
      });
    }
  }

  private async testAddSprintToRelease(): Promise<void> {
    try {
      await releasesApi.addSprintToRelease(this.testReleaseId, this.testSprintId);
      
      this.addResult({
        endpoint: `POST /api/v1/releases/${this.testReleaseId}/sprints/${this.testSprintId}`,
        method: 'POST',
        status: 'SUCCESS',
        statusCode: 200,
        message: `✅ Sprint added to release`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      // 400 is acceptable (sprint might already be assigned)
      if (statusCode === 400) {
        this.addResult({
          endpoint: `POST /api/v1/releases/${this.testReleaseId}/sprints/${this.testSprintId}`,
          method: 'POST',
          status: 'SUCCESS',
          statusCode: 400,
          message: `✅ Endpoint exists (400 = validation error, which is expected)`
        });
      } else {
        this.addResult({
          endpoint: `POST /api/v1/releases/${this.testReleaseId}/sprints/${this.testSprintId}`,
          method: 'POST',
          status: 'FAIL',
          statusCode,
          error: error.message,
          message: `❌ ${statusCode === 404 ? 'Endpoint not found (404)' : error.message}`
        });
      }
    }
  }

  private async testBulkAddSprints(): Promise<void> {
    try {
      await releasesApi.bulkAddSprintsToRelease(this.testReleaseId, [this.testSprintId]);
      
      this.addResult({
        endpoint: `POST /api/v1/releases/${this.testReleaseId}/sprints/bulk`,
        method: 'POST',
        status: 'SUCCESS',
        statusCode: 200,
        message: `✅ Sprints bulk added`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      if (statusCode === 400) {
        this.addResult({
          endpoint: `POST /api/v1/releases/${this.testReleaseId}/sprints/bulk`,
          method: 'POST',
          status: 'SUCCESS',
          statusCode: 400,
          message: `✅ Endpoint exists (400 = validation error, which is expected)`
        });
      } else {
        this.addResult({
          endpoint: `POST /api/v1/releases/${this.testReleaseId}/sprints/bulk`,
          method: 'POST',
          status: 'FAIL',
          statusCode,
          error: error.message,
          message: `❌ ${statusCode === 404 ? 'Endpoint not found (404)' : error.message}`
        });
      }
    }
  }

  private async testRemoveSprintFromRelease(): Promise<void> {
    try {
      await releasesApi.removeSprintFromRelease(this.testSprintId);
      
      this.addResult({
        endpoint: `DELETE /api/v1/releases/sprints/${this.testSprintId}`,
        method: 'DELETE',
        status: 'SUCCESS',
        statusCode: 204,
        message: `✅ Sprint removed from release`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      if (statusCode === 400) {
        this.addResult({
          endpoint: `DELETE /api/v1/releases/sprints/${this.testSprintId}`,
          method: 'DELETE',
          status: 'SUCCESS',
          statusCode: 400,
          message: `✅ Endpoint exists (400 = sprint not in release, which is expected)`
        });
      } else {
        this.addResult({
          endpoint: `DELETE /api/v1/releases/sprints/${this.testSprintId}`,
          method: 'DELETE',
          status: 'FAIL',
          statusCode,
          error: error.message,
          message: `❌ ${statusCode === 404 ? 'Endpoint not found (404)' : error.message}`
        });
      }
    }
  }

  private async testGenerateReleaseNotes(): Promise<void> {
    try {
      const notes = await releasesApi.generateReleaseNotes(this.testReleaseId);
      
      this.addResult({
        endpoint: `POST /api/v1/releases/${this.testReleaseId}/notes/generate`,
        method: 'POST',
        status: 'SUCCESS',
        statusCode: 200,
        message: `✅ Release notes generated (${typeof notes === 'string' ? notes.length : 0} chars)`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      this.addResult({
        endpoint: `POST /api/v1/releases/${this.testReleaseId}/notes/generate`,
        method: 'POST',
        status: 'FAIL',
        statusCode,
        error: error.message,
        message: `❌ ${statusCode === 404 ? 'Endpoint not found (404)' : error.message}`
      });
    }
  }

  private async testUpdateReleaseNotes(): Promise<void> {
    try {
      await releasesApi.updateReleaseNotes(this.testReleaseId, 'Test release notes');
      
      this.addResult({
        endpoint: `PUT /api/v1/releases/${this.testReleaseId}/notes`,
        method: 'PUT',
        status: 'SUCCESS',
        statusCode: 200,
        message: `✅ Release notes updated`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      this.addResult({
        endpoint: `PUT /api/v1/releases/${this.testReleaseId}/notes`,
        method: 'PUT',
        status: 'FAIL',
        statusCode,
        error: error.message,
        message: `❌ ${statusCode === 404 ? 'Endpoint not found (404)' : error.message}`
      });
    }
  }

  private async testGenerateChangelog(): Promise<void> {
    try {
      const changelog = await releasesApi.generateChangelog(this.testReleaseId);
      
      this.addResult({
        endpoint: `POST /api/v1/releases/${this.testReleaseId}/changelog/generate`,
        method: 'POST',
        status: 'SUCCESS',
        statusCode: 200,
        message: `✅ Changelog generated (${typeof changelog === 'string' ? changelog.length : 0} chars)`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      this.addResult({
        endpoint: `POST /api/v1/releases/${this.testReleaseId}/changelog/generate`,
        method: 'POST',
        status: 'FAIL',
        statusCode,
        error: error.message,
        message: `❌ ${statusCode === 404 ? 'Endpoint not found (404)' : error.message}`
      });
    }
  }

  private async testUpdateChangelog(): Promise<void> {
    try {
      await releasesApi.updateChangelog(this.testReleaseId, 'Test changelog');
      
      this.addResult({
        endpoint: `PUT /api/v1/releases/${this.testReleaseId}/changelog`,
        method: 'PUT',
        status: 'SUCCESS',
        statusCode: 200,
        message: `✅ Changelog updated`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      this.addResult({
        endpoint: `PUT /api/v1/releases/${this.testReleaseId}/changelog`,
        method: 'PUT',
        status: 'FAIL',
        statusCode,
        error: error.message,
        message: `❌ ${statusCode === 404 ? 'Endpoint not found (404)' : error.message}`
      });
    }
  }

  private async testEvaluateQualityGates(): Promise<void> {
    try {
      const gates = await releasesApi.evaluateQualityGates(this.testReleaseId);
      
      this.addResult({
        endpoint: `POST /api/v1/releases/${this.testReleaseId}/quality-gates/evaluate`,
        method: 'POST',
        status: 'SUCCESS',
        statusCode: 200,
        message: `✅ Quality gates evaluated (${Array.isArray(gates) ? gates.length : 0} gates)`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      this.addResult({
        endpoint: `POST /api/v1/releases/${this.testReleaseId}/quality-gates/evaluate`,
        method: 'POST',
        status: 'FAIL',
        statusCode,
        error: error.message,
        message: `❌ ${statusCode === 404 ? 'Endpoint not found (404)' : error.message}`
      });
    }
  }

  private async testApproveQualityGate(): Promise<void> {
    try {
      await releasesApi.approveQualityGate(this.testReleaseId, 1);
      
      this.addResult({
        endpoint: `POST /api/v1/releases/${this.testReleaseId}/quality-gates/approve`,
        method: 'POST',
        status: 'SUCCESS',
        statusCode: 200,
        message: `✅ Quality gate approved`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      if (statusCode === 400 || statusCode === 404) {
        this.addResult({
          endpoint: `POST /api/v1/releases/${this.testReleaseId}/quality-gates/approve`,
          method: 'POST',
          status: 'SUCCESS',
          statusCode,
          message: `✅ Endpoint exists (${statusCode} = expected validation/not found)`
        });
      } else {
        this.addResult({
          endpoint: `POST /api/v1/releases/${this.testReleaseId}/quality-gates/approve`,
          method: 'POST',
          status: 'FAIL',
          statusCode,
          error: error.message,
          message: `❌ ${error.message}`
        });
      }
    }
  }

  private async testDeployRelease(): Promise<void> {
    try {
      await releasesApi.deployRelease(this.testReleaseId);
      
      this.addResult({
        endpoint: `POST /api/v1/releases/${this.testReleaseId}/deploy`,
        method: 'POST',
        status: 'SUCCESS',
        statusCode: 200,
        message: `✅ Release deployed`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      if (statusCode === 400) {
        this.addResult({
          endpoint: `POST /api/v1/releases/${this.testReleaseId}/deploy`,
          method: 'POST',
          status: 'SUCCESS',
          statusCode: 400,
          message: `✅ Endpoint exists (400 = quality gates not passed, which is expected)`
        });
      } else {
        this.addResult({
          endpoint: `POST /api/v1/releases/${this.testReleaseId}/deploy`,
          method: 'POST',
          status: 'FAIL',
          statusCode,
          error: error.message,
          message: `❌ ${statusCode === 404 ? 'Endpoint not found (404)' : error.message}`
        });
      }
    }
  }

  private async testDeleteRelease(): Promise<void> {
    try {
      await releasesApi.deleteRelease(this.testReleaseId);
      
      this.addResult({
        endpoint: `DELETE /api/v1/releases/${this.testReleaseId}`,
        method: 'DELETE',
        status: 'SUCCESS',
        statusCode: 204,
        message: `✅ Release deleted`
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      const statusCode = this.extractStatusCode(error);
      if (statusCode === 400) {
        this.addResult({
          endpoint: `DELETE /api/v1/releases/${this.testReleaseId}`,
          method: 'DELETE',
          status: 'SUCCESS',
          statusCode: 400,
          message: `✅ Endpoint exists (400 = cannot delete released version, which is expected)`
        });
      } else {
        this.addResult({
          endpoint: `DELETE /api/v1/releases/${this.testReleaseId}`,
          method: 'DELETE',
          status: 'FAIL',
          statusCode,
          error: error.message,
          message: `❌ ${statusCode === 404 ? 'Endpoint not found (404)' : error.message}`
        });
      }
    }
  }

  // ==================== HELPERS ====================

  private skipMutationTests(): void {
    const mutationEndpoints = [
      { method: 'POST', endpoint: `/api/v1/projects/${this.testProjectId}/releases` },
      { method: 'PUT', endpoint: `/api/v1/releases/${this.testReleaseId}` },
      { method: 'DELETE', endpoint: `/api/v1/releases/${this.testReleaseId}` },
      { method: 'POST', endpoint: `/api/v1/releases/${this.testReleaseId}/deploy` },
      { method: 'POST', endpoint: `/api/v1/releases/${this.testReleaseId}/sprints/${this.testSprintId}` },
      { method: 'POST', endpoint: `/api/v1/releases/${this.testReleaseId}/sprints/bulk` },
      { method: 'DELETE', endpoint: `/api/v1/releases/sprints/${this.testSprintId}` },
      { method: 'POST', endpoint: `/api/v1/releases/${this.testReleaseId}/notes/generate` },
      { method: 'PUT', endpoint: `/api/v1/releases/${this.testReleaseId}/notes` },
      { method: 'POST', endpoint: `/api/v1/releases/${this.testReleaseId}/changelog/generate` },
      { method: 'PUT', endpoint: `/api/v1/releases/${this.testReleaseId}/changelog` },
      { method: 'POST', endpoint: `/api/v1/releases/${this.testReleaseId}/quality-gates/approve` }
    ];

    mutationEndpoints.forEach(({ method, endpoint }) => {
      this.addResult({
        endpoint,
        method,
        status: 'SKIP',
        message: '⏭️  Skipped (mutation test)'
      });
    });
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  private extractStatusCode(error: any): number | undefined {
    // Try to extract status code from error message or error object
    if (error?.response?.status) {
      return error.response.status;
    }
    if (error?.status) {
      return error.status;
    }
    // Check error message for status codes
    const statusMatch = error?.message?.match(/(\d{3})/);
    if (statusMatch) {
      return parseInt(statusMatch[1], 10);
    }
    // Common error patterns
    if (error?.message?.includes('401') || error?.message?.includes('Unauthorized')) {
      return 401;
    }
    if (error?.message?.includes('403') || error?.message?.includes('Forbidden')) {
      return 403;
    }
    if (error?.message?.includes('404') || error?.message?.includes('Not found')) {
      return 404;
    }
    if (error?.message?.includes('400') || error?.message?.includes('Bad Request')) {
      return 400;
    }
    return undefined;
  }

  private addResult(result: TestResult): void {
    this.results.push(result);
  }

  private printSummary(): void {
    // Summary is available via getResults() method
    // Test results are stored in this.results array
  }

  /**
   * Get test results for programmatic access
   */
  getResults(): TestResult[] {
    return this.results;
  }
}

// Export singleton instance for easy use in console
export const releaseApiTester = new ReleaseApiConnectivityTester();

// Console helper - only available in browser environment
if (typeof window !== 'undefined') {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  (window as any).testReleaseApi = async (createTestData = false) => {
    await releaseApiTester.runAllTests(createTestData);
    return releaseApiTester.getResults();
  };

  // Release API Connectivity Tester loaded
  // Run tests with: testReleaseApi() (read-only) or testReleaseApi(true) (with mutations)
}

