import { Suspense } from "react";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import * as Sentry from "@sentry/react";
import { AuthProvider } from "./contexts/AuthContext";
import { ThemeProvider } from "./contexts/ThemeContext";
import { LanguageProvider } from "./contexts/LanguageContext";
import { FeatureFlagsProvider } from "./contexts/FeatureFlagsContext";
import { PermissionProvider } from "./contexts/PermissionContext";
import { FeatureFlagsGuard } from "./components/guards/FeatureFlagsGuard";
import { MainLayout } from "./components/layout/MainLayout";
import { RequireAdminGuard } from "./components/guards/RequireAdminGuard";
import { AdminLayout } from "./components/layout/AdminLayout";
import { ErrorFallback } from "./components/ErrorFallback";

// Auth pages
import Login from "./pages/auth/Login";
import Register from "./pages/auth/Register";
import ForgotPassword from "./pages/auth/ForgotPassword";
import ResetPassword from "./pages/auth/ResetPassword";
import AcceptInvite from "./pages/auth/AcceptInvite";
import Terms from "./pages/Terms";

// App pages
import Dashboard from "./pages/Dashboard";
import Projects from "./pages/Projects";
import ProjectDetail from "./pages/ProjectDetail";
import ProjectMembers from "./pages/ProjectMembers";
import ReleaseDetailPage from "./pages/ReleaseDetailPage";
import ReleaseHealthDashboard from "./pages/ReleaseHealthDashboard";
import Tasks from "./pages/Tasks";
import Sprints from "./pages/Sprints";
import Teams from "./pages/Teams";
import Metrics from "./pages/Metrics";
import Insights from "./pages/Insights";
import Agents from "./pages/Agents";
import Backlog from "./pages/Backlog";
import Defects from "./pages/Defects";
import Profile from "./pages/Profile";
import Users from "./pages/Users";
import QuotaDetails from "./pages/QuotaDetails";
import NotFound from "./pages/NotFound";

// Admin pages
import AdminDashboard from "./pages/admin/AdminDashboard";
import AdminUsers from "./pages/admin/AdminUsers";
import AdminPermissions from "./pages/admin/AdminPermissions";
import AdminSettings from "./pages/admin/AdminSettings";
import AdminSystemHealth from "./pages/admin/AdminSystemHealth";
import AdminAuditLogs from "./pages/admin/AdminAuditLogs";
import AIGovernance from "./pages/admin/AIGovernance";
import AdminAIQuota from "./pages/admin/AdminAIQuota";
import AdminOrganizations from "./pages/admin/AdminOrganizations";
import AdminOrganizationDetail from "./pages/admin/AdminOrganizationDetail";
import AdminMyOrganization from "./pages/admin/AdminMyOrganization";
import AdminOrganizationMembers from "./pages/admin/AdminOrganizationMembers";
import AdminMemberAIQuotas from "./pages/admin/AdminMemberAIQuotas";
import AdminMemberPermissions from "./pages/admin/AdminMemberPermissions";
import SuperAdminOrganizationAIQuota from "./pages/superadmin/SuperAdminOrganizationAIQuota";
import SuperAdminOrganizationPermissions from "./pages/superadmin/SuperAdminOrganizationPermissions";
import { RequireSuperAdminGuard } from "./components/guards/RequireSuperAdminGuard";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // 5 minutes
      retry: 1,
    },
  },
});

const App = () => (
  <Suspense fallback={<div>Loading...</div>}>
    <Sentry.ErrorBoundary fallback={ErrorFallback}>
      <QueryClientProvider client={queryClient}>
        <ThemeProvider>
          <AuthProvider>
            <LanguageProvider>
              <PermissionProvider>
                <FeatureFlagsProvider>
                <TooltipProvider>
                  <BrowserRouter
                    future={{
                      v7_startTransition: true,
                      v7_relativeSplatPath: true,
                    }}
                  >
                    <Routes>
                      {/* Public routes - don't require feature flags */}
                      <Route path="/login" element={<Login />} />
                      <Route path="/register" element={<Register />} />
                      <Route path="/forgot-password" element={<ForgotPassword />} />
                      <Route path="/reset-password/:token" element={<ResetPassword />} />
                      <Route path="/invite/accept/:token" element={<AcceptInvite />} />
                      <Route path="/terms" element={<Terms />} />

                      {/* Protected routes - require feature flags to be loaded */}
                      <Route
                        element={
                          <FeatureFlagsGuard>
                            <MainLayout />
                          </FeatureFlagsGuard>
                        }
                      >
                        <Route path="/" element={<Navigate to="/dashboard" replace />} />
                        <Route path="/dashboard" element={<Dashboard />} />
                        <Route path="/projects" element={<Projects />} />
                        <Route
                          path="/projects/:projectId/releases/:releaseId"
                          element={<ReleaseDetailPage />}
                        />
                        <Route
                          path="/projects/:projectId/releases/health"
                          element={<ReleaseHealthDashboard />}
                        />
                        <Route path="/projects/:id" element={<ProjectDetail />} />
                        <Route path="/projects/:id/members" element={<ProjectMembers />} />
                        <Route path="/tasks" element={<Tasks />} />
                        <Route path="/sprints" element={<Sprints />} />
                        <Route path="/backlog" element={<Backlog />} />
                        <Route path="/defects" element={<Defects />} />
                        <Route path="/profile" element={<Profile />} />
                        <Route path="/teams" element={<Teams />} />
                        <Route path="/users" element={<Users />} />
                        <Route path="/metrics" element={<Metrics />} />
                        <Route path="/insights" element={<Insights />} />
                        <Route path="/agents" element={<Agents />} />
                        <Route path="/settings/ai-quota" element={<QuotaDetails />} />
                      </Route>

                      {/* Admin routes - require feature flags to be loaded */}
                      <Route
                        path="/admin"
                        element={
                          <FeatureFlagsGuard>
                            <RequireAdminGuard>
                              <AdminLayout />
                            </RequireAdminGuard>
                          </FeatureFlagsGuard>
                        }
                      >
                        <Route index element={<Navigate to="/admin/dashboard" replace />} />
                        <Route path="dashboard" element={<AdminDashboard />} />
                        <Route path="users" element={<AdminUsers />} />
                        <Route path="permissions" element={<AdminPermissions />} />
                        <Route path="settings" element={<AdminSettings />} />
                        <Route path="audit-logs" element={<AdminAuditLogs />} />
                        <Route path="system-health" element={<AdminSystemHealth />} />
                        <Route path="ai-governance" element={<AIGovernance />} />
                        <Route path="ai-quota" element={<AdminAIQuota />} />
                        {/* SuperAdmin only routes */}
                        <Route
                          path="organizations"
                          element={
                            <RequireSuperAdminGuard>
                              <AdminOrganizations />
                            </RequireSuperAdminGuard>
                          }
                        />
                        <Route
                          path="organizations/:orgId"
                          element={
                            <RequireSuperAdminGuard>
                              <AdminOrganizationDetail />
                            </RequireSuperAdminGuard>
                          }
                        />
                        <Route
                          path="organizations/:orgId/ai-quota"
                          element={
                            <RequireSuperAdminGuard>
                              <SuperAdminOrganizationAIQuota />
                            </RequireSuperAdminGuard>
                          }
                        />
                        <Route
                          path="organizations/:orgId/permissions"
                          element={
                            <RequireSuperAdminGuard>
                              <SuperAdminOrganizationPermissions />
                            </RequireSuperAdminGuard>
                          }
                        />
                        {/* Admin own-org routes */}
                        <Route path="organization" element={<AdminMyOrganization />} />
                        <Route path="organization/members" element={<AdminOrganizationMembers />} />
                        <Route path="ai-quotas" element={<AdminMemberAIQuotas />} />
                        <Route path="permissions/members" element={<AdminMemberPermissions />} />
                      </Route>

                      {/* 404 */}
                      <Route path="*" element={<NotFound />} />
                    </Routes>
                  </BrowserRouter>
                </TooltipProvider>
              </FeatureFlagsProvider>
            </PermissionProvider>
            </LanguageProvider>
          </AuthProvider>
        </ThemeProvider>
      </QueryClientProvider>
    </Sentry.ErrorBoundary>
  </Suspense>
);

export default App;
