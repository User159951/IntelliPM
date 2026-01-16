export interface paths {
    "/api/v1/Activity/recent": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get recent activities for user's projects */
        get: {
            parameters: {
                query?: {
                    limit?: number;
                    projectId?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Activity.Queries.GetRecentActivityResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Activity.Queries.GetRecentActivityResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Activity.Queries.GetRecentActivityResponse"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai/quota/{organizationId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * Update AI quota for an organization (Admin only).
         *     Allows administrators to modify quota limits, change tiers, and configure overage settings.
         */
        put: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description Organization ID */
                    organizationId: number;
                };
                cookie?: never;
            };
            /** @description Quota update request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.UpdateQuotaRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.UpdateQuotaRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.UpdateQuotaRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.UpdateAIQuotaResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.UpdateAIQuotaResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.UpdateAIQuotaResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai/disable/{organizationId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Disable AI for an organization (Kill switch).
         *     Emergency kill switch to immediately disable all AI features for an organization.
         */
        post: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description Organization ID */
                    organizationId: number;
                };
                cookie?: never;
            };
            /** @description Disable request with reason and options */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.DisableAIRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.DisableAIRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.DisableAIRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.DisableAIForOrgResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.DisableAIForOrgResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.DisableAIForOrgResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai/enable/{organizationId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Enable AI for an organization.
         *     Re-enables AI features for an organization that was previously disabled.
         */
        post: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description Organization ID */
                    organizationId: number;
                };
                cookie?: never;
            };
            /** @description Enable request with tier and reason */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.EnableAIRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.EnableAIRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.EnableAIRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.EnableAIForOrgResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.EnableAIForOrgResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.EnableAIForOrgResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai/quotas": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get all AI quotas (Admin only).
         *     Returns paginated list of all organization quotas with filtering options.
         */
        get: {
            parameters: {
                query?: {
                    /** @description Filter by tier name */
                    tierName?: string;
                    /** @description Filter by active status */
                    isActive?: boolean;
                    /** @description Filter by exceeded status */
                    isExceeded?: boolean;
                    /** @description Page number (default: 1) */
                    page?: number;
                    /** @description Page size (default: 20, max: 100) */
                    pageSize?: number;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.AIQuotaDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.AIQuotaDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.AIQuotaDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai/decisions/all": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get AI decisions across all organizations (Admin only).
         *     Returns paginated list of decisions from all organizations with filtering options.
         */
        get: {
            parameters: {
                query?: {
                    /** @description Filter by organization ID */
                    organizationId?: number;
                    /** @description Filter by decision type */
                    decisionType?: string;
                    /** @description Filter by agent type */
                    agentType?: string;
                    /** @description Start date for filtering */
                    startDate?: string;
                    /** @description End date for filtering */
                    endDate?: string;
                    /** @description Page number (default: 1) */
                    page?: number;
                    /** @description Page size (default: 20, max: 100) */
                    pageSize?: number;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.AIDecisionLogDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.AIDecisionLogDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.AIDecisionLogDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai/decisions/export": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Export AI decisions to CSV (Admin only).
         *     Generates a CSV file with all AI decisions for compliance and reporting purposes.
         *     Admin can only export their own organization; SuperAdmin can export any organization or all.
         */
        get: {
            parameters: {
                query?: {
                    /** @description Filter by organization ID (optional for SuperAdmin, required for Admin) */
                    organizationId?: number;
                    /** @description Start date for export (default: 30 days ago) */
                    startDate?: string;
                    /** @description End date for export (default: now) */
                    endDate?: string;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": string;
                        "application/json; version=1.0": string;
                        "text/json; version=1.0": string;
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai/overview/stats": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get AI overview statistics aggregated across all organizations (Admin only).
         *     Returns comprehensive statistics including organization counts, decision metrics, top agents, and quota breakdown.
         */
        get: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.AIOverviewStatsDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.AIOverviewStatsDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.AIOverviewStatsDto"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai/global/toggle": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Toggle global AI kill switch (SuperAdmin only).
         *     Emergency kill switch to immediately disable/enable all AI features system-wide.
         *     This affects all organizations regardless of their individual settings.
         */
        post: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            /** @description Toggle request with enabled status and reason */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.ToggleGlobalAIRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.ToggleGlobalAIRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.ToggleGlobalAIRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.ToggleGlobalAIResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.ToggleGlobalAIResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.ToggleGlobalAIResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai/global/status": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get global AI kill switch status (SuperAdmin only).
         *     Returns the current state of the global AI kill switch.
         */
        get: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.Queries.GlobalAIStatusResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Queries.GlobalAIStatusResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Queries.GlobalAIStatusResponse"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai-quota/members": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get paginated list of organization members with their AI quota information.
         *     For SuperAdmin: can filter by organizationId. For Admin: uses their own organization.
         */
        get: {
            parameters: {
                query?: {
                    /** @description Optional organization ID (SuperAdmin only). If not provided, uses current user's organization. */
                    organizationId?: number;
                    /** @description Page number (default: 1) */
                    page?: number;
                    /** @description Page size (default: 20, max: 100) */
                    pageSize?: number;
                    /** @description Search by email or name (optional) */
                    searchTerm?: string;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.AdminAiQuotaMemberDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.AdminAiQuotaMemberDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.AdminAiQuotaMemberDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai-quota/members/{userId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /** Update or create a user AI quota override. */
        put: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description User ID */
                    userId: number;
                };
                cookie?: never;
            };
            /** @description Quota override request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.UpdateMemberQuotaRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.UpdateMemberQuotaRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.UpdateMemberQuotaRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.UpdateUserAIQuotaOverrideResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.UpdateUserAIQuotaOverrideResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.UpdateUserAIQuotaOverrideResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai-quota/members/{userId}/reset": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Reset (delete) a user AI quota override, reverting to organization default. */
        post: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description User ID */
                    userId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.ResetUserAIQuotaOverrideResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.ResetUserAIQuotaOverrideResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Commands.ResetUserAIQuotaOverrideResponse"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai-quota/ai-quotas/members": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get paginated list of organization members with their effective AI quotas (new model).
         *     Uses OrganizationAIQuota and UserAIQuota entities.
         */
        get: {
            parameters: {
                query?: {
                    /** @description Page number (default: 1) */
                    page?: number;
                    /** @description Page size (default: 20, max: 100) */
                    pageSize?: number;
                    /** @description Search by email or name (optional) */
                    searchTerm?: string;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.DTOs.MemberAIQuotaDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.DTOs.MemberAIQuotaDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.DTOs.MemberAIQuotaDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai-quota/ai-quotas/members/{userId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * Update or create a user AI quota override (new model).
         *     Uses OrganizationAIQuota and UserAIQuota entities.
         *     Validates that override values don't exceed organization limits.
         */
        put: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description User ID */
                    userId: number;
                };
                cookie?: never;
            };
            /** @description Quota override request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.UpdateMemberAIQuotaRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.UpdateMemberAIQuotaRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.UpdateMemberAIQuotaRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.MemberAIQuotaDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.MemberAIQuotaDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.MemberAIQuotaDto"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai-quota/usage-history": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get AI quota usage history for admin dashboard.
         *     Returns daily usage data aggregated from AIDecisionLog with pagination support.
         */
        get: {
            parameters: {
                query?: {
                    /** @description Organization ID (optional - if not provided, returns data for all organizations) */
                    organizationId?: number;
                    /** @description Start date for history (default: 30 days ago) */
                    startDate?: string;
                    /** @description End date for history (default: now) */
                    endDate?: string;
                    /** @description Page number (default: 1, minimum: 1) */
                    page?: number;
                    /** @description Page size (default: 20, minimum: 1, maximum: 100) */
                    pageSize?: number;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.DailyUsageHistoryDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.DailyUsageHistoryDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.DailyUsageHistoryDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai-quota/breakdown": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get AI quota breakdown by agent type and decision type.
         *     Provides detailed breakdown for admin dashboard.
         */
        get: {
            parameters: {
                query?: {
                    /** @description Organization ID (optional - if not provided, returns data for all organizations) */
                    organizationId?: number;
                    /** @description Period for breakdown: "day", "week", "month" (default: "month") */
                    period?: string;
                    /** @description Start date for breakdown (optional - defaults based on period) */
                    startDate?: string;
                    /** @description End date for breakdown (optional - defaults to now) */
                    endDate?: string;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.Queries.AIQuotaBreakdownDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Queries.AIQuotaBreakdownDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Queries.AIQuotaBreakdownDto"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/permissions/members": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get a paginated list of organization members with their permissions (Admin only). */
        get: {
            parameters: {
                query?: {
                    /** @description Page number (default: 1) */
                    page?: number;
                    /** @description Page size (default: 20, max: 100) */
                    pageSize?: number;
                    /** @description Search term for filtering by name or email */
                    searchTerm?: string;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Permissions.DTOs.MemberPermissionDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Permissions.DTOs.MemberPermissionDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Permissions.DTOs.MemberPermissionDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/permissions/members/{userId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * Update a member's role and/or permissions (Admin only - own organization).
         *     Enforces organization permission policy: assigned permissions must be subset of org allowed permissions.
         */
        put: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description User ID */
                    userId: number;
                };
                cookie?: never;
            };
            /** @description Permission update request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Permissions.DTOs.UpdateMemberPermissionRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Permissions.DTOs.UpdateMemberPermissionRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.Application.Permissions.DTOs.UpdateMemberPermissionRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Permissions.DTOs.MemberPermissionDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Permissions.DTOs.MemberPermissionDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Permissions.DTOs.MemberPermissionDto"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Agent/improve-task": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Improves a messy task description using AI with automatic function calling
         * @description Sample request:
         *
         *         POST /api/v1/Agent/improve-task
         *         {
         *            "description": "fix bug in login"
         *         }
         *
         *     The AI will improve the description to be more detailed and professional.
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Task description to improve */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ImproveTaskRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ImproveTaskRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ImproveTaskRequest"];
                };
            };
            responses: {
                /** @description Task description improved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                    };
                };
                /** @description Bad request - Description is empty or too long (max 5000 characters) */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description AI is disabled for the organization */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description AI quota exceeded */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Agent/analyze-risks/{projectId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Analyzes project risks using AI */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID to analyze for risks */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Risk analysis completed successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description AI is disabled for the organization */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Project not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description AI quota exceeded */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Agent/audit-log": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Gets paginated audit log of all agent executions */
        get: {
            parameters: {
                query?: {
                    /** @description Page number (default: 1, minimum: 1) */
                    page?: number;
                    /** @description Number of items per page (default: 50, minimum: 1, maximum: 100) */
                    pageSize?: number;
                    /** @description Optional filter by agent ID */
                    agentId?: string;
                    /** @description Optional filter by agent type (e.g., DeliveryAgent, ProductAgent) */
                    agentType?: string;
                    /** @description Optional filter by user ID */
                    userId?: string;
                    /** @description Optional filter by status (Pending, Success, Error) */
                    status?: string;
                    /** @description Optional filter by success status (true/false) */
                    success?: boolean;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Returns the paginated audit log */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Agent.Queries.GetAgentAuditLogsResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Agent.Queries.GetAgentAuditLogsResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Agent.Queries.GetAgentAuditLogsResponse"];
                    };
                };
                /** @description Bad request - Invalid pagination parameters */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Agent/metrics": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Gets agent execution statistics and metrics */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Returns the agent metrics */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Agent.Queries.AgentMetricsDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Agent.Queries.AgentMetricsDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Agent.Queries.AgentMetricsDto"];
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Agent/analyze-project/{projectId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Analyze project using AI agent */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID to analyze */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Analysis completed successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                    };
                };
                /** @description Project not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Error during analysis */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Agent/detect-risks/{projectId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Detect risks in a project using AI agent */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID to analyze for risks */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Risk detection completed successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                    };
                };
                /** @description Project not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Error during risk detection */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Agent/plan-sprint/{sprintId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Plan sprint using AI agent */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Sprint ID to plan */
                    sprintId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Sprint planning completed successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                    };
                };
                /** @description Sprint not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Error during sprint planning */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Agent/analyze-dependencies/{projectId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Analyze task dependencies for a project using AI agent */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID to analyze dependencies */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Dependency analysis completed successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                    };
                };
                /** @description Project not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Error during dependency analysis */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Agent/generate-retrospective/{sprintId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Generate sprint retrospective using AI agent */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Sprint ID to generate retrospective for */
                    sprintId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Retrospective generated successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.DTOs.Agent.AgentResponse"];
                    };
                };
                /** @description Sprint is not completed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Sprint not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Error during retrospective generation */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/agents/run-product": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Run Product Agent for project analysis
         * @description Analyzes the project from a product perspective, providing insights on features, user stories, and product strategy.
         *     The agent uses AI to analyze project data and generate recommendations.
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID to analyze */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Agent execution completed successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": unknown;
                        "application/json; version=1.0": unknown;
                        "text/json; version=1.0": unknown;
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User does not have permission or AI is disabled */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Project not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/agents/run-delivery": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Run Delivery Agent for project analysis
         * @description Analyzes the project from a delivery perspective, focusing on sprint planning, velocity, and delivery timelines.
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID to analyze */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Agent execution completed successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": unknown;
                        "application/json; version=1.0": unknown;
                        "text/json; version=1.0": unknown;
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User does not have permission or AI is disabled */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Project not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/agents/run-manager": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Run Manager Agent for project analysis
         * @description Analyzes the project from a management perspective, providing insights on resource allocation, risks, and key decisions.
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID to analyze */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Agent execution completed successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": unknown;
                        "application/json; version=1.0": unknown;
                        "text/json; version=1.0": unknown;
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User does not have permission or AI is disabled */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Project not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/agents/run-qa": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Run QA Agent for project analysis
         * @description Analyzes the project from a quality assurance perspective, identifying defects, test coverage, and quality metrics.
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID to analyze */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Agent execution completed successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": unknown;
                        "application/json; version=1.0": unknown;
                        "text/json; version=1.0": unknown;
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User does not have permission or AI is disabled */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Project not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/agents/run-business": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Run Business Agent for project analysis
         * @description Analyzes the project from a business perspective, providing insights on ROI, business value, and strategic alignment.
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID to analyze */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Agent execution completed successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": unknown;
                        "application/json; version=1.0": unknown;
                        "text/json; version=1.0": unknown;
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User does not have permission or AI is disabled */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Project not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/agents/notes": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Store a note for an AI agent
         * @description Sample request:
         *
         *         POST /api/v1/projects/{projectId}/agents/notes
         *         {
         *            "type": "Decision",
         *            "content": "We decided to use React for the frontend"
         *         }
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            /** @description Note type and content */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.StoreNoteRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.StoreNoteRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.StoreNoteRequest"];
                };
            };
            responses: {
                /** @description Note stored successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": unknown;
                        "application/json; version=1.0": unknown;
                        "text/json; version=1.0": unknown;
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User does not have permission */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/ai/decisions": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get AI decision logs for the current organization.
         *     Supports filtering by decision type, agent type, entity, date range, and approval status.
         */
        get: {
            parameters: {
                query?: {
                    /** @description Filter by decision type (e.g., "RiskDetection", "SprintPlanning") */
                    decisionType?: string;
                    /** @description Filter by agent type (e.g., "DeliveryAgent", "ProductAgent") */
                    agentType?: string;
                    /** @description Filter by entity type (e.g., "Project", "Sprint", "Task") */
                    entityType?: string;
                    /** @description Filter by specific entity ID */
                    entityId?: number;
                    /** @description Start date for filtering decisions */
                    startDate?: string;
                    /** @description End date for filtering decisions */
                    endDate?: string;
                    /** @description Filter by approval requirement status */
                    requiresApproval?: boolean;
                    /** @description Page number (default: 1) */
                    page?: number;
                    /** @description Page size (default: 20, max: 100) */
                    pageSize?: number;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.AIDecisionLogDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.AIDecisionLogDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.AIDecisionLogDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/ai/decisions/{decisionId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get specific AI decision by ID.
         *     Returns detailed information including reasoning, input/output data, and approval status.
         */
        get: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description Unique decision identifier (GUID) */
                    decisionId: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.Queries.AIDecisionLogDetailDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Queries.AIDecisionLogDetailDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Queries.AIDecisionLogDetailDto"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/ai/decisions/{decisionId}/approve": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Approve an AI decision (human-in-the-loop).
         *     Allows authorized users to approve decisions that require human approval.
         */
        post: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description Unique decision identifier (GUID) */
                    decisionId: string;
                };
                cookie?: never;
            };
            /** @description Approval request with optional notes */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ApproveDecisionRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ApproveDecisionRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ApproveDecisionRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/ai/decisions/{decisionId}/reject": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Reject an AI decision.
         *     Allows authorized users to reject decisions that require human approval.
         */
        post: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description Unique decision identifier (GUID) */
                    decisionId: string;
                };
                cookie?: never;
            };
            /** @description Rejection request with reason and optional notes */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.RejectDecisionRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.RejectDecisionRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.RejectDecisionRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/ai/quota": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get current AI quota status for organization.
         *     Returns real-time quota usage, limits, and status information.
         */
        get: {
            parameters: {
                query?: {
                    /** @description Optional organization ID. If not provided, uses current user's organization. */
                    organizationId?: number;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.Queries.AIQuotaStatusDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Queries.AIQuotaStatusDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Queries.AIQuotaStatusDto"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/ai/usage/statistics": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get AI usage statistics for current organization.
         *     Provides aggregated usage data including tokens, requests, decisions, and costs.
         */
        get: {
            parameters: {
                query?: {
                    /** @description Start date for statistics (default: 30 days ago) */
                    startDate?: string;
                    /** @description End date for statistics (default: now) */
                    endDate?: string;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.Queries.AIUsageStatisticsDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Queries.AIUsageStatisticsDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.Queries.AIUsageStatisticsDto"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/ai/executions": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get agent execution logs for performance monitoring and analysis.
         *     Returns detailed execution logs with performance metrics, success/failure status, and token usage.
         */
        get: {
            parameters: {
                query?: {
                    /** @description Filter by agent ID (e.g., "delivery-agent") */
                    agentId?: string;
                    /** @description Filter by agent type (e.g., "DeliveryAgent", "ProductAgent") */
                    agentType?: string;
                    /** @description Filter by user ID */
                    userId?: string;
                    /** @description Filter by status (e.g., "Success", "Error") */
                    status?: string;
                    /** @description Filter by success status (true/false) */
                    success?: boolean;
                    /** @description Page number (default: 1) */
                    page?: number;
                    /** @description Page size (default: 20, max: 100) */
                    pageSize?: number;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Agent.Queries.GetAgentAuditLogsResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Agent.Queries.GetAgentAuditLogsResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Agent.Queries.GetAgentAuditLogsResponse"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai-quota-templates": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all AI quota templates. */
        get: {
            parameters: {
                query?: {
                    /** @description If true, only return active templates (default: true) */
                    activeOnly?: boolean;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.AIQuotaTemplateDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.AIQuotaTemplateDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.AIQuotaTemplateDto"][];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /** Create a new AI quota template. */
        post: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            /** @description Template creation request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.CreateAIQuotaTemplateRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.CreateAIQuotaTemplateRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.CreateAIQuotaTemplateRequest"];
                };
            };
            responses: {
                /** @description Created */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.AIQuotaTemplateDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.AIQuotaTemplateDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.AIQuotaTemplateDto"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/ai-quota-templates/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * Update an existing AI quota template.
         *     System templates can be updated but their TierName cannot be changed.
         */
        put: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description Template ID */
                    id: number;
                };
                cookie?: never;
            };
            /** @description Template update request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.UpdateAIQuotaTemplateRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.UpdateAIQuotaTemplateRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.UpdateAIQuotaTemplateRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.AIQuotaTemplateDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.AIQuotaTemplateDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.AIQuotaTemplateDto"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Alerts": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get alerts for the current user */
        get: {
            parameters: {
                query?: {
                    /** @description Filter to show only unread alerts (default: true) */
                    unreadOnly?: boolean;
                    /** @description Maximum number of alerts to return (default: 10, max: 50) */
                    limit?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Alerts retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": unknown;
                        "application/json; version=1.0": unknown;
                        "text/json; version=1.0": unknown;
                    };
                };
                /** @description Error retrieving alerts */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Alerts/{id}/read": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Mark an alert as read */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Alert ID */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Alert marked as read successfully */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Alert not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Error updating alert */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Attachments": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all attachments for a specific entity */
        get: {
            parameters: {
                query?: {
                    /** @description Type of entity (Task, Project, Comment, Defect) */
                    entityType?: string;
                    /** @description ID of the entity */
                    entityId?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Attachments retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Attachments.Queries.AttachmentDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Attachments.Queries.AttachmentDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Attachments.Queries.AttachmentDto"][];
                    };
                };
                /** @description Bad request - Invalid entity type or ID */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Attachments/upload": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Upload a new attachment to an entity */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "multipart/form-data": {
                        ContentType?: string;
                        ContentDisposition?: string;
                        Headers?: {
                            [key: string]: string[];
                        };
                        /** Format: int64 */
                        Length?: number;
                        Name?: string;
                        FileName?: string;
                        entityType?: string;
                        /** Format: int32 */
                        entityId?: number;
                    };
                };
            };
            responses: {
                /** @description Attachment uploaded successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Attachments.Queries.AttachmentDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Attachments.Queries.AttachmentDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Attachments.Queries.AttachmentDto"];
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Entity not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Attachments/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Download an attachment by ID */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Attachment ID */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description File downloaded successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": string;
                        "application/json; version=1.0": string;
                        "text/json; version=1.0": string;
                    };
                };
                /** @description Attachment not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        /** Delete an attachment (soft delete) */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Attachment ID */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Attachment deleted successfully */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Unauthorized - Not the uploader */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Attachment not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/audit-logs": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get audit logs with filters and pagination. */
        get: {
            parameters: {
                query?: {
                    /** @description Page number (default: 1) */
                    page?: number;
                    /** @description Page size (default: 20, max: 100) */
                    pageSize?: number;
                    /** @description Filter by action */
                    action?: string;
                    /** @description Filter by entity type */
                    entityType?: string;
                    /** @description Filter by user ID */
                    userId?: number;
                    /** @description Filter by start date */
                    startDate?: string;
                    /** @description Filter by end date */
                    endDate?: string;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Admin.AuditLogs.Queries.AuditLogDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Admin.AuditLogs.Queries.AuditLogDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Admin.AuditLogs.Queries.AuditLogDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Auth/login": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Authenticate user and return JWT tokens
         * @description Authenticates a user with username and password. Returns JWT access and refresh tokens.
         *     In development mode, tokens are returned in the response body. In production, tokens are set as HTTP-only cookies.
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Login credentials (username and password) */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LoginRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LoginRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LoginRequest"];
                };
            };
            responses: {
                /** @description Login successful */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": unknown;
                        "application/json; version=1.0": unknown;
                        "text/json; version=1.0": unknown;
                    };
                };
                /** @description Invalid credentials */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Auth/refresh": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Refresh JWT access token using refresh token
         * @description Uses the refresh token from HTTP-only cookie to generate a new access token.
         *     The refresh token must be valid and not expired.
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Token refreshed successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": unknown;
                        "application/json; version=1.0": unknown;
                        "text/json; version=1.0": unknown;
                    };
                };
                /** @description Refresh token not found or invalid */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Auth/logout": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Logout user by clearing authentication cookies
         * @description Clears the auth_token and refresh_token HTTP-only cookies.
         *     This endpoint does not invalidate tokens on the server side (stateless JWT).
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Logged out successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": unknown;
                        "application/json; version=1.0": unknown;
                        "text/json; version=1.0": unknown;
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Auth/forgot-password": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Request a password reset email */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Email or username */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ForgotPasswordRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ForgotPasswordRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ForgotPasswordRequest"];
                };
            };
            responses: {
                /** @description If account exists, password reset email has been sent */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.RequestPasswordResetResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.RequestPasswordResetResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.RequestPasswordResetResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Auth/reset-password": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Reset password using a token from email */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Reset token and new password */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ResetPasswordRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ResetPasswordRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ResetPasswordRequest"];
                };
            };
            responses: {
                /** @description Password has been reset successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.ResetPasswordResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.ResetPasswordResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.ResetPasswordResponse"];
                    };
                };
                /** @description Bad request - Invalid token or validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Auth/me": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get current authenticated user information
         * @description Returns the current user's profile information based on the JWT token in the Authorization header or cookie.
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description User information retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Queries.CurrentUserDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Queries.CurrentUserDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Queries.CurrentUserDto"];
                    };
                };
                /** @description Authentication required */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Auth/invite": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Invite a user to the system */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.InviteUserRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.InviteUserRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.InviteUserRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.InviteUserResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.InviteUserResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.InviteUserResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Auth/invite/{token}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Validate an invitation token */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    token: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Queries.ValidateInviteTokenResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Queries.ValidateInviteTokenResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Queries.ValidateInviteTokenResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Auth/invite/accept": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Accept an invitation and create user account */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AcceptOrganizationInviteRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AcceptOrganizationInviteRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AcceptOrganizationInviteRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.AcceptInviteResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.AcceptInviteResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.AcceptInviteResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/backlog/stories": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Create a new user story in the project backlog */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            /** @description Story creation request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateBacklogItemRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateBacklogItemRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateBacklogItemRequest"];
                };
            };
            responses: {
                /** @description Story created successfully */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": unknown;
                        "application/json; version=1.0": unknown;
                        "text/json; version=1.0": unknown;
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/backlog/features": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Create a new feature in the project backlog */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            /** @description Feature creation request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateBacklogItemRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateBacklogItemRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateBacklogItemRequest"];
                };
            };
            responses: {
                /** @description Feature created successfully */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": unknown;
                        "application/json; version=1.0": unknown;
                        "text/json; version=1.0": unknown;
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/backlog/epics": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Create a new epic in the project backlog */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            /** @description Epic creation request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateBacklogItemRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateBacklogItemRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateBacklogItemRequest"];
                };
            };
            responses: {
                /** @description Epic created successfully */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": unknown;
                        "application/json; version=1.0": unknown;
                        "text/json; version=1.0": unknown;
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/backlog/tasks": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get backlog tasks (unassigned tasks) for a project sorted by priority. */
        get: {
            parameters: {
                query?: {
                    /** @description Page number (default: 1) */
                    page?: number;
                    /** @description Page size (default: 50, max: 100) */
                    pageSize?: number;
                    /** @description Optional priority filter: "Critical", "High", "Medium", "Low" */
                    priority?: string;
                    /** @description Optional status filter: "Todo", "InProgress", "Done" */
                    status?: string;
                    /** @description Optional search term to filter by title or description */
                    searchTerm?: string;
                };
                header?: never;
                path: {
                    /** @description The ID of the project */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Backlog tasks retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Backlog.Queries.BacklogTaskDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Backlog.Queries.BacklogTaskDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Backlog.Queries.BacklogTaskDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User doesn't have access to this project */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Comments": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all comments for a specific entity */
        get: {
            parameters: {
                query?: {
                    /** @description Type of entity (Task, Project, Sprint, Defect, BacklogItem) */
                    entityType?: string;
                    /** @description ID of the entity */
                    entityId?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Comments retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Comments.Queries.CommentDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Comments.Queries.CommentDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Comments.Queries.CommentDto"][];
                    };
                };
                /** @description Bad request - Invalid entity type or ID */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /** Add a new comment to an entity */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Comment creation request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AddCommentRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AddCommentRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AddCommentRequest"];
                };
            };
            responses: {
                /** @description Comment created successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Comments.Commands.AddCommentResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Comments.Commands.AddCommentResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Comments.Commands.AddCommentResponse"];
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Entity not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Comments/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /** Update an existing comment */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Comment ID */
                    id: number;
                };
                cookie?: never;
            };
            /** @description Comment update request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateCommentRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateCommentRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateCommentRequest"];
                };
            };
            responses: {
                /** @description Comment updated successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Comments.Queries.CommentDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Comments.Queries.CommentDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Comments.Queries.CommentDto"];
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized - Not the comment author */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Comment not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        /** Delete a comment (soft delete) */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Comment ID */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Comment deleted successfully */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Unauthorized - Not the comment author */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Comment not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/dashboard/stats": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get admin dashboard statistics including user counts, project counts, growth metrics, and recent activities. */
        get: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Dashboard statistics retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Admin.Dashboard.Queries.AdminDashboardStatsDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Admin.Dashboard.Queries.AdminDashboardStatsDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Admin.Dashboard.Queries.AdminDashboardStatsDto"];
                    };
                };
                /** @description Forbidden - User is not an admin */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/data-seeding/refresh": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Refreshes all seed data (permissions, role-permissions, workflow rules, AI policies).
         *     This will apply any new seeds that haven't been applied yet.
         *     SuperAdmin only.
         */
        post: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.RefreshSeedResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.RefreshSeedResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.RefreshSeedResponse"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/data-seeding/history": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Gets the seed history (all applied seeds).
         *     SuperAdmin only.
         */
        get: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.SeedHistoryDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.SeedHistoryDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.SeedHistoryDto"][];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/data-seeding/policy-versions": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Gets all RBAC policy versions.
         *     SuperAdmin only.
         */
        get: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.RBACPolicyVersionDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.RBACPolicyVersionDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.RBACPolicyVersionDto"][];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/data-seeding/policy-versions/active": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Gets the current active RBAC policy version.
         *     SuperAdmin only.
         */
        get: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.RBACPolicyVersionDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.RBACPolicyVersionDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.RBACPolicyVersionDto"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/dead-letter-queue": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all dead letter queue messages with pagination. */
        get: {
            parameters: {
                query?: {
                    /** @description Page number (default: 1) */
                    page?: number;
                    /** @description Page size (default: 20) */
                    pageSize?: number;
                    /** @description Optional filter by event type */
                    eventType?: string;
                    /** @description Optional filter by start date */
                    startDate?: string;
                    /** @description Optional filter by end date */
                    endDate?: string;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Dead letter queue messages retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Admin.DeadLetterQueue.Queries.DeadLetterMessageDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Admin.DeadLetterQueue.Queries.DeadLetterMessageDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Admin.DeadLetterQueue.Queries.DeadLetterMessageDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User is not an admin */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/dead-letter-queue/{id}/retry": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Retry a dead letter queue message by moving it back to the outbox for reprocessing. */
        post: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description Dead letter message ID (Guid) */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Message moved back to outbox successfully */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User is not an admin */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found - Dead letter message not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/dead-letter-queue/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post?: never;
        /** Permanently delete a dead letter queue message. */
        delete: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description Dead letter message ID (Guid) */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Message deleted successfully */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User is not an admin */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found - Dead letter message not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/defects": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all defects for a project with optional filters */
        get: {
            parameters: {
                query?: {
                    /** @description Optional status filter */
                    status?: string;
                    /** @description Optional severity filter */
                    severity?: string;
                    /** @description Optional assignee filter */
                    assignedToId?: number;
                };
                header?: never;
                path: {
                    /** @description Project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Defects retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Defects.Queries.GetProjectDefectsResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Defects.Queries.GetProjectDefectsResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Defects.Queries.GetProjectDefectsResponse"];
                    };
                };
                /** @description Error retrieving defects */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        /** Create a new defect */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            /** @description Defect creation request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateDefectRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateDefectRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateDefectRequest"];
                };
            };
            responses: {
                /** @description Defect created successfully */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Defects.Commands.CreateDefectResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Defects.Commands.CreateDefectResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Defects.Commands.CreateDefectResponse"];
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Error creating defect */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/defects/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get a specific defect by ID */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID */
                    projectId: number;
                    /** @description Defect ID */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Defect retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Defects.Queries.DefectDetailDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Defects.Queries.DefectDetailDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Defects.Queries.DefectDetailDto"];
                    };
                };
                /** @description Defect not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Error retrieving defect */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        /** Delete a defect */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID */
                    projectId: number;
                    /** @description Defect ID */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Defect deleted successfully */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Defect not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Error deleting defect */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        /** Update an existing defect */
        patch: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID */
                    projectId: number;
                    /** @description Defect ID */
                    id: number;
                };
                cookie?: never;
            };
            /** @description Defect update request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateDefectRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateDefectRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateDefectRequest"];
                };
            };
            responses: {
                /** @description Defect updated successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Defects.Commands.UpdateDefectResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Defects.Commands.UpdateDefectResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Defects.Commands.UpdateDefectResponse"];
                    };
                };
                /** @description Defect not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Error updating defect */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        trace?: never;
    };
    "/api/v1/feature-flags": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all feature flags for the current user's organization. */
        get: {
            parameters: {
                query?: {
                    /** @description Optional organization ID to filter feature flags. If not provided, uses current user's organization. */
                    organizationId?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Returns the list of feature flags */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Queries.FeatureFlagDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Queries.FeatureFlagDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Queries.FeatureFlagDto"][];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/feature-flags/{name}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get a feature flag by name for the current user's organization. */
        get: {
            parameters: {
                query?: {
                    /** @description Optional organization ID to filter feature flags. If not provided, uses current user's organization. */
                    organizationId?: number;
                };
                header?: never;
                path: {
                    /** @description The name of the feature flag */
                    name: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Returns the feature flag */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Queries.FeatureFlagDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Queries.FeatureFlagDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Queries.FeatureFlagDto"];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Feature flag not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/feature-flags": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all feature flags, optionally filtered by organization ID. */
        get: {
            parameters: {
                query?: {
                    /** @description Optional organization ID to filter feature flags. If not provided, returns all flags. */
                    organizationId?: number;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Feature flags retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Queries.FeatureFlagDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Queries.FeatureFlagDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Queries.FeatureFlagDto"][];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User is not an admin */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /** Create a new feature flag. */
        post: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            /** @description Feature flag creation details */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Commands.CreateFeatureFlagCommand"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Commands.CreateFeatureFlagCommand"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Commands.CreateFeatureFlagCommand"];
                };
            };
            responses: {
                /** @description Feature flag created successfully */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Queries.FeatureFlagDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Queries.FeatureFlagDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Queries.FeatureFlagDto"];
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User is not an admin */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Conflict - Feature flag with same name and organization already exists */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/feature-flags/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /** Update an existing feature flag. */
        put: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description Feature flag ID (Guid) */
                    id: string;
                };
                cookie?: never;
            };
            /** @description Feature flag update details (IsEnabled and/or Description) */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.UpdateFeatureFlagRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.UpdateFeatureFlagRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.UpdateFeatureFlagRequest"];
                };
            };
            responses: {
                /** @description Feature flag updated successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Queries.FeatureFlagDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Queries.FeatureFlagDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.FeatureFlags.Queries.FeatureFlagDto"];
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User is not an admin */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found - Feature flag not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Health": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get API health status
         * @description Checks database connectivity and returns overall API health status.
         *     Returns 200 OK if healthy, 503 Service Unavailable if unhealthy.
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description API is healthy */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": unknown;
                        "application/json; version=1.0": unknown;
                        "text/json; version=1.0": unknown;
                    };
                };
                /** @description API is unhealthy (database connection failed) */
                503: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": unknown;
                        "application/json; version=1.0": unknown;
                        "text/json; version=1.0": unknown;
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/health/api": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Performs smoke tests on critical API endpoints
         *     Tests routing, authentication, and basic endpoint availability
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ApiHealthResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ApiHealthResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ApiHealthResponse"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/insights": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get AI insights for a project */
        get: {
            parameters: {
                query?: {
                    /** @description Optional status filter */
                    status?: string;
                    /** @description Optional agent type filter */
                    agentType?: string;
                };
                header?: never;
                path: {
                    /** @description Project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Insights retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": unknown;
                        "application/json; version=1.0": unknown;
                        "text/json; version=1.0": unknown;
                    };
                };
                /** @description Project not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Error retrieving insights */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Lookups/project-types": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all project types with metadata */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LookupResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LookupResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LookupResponse"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Lookups/task-statuses": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all task statuses with metadata */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LookupResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LookupResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LookupResponse"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Lookups/task-priorities": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all task priorities with metadata */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LookupResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LookupResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LookupResponse"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Metrics": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get metrics summary for all projects or a specific project */
        get: {
            parameters: {
                query?: {
                    projectId?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.MetricsSummaryDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.MetricsSummaryDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.MetricsSummaryDto"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Metrics/sprint-velocity-chart": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get sprint velocity chart data (last 6 completed sprints) */
        get: {
            parameters: {
                query?: {
                    projectId?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.SprintVelocityChartResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.SprintVelocityChartResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.SprintVelocityChartResponse"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Metrics/task-distribution": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get task distribution by status */
        get: {
            parameters: {
                query?: {
                    projectId?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.TaskDistributionResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.TaskDistributionResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.TaskDistributionResponse"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Metrics/sprint-burndown": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get sprint burndown chart data */
        get: {
            parameters: {
                query?: {
                    sprintId?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.SprintBurndownResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.SprintBurndownResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.SprintBurndownResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Metrics/defects-by-severity": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get defects by severity */
        get: {
            parameters: {
                query?: {
                    projectId?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.DefectsBySeverityResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.DefectsBySeverityResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.DefectsBySeverityResponse"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Metrics/team-velocity": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get team velocity trend over time */
        get: {
            parameters: {
                query?: {
                    projectId?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.TeamVelocityResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.TeamVelocityResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Queries.Metrics.TeamVelocityResponse"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Metrics/velocity": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get sprint velocity based on completed story points. */
        get: {
            parameters: {
                query?: {
                    /** @description The ID of the project (required) */
                    projectId?: number;
                    /** @description Optional sprint ID. If provided, returns velocity for that sprint only. If null, returns velocity for last N sprints. */
                    sprintId?: number;
                    /** @description Number of sprints to retrieve for trend analysis (default: 5, max: 20). Only used when sprintId is not provided. */
                    lastNSprints?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Sprint velocity retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Queries.SprintVelocityResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Queries.SprintVelocityResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Queries.SprintVelocityResponse"];
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User doesn't have access to this project */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found - Project or sprint not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/milestones": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all milestones for a project with optional filtering. */
        get: {
            parameters: {
                query?: {
                    /** @description Optional status filter (Pending, InProgress, Completed, Missed, Cancelled) */
                    status?: string;
                    /** @description Whether to include completed milestones (default: false) */
                    includeCompleted?: boolean;
                };
                header?: never;
                path: {
                    /** @description The project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Milestones retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"][];
                    };
                };
                /** @description Invalid request parameters */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /** Creates a new milestone for a project. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            /** @description The milestone creation request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.MilestonesController.CreateMilestoneRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.MilestonesController.CreateMilestoneRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.MilestonesController.CreateMilestoneRequest"];
                };
            };
            responses: {
                /** @description Milestone created successfully */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"];
                    };
                };
                /** @description Invalid request data */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Project not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/milestones/next": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get the next upcoming milestone for a project. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Next milestone retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description No upcoming milestones found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/milestones/statistics": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get milestone statistics for a project. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Statistics retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneStatisticsDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneStatisticsDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneStatisticsDto"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Milestones/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get a specific milestone by ID. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The milestone ID */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Milestone retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Milestone not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        /** Updates an existing milestone. */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The milestone ID */
                    id: number;
                };
                cookie?: never;
            };
            /** @description The milestone update request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.MilestonesController.UpdateMilestoneRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.MilestonesController.UpdateMilestoneRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.MilestonesController.UpdateMilestoneRequest"];
                };
            };
            responses: {
                /** @description Milestone updated successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"];
                    };
                };
                /** @description Invalid request data */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Milestone not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        /** Deletes a milestone. */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The milestone ID */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Milestone deleted successfully */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Milestone not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Milestones/overdue": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all overdue milestones across all projects. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Overdue milestones retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"][];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Milestones/{id}/complete": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Marks a milestone as completed. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The milestone ID */
                    id: number;
                };
                cookie?: never;
            };
            /** @description Optional completion request with completion date */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.MilestonesController.CompleteMilestoneRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.MilestonesController.CompleteMilestoneRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.MilestonesController.CompleteMilestoneRequest"];
                };
            };
            responses: {
                /** @description Milestone completed successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto"];
                    };
                };
                /** @description Invalid request data or milestone cannot be completed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Milestone not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Notifications": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get notifications for the current user */
        get: {
            parameters: {
                query?: {
                    unreadOnly?: boolean;
                    limit?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Notifications.Queries.GetNotificationsResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Notifications.Queries.GetNotificationsResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Notifications.Queries.GetNotificationsResponse"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Notifications/{id}/read": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        /** Mark a notification as read */
        patch: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        trace?: never;
    };
    "/api/v1/Notifications/mark-all-read": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        /** Mark all notifications as read for the current user */
        patch: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        trace?: never;
    };
    "/api/v1/Notifications/unread-count": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get count of unread notifications for current user */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Notifications.Queries.GetUnreadNotificationCountResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Notifications.Queries.GetUnreadNotificationCountResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Notifications.Queries.GetUnreadNotificationCountResponse"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/organization/me": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get the current user's organization details (Admin only). */
        get: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Queries.OrganizationDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Queries.OrganizationDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Queries.OrganizationDto"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/organization/members": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get a paginated list of organization members (Admin only - own organization). */
        get: {
            parameters: {
                query?: {
                    /** @description Page number (default: 1) */
                    page?: number;
                    /** @description Page size (default: 20, max: 100) */
                    pageSize?: number;
                    /** @description Search term for filtering by name or email */
                    searchTerm?: string;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Identity.DTOs.UserListDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Identity.DTOs.UserListDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Identity.DTOs.UserListDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/organization/permission-policy": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get the current user's organization permission policy (Admin only).
         *     Returns the permission policy for the admin's organization.
         *     If no policy exists, returns default values (all permissions allowed).
         */
        get: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.DTOs.OrganizationPermissionPolicyDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.DTOs.OrganizationPermissionPolicyDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.DTOs.OrganizationPermissionPolicyDto"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/organization/members/{userId}/global-role": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * Update a user's global role within the organization (Admin only - own organization).
         *     Admin can only assign Admin or User roles, not SuperAdmin.
         */
        put: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description User ID */
                    userId: number;
                };
                cookie?: never;
            };
            /** @description Role update request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.UpdateUserGlobalRoleRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.UpdateUserGlobalRoleRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.UpdateUserGlobalRoleRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.UpdateUserGlobalRoleResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.UpdateUserGlobalRoleResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.UpdateUserGlobalRoleResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/organizations": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get a paginated list of organizations (SuperAdmin only). */
        get: {
            parameters: {
                query?: {
                    /** @description Page number (default: 1) */
                    page?: number;
                    /** @description Page size (default: 20, max: 100) */
                    pageSize?: number;
                    /** @description Search term for filtering by name or code */
                    searchTerm?: string;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Organizations.Queries.OrganizationDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Organizations.Queries.OrganizationDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Organizations.Queries.OrganizationDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /** Create a new organization (SuperAdmin only). */
        post: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            /** @description Organization creation details */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.CreateOrganizationCommand"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.CreateOrganizationCommand"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.CreateOrganizationCommand"];
                };
            };
            responses: {
                /** @description Created */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.CreateOrganizationResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.CreateOrganizationResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.CreateOrganizationResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/organizations/{orgId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get a single organization by ID (SuperAdmin only). */
        get: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description Organization ID */
                    orgId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Queries.OrganizationDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Queries.OrganizationDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Queries.OrganizationDto"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        /** Update an organization (SuperAdmin only). */
        put: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description Organization ID */
                    orgId: number;
                };
                cookie?: never;
            };
            /** @description Organization update details */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.UpdateOrganizationCommand"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.UpdateOrganizationCommand"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.UpdateOrganizationCommand"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.UpdateOrganizationResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.UpdateOrganizationResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.UpdateOrganizationResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        /** Delete an organization (SuperAdmin only). */
        delete: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description Organization ID */
                    orgId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.DeleteOrganizationResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.DeleteOrganizationResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.Commands.DeleteOrganizationResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Permissions/me": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get current user's permissions
         * @description Returns the current authenticated user's permissions and global role.
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Permissions retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UserPermissionsResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UserPermissionsResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UserPermissionsResponse"];
                    };
                };
                /** @description Unauthorized - Authentication required */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Permissions/matrix": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get permissions matrix (Admin only)
         * @description Returns a matrix showing all permissions and which roles have access to them.
         *     Only administrators can access this endpoint.
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Permissions matrix retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Permissions.Queries.PermissionsMatrixDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Permissions.Queries.PermissionsMatrixDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Permissions.Queries.PermissionsMatrixDto"];
                    };
                };
                /** @description Forbidden - User is not an administrator */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Permissions/roles/{role}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * Update permissions for a role (Admin only)
         * @description Updates the permissions assigned to a specific role.
         *     Only administrators can modify role permissions.
         */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Role name (Admin, Manager, Developer, Tester, Viewer) */
                    role: string;
                };
                cookie?: never;
            };
            /** @description List of permission IDs to assign to the role */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateRolePermissionsRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateRolePermissionsRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateRolePermissionsRequest"];
                };
            };
            responses: {
                /** @description Role permissions updated successfully */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad request - Invalid role name */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User is not an administrator */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Projects": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all projects for the current user with pagination */
        get: {
            parameters: {
                query?: {
                    /** @description Page number (default: 1, minimum: 1) */
                    page?: number;
                    /** @description Number of items per page (default: 20, minimum: 1, maximum: 100) */
                    pageSize?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Returns the paginated list of projects */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Projects.Queries.ProjectListDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Projects.Queries.ProjectListDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Projects.Queries.ProjectListDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /**
         * Create a new project
         * @description Sample request:
         *
         *         POST /api/v1/Projects
         *         {
         *            "name": "My New Project",
         *            "description": "Project description",
         *            "type": "Scrum",
         *            "sprintDurationDays": 14,
         *            "status": "Active",
         *            "startDate": "2025-01-01T00:00:00Z",
         *            "memberIds": [1, 2, 3]
         *         }
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Project creation request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateProjectRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateProjectRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateProjectRequest"];
                };
            };
            responses: {
                /** @description Project created successfully */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Commands.CreateProjectResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Commands.CreateProjectResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Commands.CreateProjectResponse"];
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User does not have permission to create projects */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Projects/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get a specific project by ID */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Returns the project details */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Queries.GetProjectByIdResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Queries.GetProjectByIdResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Queries.GetProjectByIdResponse"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Project not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        /** Update an existing project */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateProjectRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateProjectRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateProjectRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Commands.UpdateProjectResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Commands.UpdateProjectResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Commands.UpdateProjectResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        /** Delete (archive) a project */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Projects/{id}/my-role": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get the current user's role in a project */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Returns the user's role in the project */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": "ProductOwner" | "ScrumMaster" | "Developer" | "Tester" | "Viewer" | "Manager";
                        "application/json; version=1.0": "ProductOwner" | "ScrumMaster" | "Developer" | "Tester" | "Viewer" | "Manager";
                        "text/json; version=1.0": "ProductOwner" | "ScrumMaster" | "Developer" | "Tester" | "Viewer" | "Manager";
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Projects/{id}/permanent": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post?: never;
        /** Permanently delete a project and all its data */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Projects/{id}/members": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all members of a project */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Returns the list of project members */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Queries.ProjectMemberDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Queries.ProjectMemberDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Queries.ProjectMemberDto"][];
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User does not have permission to view project members */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Project not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /**
         * Invite a member to a project
         * @description Sample request:
         *
         *         POST /api/v1/Projects/{id}/members
         *         {
         *            "email": "user@example.com",
         *            "role": "Developer"
         *         }
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID */
                    id: number;
                };
                cookie?: never;
            };
            /** @description Invitation request with email and role */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.InviteMemberRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.InviteMemberRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.InviteMemberRequest"];
                };
            };
            responses: {
                /** @description Member invited successfully */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": number;
                        "application/json; version=1.0": number;
                        "text/json; version=1.0": number;
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User does not have permission to invite members */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Project or user not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Projects/{projectId}/assign-team": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Assign an entire team to a project */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The ID of the project */
                    projectId: number;
                };
                cookie?: never;
            };
            /** @description The team assignment request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AssignTeamToProjectRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AssignTeamToProjectRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AssignTeamToProjectRequest"];
                };
            };
            responses: {
                /** @description Team successfully assigned to project */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Commands.AssignTeamToProjectResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Commands.AssignTeamToProjectResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Commands.AssignTeamToProjectResponse"];
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User doesn't have permission to assign teams */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found - Project or team not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Projects/{id}/assigned-teams": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get all teams assigned to a project.
         *     Returns teams that are currently assigned (active) to the project.
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Returns the list of assigned teams */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Queries.ProjectAssignedTeamDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Queries.ProjectAssignedTeamDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Projects.Queries.ProjectAssignedTeamDto"][];
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User does not have permission to view project teams */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Project not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Projects/{projectId}/members/{userId}/role": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /** Change a member's role in a project */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    projectId: number;
                    userId: number;
                };
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ChangeRoleRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ChangeRoleRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ChangeRoleRequest"];
                };
            };
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Projects/{projectId}/members/{userId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post?: never;
        /** Remove a member from a project */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    projectId: number;
                    userId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Projects/{projectId}/dependency-graph": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get the complete dependency graph for a project */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The project ID to get the dependency graph for */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Dependency graph retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.DTOs.DependencyGraphDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.DTOs.DependencyGraphDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.DTOs.DependencyGraphDto"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Projects/{projectId}/permissions": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get current user's permissions for a specific project */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Permissions retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ProjectPermissionsResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ProjectPermissionsResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ProjectPermissionsResponse"];
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Project not found or user is not a member */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/ReadModels/task-board/{projectId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get task board read model for a project */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Task board retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Projections.Queries.TaskBoardReadModelDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Projections.Queries.TaskBoardReadModelDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Projections.Queries.TaskBoardReadModelDto"];
                    };
                };
                /** @description Not Modified - Resource hasn't changed (ETag match) */
                304: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Task board not found for the specified project */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/ReadModels/sprint-summary/{sprintId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get sprint summary read model */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Sprint ID */
                    sprintId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Sprint summary retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Projections.Queries.SprintSummaryReadModelDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Projections.Queries.SprintSummaryReadModelDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Projections.Queries.SprintSummaryReadModelDto"];
                    };
                };
                /** @description Not Modified - Resource hasn't changed (ETag match) */
                304: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Sprint summary not found for the specified sprint */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/ReadModels/project-overview/{projectId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get project overview read model */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Project overview retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Projections.Queries.ProjectOverviewReadModelDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Projections.Queries.ProjectOverviewReadModelDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Projections.Queries.ProjectOverviewReadModelDto"];
                    };
                };
                /** @description Not Modified - Resource hasn't changed (ETag match) */
                304: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Project overview not found for the specified project */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/ReadModels/project-overviews": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get multiple project overviews (for dashboard) */
        get: {
            parameters: {
                query?: {
                    /** @description Optional organization ID filter */
                    organizationId?: number;
                    /** @description Optional status filter (Active, Archived) */
                    status?: string;
                    /** @description Page number (default: 1) */
                    page?: number;
                    /** @description Page size (default: 20, max: 100) */
                    pageSize?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Project overviews retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Projections.Queries.ProjectOverviewReadModelDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Projections.Queries.ProjectOverviewReadModelDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Projections.Queries.ProjectOverviewReadModelDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/read-models/rebuild": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Rebuild read model projections from source data.
         *     Admin-only operation for maintaining projection consistency.
         */
        post: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            /** @description Rebuild projection request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.RebuildProjectionRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.RebuildProjectionRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.Admin.RebuildProjectionRequest"];
                };
            };
            responses: {
                /** @description Projections rebuilt successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Projections.Commands.RebuildProjectionResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Projections.Commands.RebuildProjectionResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Projections.Commands.RebuildProjectionResponse"];
                    };
                };
                /** @description Invalid request parameters */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User is not an admin */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/releases": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all releases for a project with optional status filtering. */
        get: {
            parameters: {
                query?: {
                    /** @description Optional status filter */
                    status?: string;
                };
                header?: never;
                path: {
                    /** @description The project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Releases retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseDto"][];
                    };
                };
                /** @description Invalid request parameters */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /** Create a new release for a project. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            /** @description Release creation data */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.CreateReleaseRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.CreateReleaseRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.CreateReleaseRequest"];
                };
            };
            responses: {
                /** @description Release created successfully */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseDto"];
                    };
                };
                /** @description Invalid request data */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Releases/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get a specific release by ID. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The release ID */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Release retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseDto"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Release not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        /** Update an existing release. */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The release ID */
                    id: number;
                };
                cookie?: never;
            };
            /** @description Release update data */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.UpdateReleaseRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.UpdateReleaseRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.UpdateReleaseRequest"];
                };
            };
            responses: {
                /** @description Release updated successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseDto"];
                    };
                };
                /** @description Invalid request data */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Release not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        /** Delete a release. */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The release ID */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Release deleted successfully */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Release not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/sprints/available": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get available sprints that can be added to a release. */
        get: {
            parameters: {
                query?: {
                    /** @description Optional release ID (for editing existing release) */
                    releaseId?: number;
                };
                header?: never;
                path: {
                    /** @description The project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Available sprints retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseSprintDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseSprintDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseSprintDto"][];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/projects/{projectId}/releases/statistics": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get release statistics for a project. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Statistics retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseStatisticsDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseStatisticsDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseStatisticsDto"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Releases/{id}/deploy": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Deploy a release. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The release ID */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Release deployed successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseDto"];
                    };
                };
                /** @description Release cannot be deployed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Release not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Releases/{releaseId}/sprints/{sprintId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Add a sprint to a release. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The release ID */
                    releaseId: number;
                    /** @description The sprint ID */
                    sprintId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Sprint added successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Release or sprint not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Releases/{releaseId}/sprints/bulk": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Bulk add sprints to a release. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The release ID */
                    releaseId: number;
                };
                cookie?: never;
            };
            /** @description Request containing sprint IDs */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.BulkAddSprintsRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.BulkAddSprintsRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.BulkAddSprintsRequest"];
                };
            };
            responses: {
                /** @description Sprints added successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.BulkAddSprintsResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.BulkAddSprintsResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.BulkAddSprintsResponse"];
                    };
                };
                /** @description Invalid request data */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Release not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Releases/{releaseId}/notes/generate": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Generate release notes for a release using AI. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The release ID */
                    releaseId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Release notes generated successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.GenerateReleaseNotesResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.GenerateReleaseNotesResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.GenerateReleaseNotesResponse"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Release not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Releases/{releaseId}/notes": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /** Update release notes (auto-generate or manual). */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The release ID */
                    releaseId: number;
                };
                cookie?: never;
            };
            /** @description Request containing notes and auto-generate flag */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.UpdateReleaseNotesRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.UpdateReleaseNotesRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.UpdateReleaseNotesRequest"];
                };
            };
            responses: {
                /** @description Release notes updated successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Release not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Releases/{releaseId}/changelog/generate": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Generate changelog for a release using AI. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The release ID */
                    releaseId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Changelog generated successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.GenerateChangelogResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.GenerateChangelogResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.GenerateChangelogResponse"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Release not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Releases/{releaseId}/changelog": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /** Update changelog (auto-generate or manual). */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The release ID */
                    releaseId: number;
                };
                cookie?: never;
            };
            /** @description Request containing changelog and auto-generate flag */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.UpdateChangelogRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.UpdateChangelogRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.UpdateChangelogRequest"];
                };
            };
            responses: {
                /** @description Changelog updated successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Release not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Releases/{releaseId}/quality-gates/evaluate": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Evaluate all quality gates for a release. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The release ID */
                    releaseId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Quality gates evaluated successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.QualityGateDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.QualityGateDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.QualityGateDto"][];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Release not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Releases/{releaseId}/quality-gates/approve": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Approve a quality gate manually. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The release ID */
                    releaseId: number;
                };
                cookie?: never;
            };
            /** @description Request containing gate type */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.ApproveQualityGateRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.ApproveQualityGateRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ReleasesController.ApproveQualityGateRequest"];
                };
            };
            responses: {
                /** @description Quality gate approved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Invalid gate type or gate cannot be approved */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Release not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Releases/sprints/{sprintId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post?: never;
        /** Remove a sprint from a release. */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The sprint ID */
                    sprintId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Sprint removed successfully */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Insufficient permissions */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Sprint not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/reports/activity-by-role": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get activity report grouped by user role.
         *     Shows actions grouped by user role with counts and last performed dates.
         */
        get: {
            parameters: {
                query?: {
                    /** @description Start date for filtering (optional) */
                    startDate?: string;
                    /** @description End date for filtering (optional) */
                    endDate?: string;
                    /** @description Filter by specific role (optional) */
                    roleFilter?: string;
                    /** @description Filter by specific action type (optional) */
                    actionTypeFilter?: string;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Reports.DTOs.RoleActivityReportDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Reports.DTOs.RoleActivityReportDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Reports.DTOs.RoleActivityReportDto"][];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/reports/ai-decisions-by-role": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get AI decision report grouped by approver role.
         *     Shows AI decisions by who approved/rejected them, with response times and confidence scores.
         */
        get: {
            parameters: {
                query?: {
                    /** @description Start date for filtering (optional) */
                    startDate?: string;
                    /** @description End date for filtering (optional) */
                    endDate?: string;
                    /** @description Filter by specific role (optional) */
                    roleFilter?: string;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Reports.DTOs.AIDecisionRoleReportDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Reports.DTOs.AIDecisionRoleReportDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Reports.DTOs.AIDecisionRoleReportDto"][];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/reports/workflow-transitions-by-role": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get workflow transition report grouped by user role.
         *     Shows status changes grouped by role with transition counts.
         */
        get: {
            parameters: {
                query?: {
                    /** @description Start date for filtering (optional) */
                    startDate?: string;
                    /** @description End date for filtering (optional) */
                    endDate?: string;
                    /** @description Filter by specific role (optional) */
                    roleFilter?: string;
                    /** @description Filter by entity type (task, sprint, project, etc.) (optional) */
                    entityTypeFilter?: string;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Reports.DTOs.WorkflowRoleReportDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Reports.DTOs.WorkflowRoleReportDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Reports.DTOs.WorkflowRoleReportDto"][];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/reports/activity-by-role/export": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Export activity by role report to CSV. */
        get: {
            parameters: {
                query?: {
                    startDate?: string;
                    endDate?: string;
                    roleFilter?: string;
                    actionTypeFilter?: string;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": string;
                        "application/json; version=1.0": string;
                        "text/json; version=1.0": string;
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/reports/ai-decisions-by-role/export": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Export AI decisions by role report to CSV. */
        get: {
            parameters: {
                query?: {
                    startDate?: string;
                    endDate?: string;
                    roleFilter?: string;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": string;
                        "application/json; version=1.0": string;
                        "text/json; version=1.0": string;
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/reports/workflow-transitions-by-role/export": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Export workflow transitions by role report to CSV. */
        get: {
            parameters: {
                query?: {
                    startDate?: string;
                    endDate?: string;
                    roleFilter?: string;
                    entityTypeFilter?: string;
                };
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": string;
                        "application/json; version=1.0": string;
                        "text/json; version=1.0": string;
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Search": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Global search across projects, tasks, and users */
        get: {
            parameters: {
                query?: {
                    q?: string;
                    limit?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Search.Queries.SearchResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Search.Queries.SearchResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Search.Queries.SearchResponse"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Settings/global": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all global settings (admin only) */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": {
                            [key: string]: string;
                        };
                        "application/json; version=1.0": {
                            [key: string]: string;
                        };
                        "text/json; version=1.0": {
                            [key: string]: string;
                        };
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Settings/{key}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /** Update a global setting (admin only) */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    key: string;
                };
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateSettingRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateSettingRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateSettingRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Settings.Commands.UpdateSettingResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Settings.Commands.UpdateSettingResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Settings.Commands.UpdateSettingResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Settings/test-email": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Send a test email (admin only) */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.SendTestEmailRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.SendTestEmailRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.SendTestEmailRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Settings.Commands.SendTestEmailResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Settings.Commands.SendTestEmailResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Settings.Commands.SendTestEmailResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Settings/organization": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all organization settings for the current user's organization */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": {
                            [key: string]: string;
                        };
                        "application/json; version=1.0": {
                            [key: string]: string;
                        };
                        "text/json; version=1.0": {
                            [key: string]: string;
                        };
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Settings/language": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get current user's language preference */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LanguageResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LanguageResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LanguageResponse"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        /** Update current user's language preference */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateLanguageRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateLanguageRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateLanguageRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LanguageResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LanguageResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.LanguageResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Settings": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all settings (backward compatibility - redirects to global) */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": {
                            [key: string]: string;
                        };
                        "application/json; version=1.0": {
                            [key: string]: string;
                        };
                        "text/json; version=1.0": {
                            [key: string]: string;
                        };
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Sprints/project/{projectId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all sprints for a project */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Queries.GetSprintsByProjectResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Queries.GetSprintsByProjectResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Queries.GetSprintsByProjectResponse"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Sprints/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get sprint by ID with tasks */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Queries.SprintDetailDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Queries.SprintDetailDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Queries.SprintDetailDto"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Sprints": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Create a new sprint */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateSprintRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateSprintRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateSprintRequest"];
                };
            };
            responses: {
                /** @description Created */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Queries.SprintDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Queries.SprintDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Queries.SprintDto"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Sprints/{id}/assign-tasks": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Assign tasks to a sprint */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AssignTasksRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AssignTasksRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AssignTasksRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.AssignTasksToSprintResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.AssignTasksToSprintResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.AssignTasksToSprintResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Sprints/{sprintId}/add-tasks": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Add tasks to a sprint with automatic velocity calculation. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The ID of the sprint */
                    sprintId: number;
                };
                cookie?: never;
            };
            /** @description The request containing task IDs and optional capacity warning flag */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AddTasksToSprintRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AddTasksToSprintRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AddTasksToSprintRequest"];
                };
            };
            responses: {
                /** @description Tasks added successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.AddTaskToSprintResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.AddTaskToSprintResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.AddTaskToSprintResponse"];
                    };
                };
                /** @description Tasks added but sprint is over capacity (warning in response) */
                207: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.AddTaskToSprintResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.AddTaskToSprintResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.AddTaskToSprintResponse"];
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User doesn't have permission to manage sprints */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found - Sprint or tasks not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Sprints/{sprintId}/remove-tasks": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Remove tasks from a sprint and return them to the backlog. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The ID of the sprint */
                    sprintId: number;
                };
                cookie?: never;
            };
            /** @description The request containing task IDs to remove */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.RemoveTasksFromSprintRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.RemoveTasksFromSprintRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.RemoveTasksFromSprintRequest"];
                };
            };
            responses: {
                /** @description Tasks removed successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.RemoveTaskFromSprintResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.RemoveTaskFromSprintResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.RemoveTaskFromSprintResponse"];
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User doesn't have permission to manage sprints */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found - Sprint or tasks not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Sprints/{id}/start": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        /** Start a sprint (change status to Active) */
        patch: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.StartSprintResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.StartSprintResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.StartSprintResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        trace?: never;
    };
    "/api/v1/Sprints/{id}/complete": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        /** Complete a sprint (change status to Completed) */
        patch: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CompleteSprintRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CompleteSprintRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CompleteSprintRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.CompleteSprintResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.CompleteSprintResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Sprints.Commands.CompleteSprintResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        trace?: never;
    };
    "/api/v1/superadmin/organizations/{orgId}/ai-quota": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get organization AI quota by organization ID (SuperAdmin only). */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Organization ID */
                    orgId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.OrganizationAIQuotaDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.OrganizationAIQuotaDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.OrganizationAIQuotaDto"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        /** Upsert (create or update) organization AI quota (SuperAdmin only). */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Organization ID */
                    orgId: number;
                };
                cookie?: never;
            };
            /** @description Quota update request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.UpdateOrganizationAIQuotaRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.UpdateOrganizationAIQuotaRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.UpdateOrganizationAIQuotaRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.OrganizationAIQuotaDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.OrganizationAIQuotaDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.AI.DTOs.OrganizationAIQuotaDto"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/superadmin/organizations/ai-quotas": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get a paginated list of all organization AI quotas (SuperAdmin only). */
        get: {
            parameters: {
                query?: {
                    /** @description Page number (default: 1) */
                    page?: number;
                    /** @description Page size (default: 20, max: 100) */
                    pageSize?: number;
                    /** @description Search term for filtering by organization name or code */
                    searchTerm?: string;
                    /** @description Filter by AI enabled status */
                    isAIEnabled?: boolean;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.DTOs.OrganizationAIQuotaDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.DTOs.OrganizationAIQuotaDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.DTOs.OrganizationAIQuotaDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/superadmin/organizations/{orgId}/permission-policy": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get organization permission policy by organization ID (SuperAdmin only). */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Organization ID */
                    orgId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.DTOs.OrganizationPermissionPolicyDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.DTOs.OrganizationPermissionPolicyDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.DTOs.OrganizationPermissionPolicyDto"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        /** Upsert (create or update) organization permission policy (SuperAdmin only). */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Organization ID */
                    orgId: number;
                };
                cookie?: never;
            };
            /** @description Policy update request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.DTOs.UpdateOrganizationPermissionPolicyRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.DTOs.UpdateOrganizationPermissionPolicyRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.DTOs.UpdateOrganizationPermissionPolicyRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.DTOs.OrganizationPermissionPolicyDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.DTOs.OrganizationPermissionPolicyDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Organizations.DTOs.OrganizationPermissionPolicyDto"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/system-health": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get current system health metrics. */
        get: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Admin.SystemHealth.Queries.SystemHealthDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Admin.SystemHealth.Queries.SystemHealthDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Admin.SystemHealth.Queries.SystemHealthDto"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Tasks/project/{projectId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all tasks for a project with optional filters */
        get: {
            parameters: {
                query?: {
                    /** @description Optional status filter (Todo, InProgress, Blocked, Done) */
                    status?: string;
                    /** @description Optional assignee ID filter */
                    assigneeId?: number;
                    /** @description Optional priority filter (Low, Medium, High, Critical) */
                    priority?: string;
                };
                header?: never;
                path: {
                    /** @description Project ID */
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Returns the list of tasks */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.GetTasksByProjectResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.GetTasksByProjectResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.GetTasksByProjectResponse"];
                    };
                };
                /** @description Bad request - Invalid filter parameters */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Tasks/{taskId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get a specific task by ID */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Task ID */
                    taskId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Returns the task details */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.TaskDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.TaskDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.TaskDto"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Task not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        /** Update an existing task */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    taskId: number;
                };
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateTaskRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateTaskRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateTaskRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.TaskDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.TaskDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.TaskDto"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Tasks/project/{projectId}/blocked": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all blocked tasks for a project */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    projectId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.GetBlockedTasksResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.GetBlockedTasksResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.GetBlockedTasksResponse"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Tasks/assignee/{assigneeId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all tasks assigned to a specific user */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    assigneeId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.TaskDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.TaskDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.TaskDto"][];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Tasks": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Create a new task
         * @description Sample request:
         *
         *         POST /api/v1/Tasks
         *         {
         *            "title": "Implement user authentication",
         *            "description": "Add JWT-based authentication to the API",
         *            "projectId": 1,
         *            "priority": "High",
         *            "storyPoints": 5,
         *            "assigneeId": 2
         *         }
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Task creation request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateTaskRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateTaskRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.CreateTaskRequest"];
                };
            };
            responses: {
                /** @description Task created successfully */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.TaskDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.TaskDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Queries.TaskDto"];
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description User does not have permission to create tasks */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Tasks/{taskId}/status": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        /** Change task status */
        patch: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    taskId: number;
                };
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ChangeTaskStatusRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ChangeTaskStatusRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.ChangeTaskStatusRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Commands.ChangeTaskStatusResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Commands.ChangeTaskStatusResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Commands.ChangeTaskStatusResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        trace?: never;
    };
    "/api/v1/Tasks/{taskId}/assign": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        /** Assign or unassign a task to a user */
        patch: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    taskId: number;
                };
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AssignTaskRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AssignTaskRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AssignTaskRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Commands.AssignTaskResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Commands.AssignTaskResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.Commands.AssignTaskResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        trace?: never;
    };
    "/api/v1/Tasks/{taskId}/dependencies": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get all dependencies for a specific task */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The task ID to get dependencies for */
                    taskId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Dependencies retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.DTOs.TaskDependencyDto"][];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.DTOs.TaskDependencyDto"][];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.DTOs.TaskDependencyDto"][];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /** Add a dependency to a task */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The task that depends on another task (source task) */
                    taskId: number;
                };
                cookie?: never;
            };
            /** @description Dependency creation request */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AddTaskDependencyRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AddTaskDependencyRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AddTaskDependencyRequest"];
                };
            };
            responses: {
                /** @description Dependency created successfully */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.DTOs.TaskDependencyDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.DTOs.TaskDependencyDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Tasks.DTOs.TaskDependencyDto"];
                    };
                };
                /** @description Bad request - Validation failed or cycle detected */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Task not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Tasks/dependencies/{dependencyId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post?: never;
        /** Remove a task dependency */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description The ID of the dependency to remove */
                    dependencyId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Dependency removed successfully */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Dependency not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Teams": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** List all teams user has access to */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.GetAllTeamsResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.GetAllTeamsResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.GetAllTeamsResponse"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /** Register a new team */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.RegisterTeamRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.RegisterTeamRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.RegisterTeamRequest"];
                };
            };
            responses: {
                /** @description Created */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.TeamDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.TeamDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.TeamDto"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Teams/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get team by ID with members */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.TeamDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.TeamDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.TeamDto"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Teams/{id}/capacity": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        /** Update team capacity */
        patch: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateTeamCapacityRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateTeamCapacityRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateTeamCapacityRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Commands.UpdateTeamCapacityResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Commands.UpdateTeamCapacityResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Commands.UpdateTeamCapacityResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        trace?: never;
    };
    "/api/v1/Teams/{id}/availability": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get team availability (capacity with active sprint calculations) */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.TeamCapacityDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.TeamCapacityDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.TeamCapacityDto"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Teams/{id}/members": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Add a member to a team */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AddTeamMemberRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AddTeamMemberRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.AddTeamMemberRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.TeamDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.TeamDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.TeamDto"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Teams/{id}/members/{userId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post?: never;
        /** Remove a member from a team */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: number;
                    userId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.TeamDto"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.TeamDto"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Teams.Queries.TeamDto"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Users": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Get all users in the organization (paginated).
         *     Supports filtering by role and active status, sorting, and search.
         *     Available to all authenticated users (read-only for non-admins).
         */
        get: {
            parameters: {
                query?: {
                    /** @description Page number (1-based). Default: 1 */
                    page?: number;
                    /** @description Number of items per page. Default: 20, Max: 100 */
                    pageSize?: number;
                    /** @description Filter by global role (Admin or User). Optional. */
                    role?: string;
                    /** @description Filter by active status. Optional. */
                    isActive?: boolean;
                    /** @description Field to sort by. Valid values: Username, Email, CreatedAt, LastLoginAt, Role, IsActive. Default: CreatedAt */
                    sortField?: string;
                    /** @description Sort in descending order. Default: false (ascending) */
                    sortDescending?: boolean;
                    /** @description Search term to filter users by username, email, firstName, or lastName (case-insensitive). Optional. */
                    searchTerm?: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Users retrieved successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Identity.DTOs.UserListDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Identity.DTOs.UserListDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Identity.DTOs.UserListDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User is not an admin */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Users/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /** Update a user (admin only) */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateUserRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateUserRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.UpdateUserRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Users.Commands.UpdateUserResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Users.Commands.UpdateUserResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Users.Commands.UpdateUserResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        /** Delete a user permanently (admin only) */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Users.Commands.DeleteUserResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Users.Commands.DeleteUserResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Users.Commands.DeleteUserResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Users/bulk-status": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Bulk update user status (activate/deactivate multiple users, admin only) */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.BulkUpdateUsersStatusRequest"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.BulkUpdateUsersStatusRequest"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.API.Controllers.BulkUpdateUsersStatusRequest"];
                };
            };
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Users.Commands.BulkUpdateUsersStatusResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Users.Commands.BulkUpdateUsersStatusResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Users.Commands.BulkUpdateUsersStatusResponse"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Users/{id}/projects": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get user's projects (admin only) */
        get: {
            parameters: {
                query?: {
                    page?: number;
                    pageSize?: number;
                };
                header?: never;
                path: {
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Projects.Queries.ProjectListDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Projects.Queries.ProjectListDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Projects.Queries.ProjectListDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/Users/{id}/activity": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Get user's activity history (admin only) */
        get: {
            parameters: {
                query?: {
                    limit?: number;
                };
                header?: never;
                path: {
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description Success */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Activity.Queries.GetRecentActivityResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Activity.Queries.GetRecentActivityResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Activity.Queries.GetRecentActivityResponse"];
                    };
                };
                /** @description Forbidden */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/users/invite": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Invite a user to the organization. */
        post: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path?: never;
                cookie?: never;
            };
            /** @description Invitation details */
            requestBody?: {
                content: {
                    "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Admin.Commands.InviteOrganizationUserCommand"];
                    "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Admin.Commands.InviteOrganizationUserCommand"];
                    "application/*+json; version=1.0": components["schemas"]["IntelliPM.Application.Admin.Commands.InviteOrganizationUserCommand"];
                };
            };
            responses: {
                /** @description Invitation sent successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Admin.Commands.InviteOrganizationUserResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Admin.Commands.InviteOrganizationUserResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Admin.Commands.InviteOrganizationUserResponse"];
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User is not an admin */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Conflict - User already exists or pending invitation exists */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Server Error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/users/{userId}/activate": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Activate a user account. */
        post: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description The ID of the user to activate */
                    userId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description User activated successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.ActivateUserResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.ActivateUserResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.ActivateUserResponse"];
                    };
                };
                /** @description Bad request - Validation failed */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User is not an admin */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found - User not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/admin/users/{userId}/deactivate": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Deactivate a user account. */
        post: {
            parameters: {
                query?: never;
                header?: {
                    "X-Api-Version"?: string;
                };
                path: {
                    /** @description The ID of the user to deactivate */
                    userId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description User deactivated successfully */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.DeactivateUserResponse"];
                        "application/json; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.DeactivateUserResponse"];
                        "text/json; version=1.0": components["schemas"]["IntelliPM.Application.Identity.Commands.DeactivateUserResponse"];
                    };
                };
                /** @description Bad request - Validation failed or cannot deactivate own account */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized - User is not authenticated */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Forbidden - User is not an admin */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found - User not found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Internal server error */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json; version=1.0": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
}
export type webhooks = Record<string, never>;
export interface components {
    schemas: {
        "IntelliPM.API.Controllers.AcceptOrganizationInviteRequest": {
            token?: string;
            username?: string;
            password?: string;
            confirmPassword?: string;
        };
        /** @description Request DTO for adding a comment. */
        "IntelliPM.API.Controllers.AddCommentRequest": {
            /** @description Type of entity (Task, Project, Sprint, Defect, BacklogItem). */
            entityType?: string;
            /**
             * Format: int32
             * @description ID of the entity.
             */
            entityId?: number;
            /** @description Comment content. */
            content?: string;
            /**
             * Format: int32
             * @description Optional parent comment ID for threaded comments.
             */
            parentCommentId?: number | null;
        };
        "IntelliPM.API.Controllers.AddTaskDependencyRequest": {
            /** Format: int32 */
            dependentTaskId?: number;
            dependencyType?: string;
        };
        "IntelliPM.API.Controllers.AddTasksToSprintRequest": {
            taskIds?: number[];
            ignoreCapacityWarning?: boolean | null;
        };
        "IntelliPM.API.Controllers.AddTeamMemberRequest": {
            /** Format: int32 */
            userId?: number;
        };
        /** @description Request model for creating an AI quota template. */
        "IntelliPM.API.Controllers.Admin.CreateAIQuotaTemplateRequest": {
            tierName?: string;
            description?: string | null;
            /** Format: int32 */
            maxTokensPerPeriod?: number;
            /** Format: int32 */
            maxRequestsPerPeriod?: number;
            /** Format: int32 */
            maxDecisionsPerPeriod?: number;
            /** Format: double */
            maxCostPerPeriod?: number;
            allowOverage?: boolean;
            /** Format: double */
            overageRate?: number;
            /** Format: double */
            defaultAlertThresholdPercentage?: number;
            /** Format: int32 */
            displayOrder?: number;
        };
        /** @description Request model for disabling AI for an organization. */
        "IntelliPM.API.Controllers.Admin.DisableAIRequest": {
            reason?: string;
            notifyOrganization?: boolean;
            isPermanent?: boolean;
        };
        /** @description Request model for enabling AI for an organization. */
        "IntelliPM.API.Controllers.Admin.EnableAIRequest": {
            tierName?: string;
            reason?: string;
        };
        "IntelliPM.API.Controllers.Admin.RBACPolicyVersionDto": {
            /** Format: int32 */
            id?: number;
            versionNumber?: string;
            description?: string;
            /** Format: date-time */
            appliedAt?: string;
            /** Format: int32 */
            appliedByUserId?: number | null;
            isActive?: boolean;
            notes?: string | null;
            /** Format: date-time */
            createdAt?: string;
        };
        /** @description Request model for rebuilding projections. */
        "IntelliPM.API.Controllers.Admin.RebuildProjectionRequest": {
            /** @description Type of projection to rebuild: "All", "TaskBoard", "SprintSummary", "ProjectOverview" */
            projectionType?: string | null;
            /**
             * Format: int32
             * @description Optional: Rebuild projections for a specific project only
             */
            projectId?: number | null;
            /**
             * Format: int32
             * @description Optional: Rebuild projections for all projects in a specific organization
             */
            organizationId?: number | null;
            /** @description If true, delete existing read models and rebuild from scratch. */
            forceRebuild?: boolean;
        };
        "IntelliPM.API.Controllers.Admin.RefreshSeedResponse": {
            success?: boolean;
            message?: string;
            results?: components["schemas"]["IntelliPM.API.Controllers.Admin.SeedResult"][];
            policyVersionCreated?: string | null;
        };
        "IntelliPM.API.Controllers.Admin.SeedHistoryDto": {
            /** Format: int32 */
            id?: number;
            seedName?: string;
            version?: string;
            /** Format: date-time */
            appliedAt?: string;
            success?: boolean;
            errorMessage?: string | null;
            /** Format: int32 */
            recordsAffected?: number;
            description?: string | null;
        };
        "IntelliPM.API.Controllers.Admin.SeedResult": {
            seedName?: string;
            version?: string;
            applied?: boolean;
        };
        /** @description Request model for toggling global AI kill switch. */
        "IntelliPM.API.Controllers.Admin.ToggleGlobalAIRequest": {
            enabled?: boolean;
            reason?: string;
        };
        /** @description Request model for updating an AI quota template. */
        "IntelliPM.API.Controllers.Admin.UpdateAIQuotaTemplateRequest": {
            description?: string | null;
            isActive?: boolean | null;
            /** Format: int32 */
            maxTokensPerPeriod?: number | null;
            /** Format: int32 */
            maxRequestsPerPeriod?: number | null;
            /** Format: int32 */
            maxDecisionsPerPeriod?: number | null;
            /** Format: double */
            maxCostPerPeriod?: number | null;
            allowOverage?: boolean | null;
            /** Format: double */
            overageRate?: number | null;
            /** Format: double */
            defaultAlertThresholdPercentage?: number | null;
            /** Format: int32 */
            displayOrder?: number | null;
        };
        /** @description Request model for updating a feature flag. */
        "IntelliPM.API.Controllers.Admin.UpdateFeatureFlagRequest": {
            isEnabled?: boolean | null;
            description?: string | null;
        };
        /** @description Request model for updating member quota. */
        "IntelliPM.API.Controllers.Admin.UpdateMemberQuotaRequest": {
            /** Format: int32 */
            maxTokensPerPeriod?: number | null;
            /** Format: int32 */
            maxRequestsPerPeriod?: number | null;
            /** Format: int32 */
            maxDecisionsPerPeriod?: number | null;
            /** Format: double */
            maxCostPerPeriod?: number | null;
            reason?: string | null;
        };
        /** @description Request model for updating AI quota. */
        "IntelliPM.API.Controllers.Admin.UpdateQuotaRequest": {
            tierName?: string;
            /** Format: int32 */
            maxTokensPerPeriod?: number | null;
            /** Format: int32 */
            maxRequestsPerPeriod?: number | null;
            /** Format: int32 */
            maxDecisionsPerPeriod?: number | null;
            /** Format: double */
            maxCostPerPeriod?: number | null;
            allowOverage?: boolean | null;
            /** Format: double */
            overageRate?: number | null;
            enforceQuota?: boolean | null;
            applyImmediately?: boolean;
            reason?: string | null;
        };
        /** @description Request DTO for updating user global role. */
        "IntelliPM.API.Controllers.Admin.UpdateUserGlobalRoleRequest": {
            /** Format: int32 */
            userId?: number;
            /** @enum {string} */
            globalRole?: "User" | "Admin" | "SuperAdmin";
        };
        /** @description Response model for API health check endpoint */
        "IntelliPM.API.Controllers.ApiHealthResponse": {
            status?: string;
            checks?: components["schemas"]["IntelliPM.API.Controllers.EndpointCheck"][];
            error?: string | null;
            /** Format: date-time */
            timestamp?: string;
        };
        /** @description Request model for approving an AI decision. */
        "IntelliPM.API.Controllers.ApproveDecisionRequest": {
            notes?: string | null;
        };
        "IntelliPM.API.Controllers.AssignTaskRequest": {
            /** Format: int32 */
            assigneeId?: number | null;
        };
        "IntelliPM.API.Controllers.AssignTasksRequest": {
            taskIds?: number[];
        };
        "IntelliPM.API.Controllers.AssignTeamToProjectRequest": {
            /** Format: int32 */
            teamId?: number;
            /** @enum {string|null} */
            defaultRole?: "ProductOwner" | "ScrumMaster" | "Developer" | "Tester" | "Viewer" | "Manager" | null;
            memberRoleOverrides?: {
                [key: string]: "ProductOwner" | "ScrumMaster" | "Developer" | "Tester" | "Viewer" | "Manager" | null;
            } | null;
        };
        "IntelliPM.API.Controllers.BulkUpdateUsersStatusRequest": {
            userIds?: number[];
            isActive?: boolean;
        };
        "IntelliPM.API.Controllers.ChangeRoleRequest": {
            /** @enum {string} */
            newRole?: "ProductOwner" | "ScrumMaster" | "Developer" | "Tester" | "Viewer" | "Manager";
        };
        "IntelliPM.API.Controllers.ChangeTaskStatusRequest": {
            newStatus?: string;
        };
        "IntelliPM.API.Controllers.CompleteSprintRequest": {
            incompleteTasksAction?: string | null;
        };
        "IntelliPM.API.Controllers.CreateBacklogItemRequest": {
            title?: string;
            description?: string;
            /** Format: int32 */
            storyPoints?: number | null;
            domainTag?: string | null;
            /** Format: int32 */
            epicId?: number | null;
            /** Format: int32 */
            featureId?: number | null;
            acceptanceCriteria?: string | null;
        };
        "IntelliPM.API.Controllers.CreateDefectRequest": {
            /** Format: int32 */
            userStoryId?: number | null;
            /** Format: int32 */
            sprintId?: number | null;
            title?: string;
            description?: string;
            severity?: string;
            foundInEnvironment?: string | null;
            stepsToReproduce?: string | null;
            /** Format: int32 */
            assignedToId?: number | null;
        };
        "IntelliPM.API.Controllers.CreateProjectRequest": {
            name?: string;
            description?: string;
            type?: string;
            /** Format: int32 */
            sprintDurationDays?: number;
            status?: string | null;
            /** Format: date-time */
            startDate?: string | null;
            memberIds?: number[] | null;
        };
        "IntelliPM.API.Controllers.CreateSprintRequest": {
            name?: string;
            /** Format: int32 */
            projectId?: number;
            /** Format: date-time */
            startDate?: string;
            /** Format: date-time */
            endDate?: string;
            /** Format: int32 */
            capacity?: number;
            goal?: string | null;
        };
        "IntelliPM.API.Controllers.CreateTaskRequest": {
            title?: string;
            description?: string;
            /** Format: int32 */
            projectId?: number;
            priority?: string;
            /** Format: int32 */
            storyPoints?: number | null;
            /** Format: int32 */
            assigneeId?: number | null;
        };
        /** @description Individual endpoint check result */
        "IntelliPM.API.Controllers.EndpointCheck": {
            endpoint?: string;
            description?: string;
            status?: string;
            /** Format: int32 */
            expectedStatus?: number;
            /** Format: int32 */
            actualStatus?: number;
            message?: string | null;
            /** Format: int32 */
            responseTime?: number;
        };
        "IntelliPM.API.Controllers.ForgotPasswordRequest": {
            emailOrUsername?: string;
        };
        "IntelliPM.API.Controllers.ImproveTaskRequest": {
            description?: string;
        };
        "IntelliPM.API.Controllers.InviteMemberRequest": {
            email?: string;
            /** @enum {string} */
            role?: "ProductOwner" | "ScrumMaster" | "Developer" | "Tester" | "Viewer" | "Manager";
        };
        "IntelliPM.API.Controllers.InviteUserRequest": {
            email?: string;
            /** @enum {string} */
            globalRole?: "User" | "Admin" | "SuperAdmin";
            /** Format: int32 */
            projectId?: number | null;
        };
        "IntelliPM.API.Controllers.LanguageResponse": {
            language?: string;
        };
        "IntelliPM.API.Controllers.LoginRequest": {
            username?: string;
            password?: string;
        };
        /** @description Lookup item with value, label, and optional metadata */
        "IntelliPM.API.Controllers.LookupItem": {
            value?: string;
            label?: string;
            /** Format: int32 */
            displayOrder?: number | null;
            metadata?: components["schemas"]["IntelliPM.API.Controllers.LookupMetadata"];
        };
        /** @description Lookup metadata for styling and display */
        "IntelliPM.API.Controllers.LookupMetadata": {
            color?: string | null;
            icon?: string | null;
            bgColor?: string | null;
            textColor?: string | null;
            borderColor?: string | null;
            dotColor?: string | null;
        };
        /** @description Response containing lookup items */
        "IntelliPM.API.Controllers.LookupResponse": {
            items?: components["schemas"]["IntelliPM.API.Controllers.LookupItem"][];
        };
        /** @description Request DTO for completing a milestone. */
        "IntelliPM.API.Controllers.MilestonesController.CompleteMilestoneRequest": {
            /**
             * Format: date-time
             * @description Optional completion date. If not provided, uses current UTC time.
             */
            completedAt?: string | null;
        };
        /** @description Request DTO for creating a milestone. */
        "IntelliPM.API.Controllers.MilestonesController.CreateMilestoneRequest": {
            /** @description Name of the milestone. */
            name?: string;
            /** @description Optional description of the milestone. */
            description?: string | null;
            /** @description Type of milestone: "Release", "Sprint", "Deadline", "Custom". */
            type?: string;
            /**
             * Format: date-time
             * @description Due date for the milestone.
             */
            dueDate?: string;
            /**
             * Format: int32
             * @description Progress percentage (0-100). Default: 0.
             */
            progress?: number | null;
        };
        /** @description Request DTO for updating a milestone. */
        "IntelliPM.API.Controllers.MilestonesController.UpdateMilestoneRequest": {
            /** @description Updated name of the milestone. */
            name?: string;
            /** @description Updated description of the milestone. */
            description?: string | null;
            /**
             * Format: date-time
             * @description Updated due date for the milestone.
             */
            dueDate?: string;
            /**
             * Format: int32
             * @description Updated progress percentage (0-100).
             */
            progress?: number;
        };
        "IntelliPM.API.Controllers.ProjectPermissionsResponse": {
            permissions?: string[];
            projectRole?: string | null;
            /** Format: int32 */
            projectId?: number;
        };
        "IntelliPM.API.Controllers.RegisterTeamRequest": {
            name?: string;
            memberIds?: number[];
            /** Format: int32 */
            totalCapacity?: number;
        };
        /** @description Request model for rejecting an AI decision. */
        "IntelliPM.API.Controllers.RejectDecisionRequest": {
            notes?: string | null;
            reason?: string;
        };
        "IntelliPM.API.Controllers.ReleasesController.ApproveQualityGateRequest": {
            /** Format: int32 */
            gateType?: number;
        };
        "IntelliPM.API.Controllers.ReleasesController.BulkAddSprintsRequest": {
            sprintIds?: number[];
        };
        "IntelliPM.API.Controllers.ReleasesController.BulkAddSprintsResponse": {
            /** Format: int32 */
            addedCount?: number;
        };
        "IntelliPM.API.Controllers.ReleasesController.CreateReleaseRequest": {
            name?: string;
            version?: string;
            description?: string | null;
            type?: string;
            /** Format: date-time */
            plannedDate?: string;
            isPreRelease?: boolean | null;
            tagName?: string | null;
            sprintIds?: number[] | null;
        };
        "IntelliPM.API.Controllers.ReleasesController.GenerateChangelogResponse": {
            changeLog?: string;
        };
        "IntelliPM.API.Controllers.ReleasesController.GenerateReleaseNotesResponse": {
            releaseNotes?: string;
        };
        "IntelliPM.API.Controllers.ReleasesController.UpdateChangelogRequest": {
            changeLog?: string | null;
            autoGenerate?: boolean;
        };
        "IntelliPM.API.Controllers.ReleasesController.UpdateReleaseNotesRequest": {
            releaseNotes?: string | null;
            autoGenerate?: boolean;
        };
        "IntelliPM.API.Controllers.ReleasesController.UpdateReleaseRequest": {
            name?: string;
            version?: string;
            description?: string | null;
            /** Format: date-time */
            plannedDate?: string;
            status?: string;
        };
        "IntelliPM.API.Controllers.RemoveTasksFromSprintRequest": {
            taskIds?: number[];
        };
        "IntelliPM.API.Controllers.ResetPasswordRequest": {
            token?: string;
            newPassword?: string;
            confirmPassword?: string;
        };
        "IntelliPM.API.Controllers.SendTestEmailRequest": {
            email?: string;
        };
        "IntelliPM.API.Controllers.StoreNoteRequest": {
            type?: string;
            content?: string;
        };
        /** @description Request DTO for updating a comment. */
        "IntelliPM.API.Controllers.UpdateCommentRequest": {
            /** @description New comment content. */
            content?: string;
        };
        "IntelliPM.API.Controllers.UpdateDefectRequest": {
            title?: string | null;
            description?: string | null;
            severity?: string | null;
            status?: string | null;
            /** Format: int32 */
            assignedToId?: number | null;
            foundInEnvironment?: string | null;
            stepsToReproduce?: string | null;
            resolution?: string | null;
        };
        "IntelliPM.API.Controllers.UpdateLanguageRequest": {
            language?: string;
        };
        "IntelliPM.API.Controllers.UpdateProjectRequest": {
            name?: string | null;
            description?: string | null;
            status?: string | null;
            type?: string | null;
            /** Format: int32 */
            sprintDurationDays?: number | null;
        };
        "IntelliPM.API.Controllers.UpdateRolePermissionsRequest": {
            permissionIds?: number[];
        };
        "IntelliPM.API.Controllers.UpdateSettingRequest": {
            value?: string;
            category?: string | null;
        };
        "IntelliPM.API.Controllers.UpdateTaskRequest": {
            title?: string | null;
            description?: string | null;
            priority?: string | null;
            /** Format: int32 */
            storyPoints?: number | null;
        };
        "IntelliPM.API.Controllers.UpdateTeamCapacityRequest": {
            /** Format: int32 */
            newCapacity?: number;
        };
        "IntelliPM.API.Controllers.UpdateUserRequest": {
            firstName?: string | null;
            lastName?: string | null;
            email?: string | null;
            globalRole?: string | null;
        };
        "IntelliPM.API.Controllers.UserPermissionsResponse": {
            permissions?: string[];
            /** @enum {string} */
            globalRole?: "User" | "Admin" | "SuperAdmin";
        };
        "IntelliPM.Application.AI.Commands.DisableAIForOrgResponse": {
            /** Format: int32 */
            organizationId?: number;
            organizationName?: string;
            wasDisabled?: boolean;
            /** @enum {string} */
            mode?: "Temporary" | "Permanent";
            /** Format: date-time */
            disabledAt?: string;
            reason?: string;
        };
        "IntelliPM.Application.AI.Commands.EffectiveQuotaDto": {
            /** Format: int32 */
            maxTokensPerPeriod?: number;
            /** Format: int32 */
            maxRequestsPerPeriod?: number;
            /** Format: int32 */
            maxDecisionsPerPeriod?: number;
            /** Format: double */
            maxCostPerPeriod?: number;
            hasOverride?: boolean;
        };
        "IntelliPM.Application.AI.Commands.EnableAIForOrgResponse": {
            /** Format: int32 */
            organizationId?: number;
            organizationName?: string;
            wasEnabled?: boolean;
            tierName?: string;
        };
        "IntelliPM.Application.AI.Commands.QuotaLimitsDto": {
            /** Format: int32 */
            maxTokensPerPeriod?: number;
            /** Format: int32 */
            maxRequestsPerPeriod?: number;
            /** Format: int32 */
            maxDecisionsPerPeriod?: number;
            /** Format: double */
            maxCostPerPeriod?: number;
            allowOverage?: boolean;
            /** Format: double */
            overageRate?: number;
        };
        "IntelliPM.Application.AI.Commands.QuotaOverrideDto": {
            /** Format: int32 */
            maxTokensPerPeriod?: number | null;
            /** Format: int32 */
            maxRequestsPerPeriod?: number | null;
            /** Format: int32 */
            maxDecisionsPerPeriod?: number | null;
            /** Format: double */
            maxCostPerPeriod?: number | null;
            /** Format: date-time */
            createdAt?: string;
            reason?: string | null;
        };
        "IntelliPM.Application.AI.Commands.QuotaStatus": {
            /** Format: int32 */
            tokensUsed?: number;
            /** Format: int32 */
            tokensLimit?: number;
            /** Format: double */
            tokensPercentage?: number;
            /** Format: int32 */
            requestsUsed?: number;
            /** Format: int32 */
            requestsLimit?: number;
            /** Format: double */
            requestsPercentage?: number;
            /** Format: double */
            costAccumulated?: number;
            /** Format: double */
            costLimit?: number;
            /** Format: double */
            costPercentage?: number;
            isExceeded?: boolean;
            /** Format: int32 */
            daysRemaining?: number;
        };
        "IntelliPM.Application.AI.Commands.ResetUserAIQuotaOverrideResponse": {
            /** Format: int32 */
            userId?: number;
            success?: boolean;
            message?: string;
        };
        "IntelliPM.Application.AI.Commands.ToggleGlobalAIResponse": {
            enabled?: boolean;
            /** Format: date-time */
            toggledAt?: string;
            reason?: string;
            /** Format: int32 */
            updatedById?: number;
        };
        "IntelliPM.Application.AI.Commands.UpdateAIQuotaResponse": {
            /** Format: int32 */
            quotaId?: number;
            /** Format: int32 */
            organizationId?: number;
            tierName?: string;
            limits?: components["schemas"]["IntelliPM.Application.AI.Commands.QuotaLimitsDto"];
            currentStatus?: components["schemas"]["IntelliPM.Application.AI.Commands.QuotaStatus"];
        };
        "IntelliPM.Application.AI.Commands.UpdateUserAIQuotaOverrideResponse": {
            /** Format: int32 */
            overrideId?: number;
            /** Format: int32 */
            userId?: number;
            effectiveQuota?: components["schemas"]["IntelliPM.Application.AI.Commands.EffectiveQuotaDto"];
            override?: components["schemas"]["IntelliPM.Application.AI.Commands.QuotaOverrideDto"];
        };
        "IntelliPM.Application.AI.DTOs.AIOverviewStatsDto": {
            /** Format: int32 */
            totalOrganizations?: number;
            /** Format: int32 */
            organizationsWithAIEnabled?: number;
            /** Format: int32 */
            organizationsWithAIDisabled?: number;
            /** Format: int32 */
            totalDecisionsLast30Days?: number;
            /** Format: int32 */
            pendingApprovals?: number;
            /** Format: int32 */
            approvedDecisions?: number;
            /** Format: int32 */
            rejectedDecisions?: number;
            /** Format: double */
            averageConfidenceScore?: number;
            /** Format: double */
            totalCostLast30Days?: number;
            topAgents?: components["schemas"]["IntelliPM.Application.AI.DTOs.TopAgentUsageDto"][];
            quotaByTier?: components["schemas"]["IntelliPM.Application.AI.DTOs.QuotaUsageByTierDto"][];
        };
        "IntelliPM.Application.AI.DTOs.AIQuotaTemplateDto": {
            /** Format: int32 */
            id?: number;
            tierName?: string;
            description?: string | null;
            isActive?: boolean;
            isSystemTemplate?: boolean;
            /** Format: int32 */
            maxTokensPerPeriod?: number;
            /** Format: int32 */
            maxRequestsPerPeriod?: number;
            /** Format: int32 */
            maxDecisionsPerPeriod?: number;
            /** Format: double */
            maxCostPerPeriod?: number;
            allowOverage?: boolean;
            /** Format: double */
            overageRate?: number;
            /** Format: double */
            defaultAlertThresholdPercentage?: number;
            /** Format: int32 */
            displayOrder?: number;
            /** Format: date-time */
            createdAt?: string;
            /** Format: date-time */
            updatedAt?: string;
        };
        "IntelliPM.Application.AI.DTOs.EffectiveMemberQuotaDto": {
            /** Format: int64 */
            monthlyTokenLimit?: number;
            /** Format: int32 */
            monthlyRequestLimit?: number | null;
            isAIEnabled?: boolean;
            hasOverride?: boolean;
        };
        "IntelliPM.Application.AI.DTOs.MemberAIQuotaDto": {
            /** Format: int32 */
            userId?: number;
            email?: string;
            firstName?: string;
            lastName?: string;
            fullName?: string;
            globalRole?: string;
            /** Format: int32 */
            organizationId?: number;
            organizationName?: string;
            effectiveQuota?: components["schemas"]["IntelliPM.Application.AI.DTOs.EffectiveMemberQuotaDto"];
            override?: components["schemas"]["IntelliPM.Application.AI.DTOs.UserQuotaOverrideDto"];
            organizationQuota?: components["schemas"]["IntelliPM.Application.AI.DTOs.OrganizationQuotaBaseDto"];
        };
        "IntelliPM.Application.AI.DTOs.OrganizationAIQuotaDto": {
            /** Format: int32 */
            id?: number;
            /** Format: int32 */
            organizationId?: number;
            organizationName?: string;
            organizationCode?: string;
            /** Format: int64 */
            monthlyTokenLimit?: number;
            /** Format: int32 */
            monthlyRequestLimit?: number | null;
            /** Format: int32 */
            resetDayOfMonth?: number | null;
            isAIEnabled?: boolean;
            /** Format: date-time */
            createdAt?: string;
            /** Format: date-time */
            updatedAt?: string | null;
        };
        "IntelliPM.Application.AI.DTOs.OrganizationQuotaBaseDto": {
            /** Format: int64 */
            monthlyTokenLimit?: number;
            /** Format: int32 */
            monthlyRequestLimit?: number | null;
            isAIEnabled?: boolean;
        };
        "IntelliPM.Application.AI.DTOs.QuotaUsageByTierDto": {
            tierName?: string;
            /** Format: int32 */
            organizationCount?: number;
            /** Format: double */
            averageUsagePercentage?: number;
            /** Format: int32 */
            exceededCount?: number;
        };
        "IntelliPM.Application.AI.DTOs.TopAgentUsageDto": {
            agentType?: string;
            /** Format: int32 */
            decisionCount?: number;
            /** Format: int64 */
            totalTokensUsed?: number;
        };
        "IntelliPM.Application.AI.DTOs.UpdateMemberAIQuotaRequest": {
            /** Format: int64 */
            monthlyTokenLimitOverride?: number | null;
            /** Format: int32 */
            monthlyRequestLimitOverride?: number | null;
            isAIEnabledOverride?: boolean | null;
        };
        "IntelliPM.Application.AI.DTOs.UpdateOrganizationAIQuotaRequest": {
            /** Format: int64 */
            monthlyTokenLimit?: number;
            /** Format: int32 */
            monthlyRequestLimit?: number | null;
            /** Format: int32 */
            resetDayOfMonth?: number | null;
            isAIEnabled?: boolean | null;
        };
        "IntelliPM.Application.AI.DTOs.UserQuotaOverrideDto": {
            /** Format: int64 */
            monthlyTokenLimitOverride?: number | null;
            /** Format: int32 */
            monthlyRequestLimitOverride?: number | null;
            isAIEnabledOverride?: boolean | null;
            /** Format: date-time */
            createdAt?: string;
            /** Format: date-time */
            updatedAt?: string | null;
        };
        "IntelliPM.Application.AI.Queries.AIDecisionLogDetailDto": {
            /** Format: uuid */
            decisionId?: string;
            decisionType?: string;
            agentType?: string;
            entityType?: string;
            /** Format: int32 */
            entityId?: number;
            entityName?: string;
            question?: string;
            decision?: string;
            reasoning?: string;
            /** Format: double */
            confidenceScore?: number;
            modelName?: string;
            modelVersion?: string;
            /** Format: int32 */
            tokensUsed?: number;
            /** Format: int32 */
            promptTokens?: number;
            /** Format: int32 */
            completionTokens?: number;
            status?: string;
            requiresHumanApproval?: boolean;
            approvedByHuman?: boolean | null;
            /** Format: int32 */
            approvedByUserId?: number | null;
            /** Format: date-time */
            approvedAt?: string | null;
            approvalNotes?: string | null;
            wasApplied?: boolean;
            /** Format: date-time */
            appliedAt?: string | null;
            actualOutcome?: string | null;
            /** Format: date-time */
            createdAt?: string;
            /** Format: int32 */
            executionTimeMs?: number;
            isSuccess?: boolean;
            errorMessage?: string | null;
        };
        "IntelliPM.Application.AI.Queries.AIDecisionLogDto": {
            /** Format: uuid */
            decisionId?: string;
            decisionType?: string;
            agentType?: string;
            entityType?: string;
            /** Format: int32 */
            entityId?: number;
            entityName?: string;
            question?: string;
            decision?: string;
            /** Format: double */
            confidenceScore?: number;
            status?: string;
            requiresHumanApproval?: boolean;
            approvedByHuman?: boolean | null;
            /** Format: date-time */
            createdAt?: string;
            /** Format: int32 */
            tokensUsed?: number;
            /** Format: double */
            costAccumulated?: number;
        };
        "IntelliPM.Application.AI.Queries.AIQuotaBreakdownDto": {
            byAgent?: {
                [key: string]: components["schemas"]["IntelliPM.Application.AI.Queries.AgentBreakdownDto"];
            };
            byDecisionType?: {
                [key: string]: components["schemas"]["IntelliPM.Application.AI.Queries.DecisionTypeBreakdownDto"];
            };
            summary?: components["schemas"]["IntelliPM.Application.AI.Queries.PeriodSummaryDto"];
        };
        "IntelliPM.Application.AI.Queries.AIQuotaDto": {
            /** Format: int32 */
            id?: number;
            /** Format: int32 */
            organizationId?: number;
            organizationName?: string;
            tierName?: string;
            isActive?: boolean;
            usage?: components["schemas"]["IntelliPM.Application.AI.Queries.QuotaUsageDto"];
            /** Format: date-time */
            periodEndDate?: string;
            isExceeded?: boolean;
            alertSent?: boolean;
        };
        "IntelliPM.Application.AI.Queries.AIQuotaStatusDto": {
            /** Format: int32 */
            quotaId?: number;
            tierName?: string;
            isActive?: boolean;
            usage?: components["schemas"]["IntelliPM.Application.AI.Queries.QuotaUsageDto"];
            /** Format: date-time */
            periodEndDate?: string;
            /** Format: int32 */
            daysRemaining?: number;
            isExceeded?: boolean;
            alertSent?: boolean;
        };
        "IntelliPM.Application.AI.Queries.AIUsageStatisticsDto": {
            /** Format: int32 */
            totalTokensUsed?: number;
            /** Format: int32 */
            totalRequests?: number;
            /** Format: int32 */
            totalDecisions?: number;
            /** Format: double */
            totalCost?: number;
            usageByAgent?: {
                [key: string]: components["schemas"]["IntelliPM.Application.AI.Queries.AgentUsageStatsDto"];
            };
            usageByDecisionType?: {
                [key: string]: components["schemas"]["IntelliPM.Application.AI.Queries.DecisionTypeStatsDto"];
            };
            dailyUsage?: components["schemas"]["IntelliPM.Application.AI.Queries.DailyUsageDto"][];
        };
        "IntelliPM.Application.AI.Queries.AdminAiQuotaMemberDto": {
            /** Format: int32 */
            userId?: number;
            email?: string;
            firstName?: string;
            lastName?: string;
            fullName?: string;
            userRole?: string;
            /** Format: int32 */
            organizationId?: number;
            organizationName?: string;
            effectiveQuota?: components["schemas"]["IntelliPM.Application.AI.Queries.EffectiveQuotaDto"];
            override?: components["schemas"]["IntelliPM.Application.AI.Queries.QuotaOverrideDto"];
            usage?: components["schemas"]["IntelliPM.Application.AI.Queries.UserUsageDto"];
            period?: components["schemas"]["IntelliPM.Application.AI.Queries.PeriodInfoDto"];
        };
        "IntelliPM.Application.AI.Queries.AgentBreakdownDto": {
            agentType?: string;
            /** Format: int32 */
            requests?: number;
            /** Format: int32 */
            tokens?: number;
            /** Format: int32 */
            decisions?: number;
            /** Format: double */
            cost?: number;
            /** Format: double */
            percentageOfTotalTokens?: number;
        };
        "IntelliPM.Application.AI.Queries.AgentUsageStatsDto": {
            /** Format: int32 */
            tokensUsed?: number;
            /** Format: int32 */
            requestsCount?: number;
        };
        "IntelliPM.Application.AI.Queries.DailyUsageDto": {
            /** Format: date-time */
            date?: string;
            /** Format: int32 */
            tokensUsed?: number;
            /** Format: int32 */
            requestsCount?: number;
            /** Format: int32 */
            decisionsCount?: number;
        };
        "IntelliPM.Application.AI.Queries.DailyUsageHistoryDto": {
            /** Format: date-time */
            date?: string;
            /** Format: int32 */
            requests?: number;
            /** Format: int32 */
            tokens?: number;
            /** Format: int32 */
            decisions?: number;
            /** Format: double */
            cost?: number;
        };
        "IntelliPM.Application.AI.Queries.DecisionTypeBreakdownDto": {
            decisionType?: string;
            /** Format: int32 */
            decisions?: number;
            /** Format: int32 */
            tokens?: number;
            /** Format: double */
            cost?: number;
            /** Format: double */
            percentageOfTotalDecisions?: number;
        };
        "IntelliPM.Application.AI.Queries.DecisionTypeStatsDto": {
            /** Format: int32 */
            tokensUsed?: number;
            /** Format: int32 */
            decisionsCount?: number;
        };
        "IntelliPM.Application.AI.Queries.EffectiveQuotaDto": {
            /** Format: int32 */
            maxTokensPerPeriod?: number;
            /** Format: int32 */
            maxRequestsPerPeriod?: number;
            /** Format: int32 */
            maxDecisionsPerPeriod?: number;
            /** Format: double */
            maxCostPerPeriod?: number;
            hasOverride?: boolean;
        };
        "IntelliPM.Application.AI.Queries.GlobalAIStatusResponse": {
            enabled?: boolean;
            /** Format: date-time */
            lastUpdated?: string | null;
            /** Format: int32 */
            updatedById?: number | null;
            reason?: string | null;
        };
        "IntelliPM.Application.AI.Queries.PeriodInfoDto": {
            /** Format: date-time */
            periodStartDate?: string;
            /** Format: date-time */
            periodEndDate?: string;
            /** Format: int32 */
            daysRemaining?: number;
        };
        "IntelliPM.Application.AI.Queries.PeriodSummaryDto": {
            /** Format: date-time */
            startDate?: string;
            /** Format: date-time */
            endDate?: string;
            /** Format: int32 */
            totalRequests?: number;
            /** Format: int32 */
            totalTokens?: number;
            /** Format: int32 */
            totalDecisions?: number;
            /** Format: double */
            totalCost?: number;
        };
        "IntelliPM.Application.AI.Queries.QuotaOverrideDto": {
            /** Format: int32 */
            maxTokensPerPeriod?: number | null;
            /** Format: int32 */
            maxRequestsPerPeriod?: number | null;
            /** Format: int32 */
            maxDecisionsPerPeriod?: number | null;
            /** Format: double */
            maxCostPerPeriod?: number | null;
            /** Format: date-time */
            createdAt?: string;
            reason?: string | null;
        };
        "IntelliPM.Application.AI.Queries.QuotaUsageDto": {
            /** Format: int32 */
            tokensUsed?: number;
            /** Format: int32 */
            tokensLimit?: number;
            /** Format: double */
            tokensPercentage?: number;
            /** Format: int32 */
            requestsUsed?: number;
            /** Format: int32 */
            requestsLimit?: number;
            /** Format: double */
            requestsPercentage?: number;
            /** Format: double */
            costAccumulated?: number;
            /** Format: double */
            costLimit?: number;
            /** Format: double */
            costPercentage?: number;
        };
        "IntelliPM.Application.AI.Queries.UserUsageDto": {
            /** Format: int32 */
            tokensUsed?: number;
            /** Format: int32 */
            requestsUsed?: number;
            /** Format: int32 */
            decisionsMade?: number;
            /** Format: double */
            costAccumulated?: number;
            /** Format: double */
            tokensPercentage?: number;
            /** Format: double */
            requestsPercentage?: number;
            /** Format: double */
            decisionsPercentage?: number;
            /** Format: double */
            costPercentage?: number;
        };
        "IntelliPM.Application.Activity.Queries.ActivityDto": {
            /** Format: int32 */
            id?: number;
            type?: string;
            /** Format: int32 */
            userId?: number;
            userName?: string;
            userAvatar?: string | null;
            entityType?: string;
            /** Format: int32 */
            entityId?: number;
            entityName?: string | null;
            /** Format: int32 */
            projectId?: number;
            projectName?: string | null;
            /** Format: date-time */
            timestamp?: string;
        };
        "IntelliPM.Application.Activity.Queries.GetRecentActivityResponse": {
            activities?: components["schemas"]["IntelliPM.Application.Activity.Queries.ActivityDto"][];
        };
        "IntelliPM.Application.Admin.AuditLogs.Queries.AuditLogDto": {
            /** Format: int32 */
            id?: number;
            /** Format: int32 */
            userId?: number | null;
            userName?: string | null;
            action?: string;
            entityType?: string;
            /** Format: int32 */
            entityId?: number | null;
            entityName?: string | null;
            changes?: string | null;
            ipAddress?: string | null;
            userAgent?: string | null;
            /** Format: date-time */
            createdAt?: string;
        };
        "IntelliPM.Application.Admin.Commands.InviteOrganizationUserCommand": {
            email?: string;
            /** @enum {string} */
            role?: "User" | "Admin" | "SuperAdmin";
            firstName?: string;
            lastName?: string;
        };
        "IntelliPM.Application.Admin.Commands.InviteOrganizationUserResponse": {
            /** Format: uuid */
            invitationId?: string;
            email?: string;
            invitationLink?: string;
        };
        "IntelliPM.Application.Admin.Dashboard.Queries.AdminDashboardStatsDto": {
            /** Format: int32 */
            totalUsers?: number;
            /** Format: int32 */
            activeUsers?: number;
            /** Format: int32 */
            inactiveUsers?: number;
            /** Format: int32 */
            adminCount?: number;
            /** Format: int32 */
            userCount?: number;
            /** Format: int32 */
            totalProjects?: number;
            /** Format: int32 */
            activeProjects?: number;
            /** Format: int32 */
            totalOrganizations?: number;
            userGrowth?: components["schemas"]["IntelliPM.Application.Admin.Dashboard.Queries.UserGrowthDto"][];
            recentActivities?: components["schemas"]["IntelliPM.Application.Admin.Dashboard.Queries.RecentActivityDto"][];
            systemHealth?: components["schemas"]["IntelliPM.Application.Admin.SystemHealth.Queries.SystemHealthDto"];
        };
        "IntelliPM.Application.Admin.Dashboard.Queries.RecentActivityDto": {
            action?: string;
            userName?: string;
            /** Format: date-time */
            timestamp?: string;
        };
        "IntelliPM.Application.Admin.Dashboard.Queries.UserGrowthDto": {
            month?: string;
            /** Format: int32 */
            count?: number;
        };
        "IntelliPM.Application.Admin.DeadLetterQueue.Queries.DeadLetterMessageDto": {
            /** Format: uuid */
            id?: string;
            /** Format: uuid */
            originalMessageId?: string;
            eventType?: string;
            payload?: string;
            /** Format: date-time */
            originalCreatedAt?: string;
            /** Format: date-time */
            movedToDlqAt?: string;
            /** Format: int32 */
            totalRetryAttempts?: number;
            lastError?: string;
            idempotencyKey?: string | null;
        };
        "IntelliPM.Application.Admin.SystemHealth.Queries.ExternalServiceStatus": {
            name?: string;
            isHealthy?: boolean;
            statusMessage?: string | null;
            /** Format: int32 */
            responseTimeMs?: number | null;
            /** Format: date-time */
            lastChecked?: string | null;
        };
        "IntelliPM.Application.Admin.SystemHealth.Queries.SystemHealthDto": {
            /** Format: double */
            cpuUsage?: number;
            /** Format: double */
            memoryUsage?: number;
            /** Format: int64 */
            totalMemoryBytes?: number;
            /** Format: int64 */
            usedMemoryBytes?: number;
            /** Format: int64 */
            availableMemoryBytes?: number;
            databaseStatus?: string;
            databaseResponseTimeMs?: string;
            externalServices?: {
                [key: string]: components["schemas"]["IntelliPM.Application.Admin.SystemHealth.Queries.ExternalServiceStatus"];
            };
            /** Format: int32 */
            deadLetterQueueCount?: number;
            /** Format: date-time */
            timestamp?: string;
        };
        "IntelliPM.Application.Agent.Queries.AgentExecutionLogDto": {
            /** Format: uuid */
            id?: string;
            agentId?: string;
            agentType?: string;
            userId?: string;
            userInput?: string;
            agentResponse?: string | null;
            toolsCalled?: string | null;
            status?: string;
            success?: boolean;
            /** Format: int32 */
            executionTimeMs?: number;
            /** Format: int32 */
            tokensUsed?: number;
            /** Format: double */
            executionCostUsd?: number;
            /** Format: date-time */
            createdAt?: string;
            errorMessage?: string | null;
            /** Format: int32 */
            linkedDecisionId?: number | null;
        };
        "IntelliPM.Application.Agent.Queries.AgentMetricsDto": {
            /** Format: int32 */
            totalExecutions?: number;
            /** Format: int32 */
            successfulExecutions?: number;
            /** Format: int32 */
            failedExecutions?: number;
            /** Format: double */
            successRate?: number;
            /** Format: int32 */
            averageExecutionTimeMs?: number;
            /** Format: double */
            totalCostUsd?: number;
            /** Format: date-time */
            lastExecutionAt?: string | null;
            byAgentType?: components["schemas"]["IntelliPM.Application.Agent.Queries.AgentTypeMetric"][];
        };
        "IntelliPM.Application.Agent.Queries.AgentTypeMetric": {
            agentId?: string;
            /** Format: int32 */
            executionCount?: number;
            /** Format: int32 */
            successCount?: number;
            /** Format: int32 */
            failureCount?: number;
            /** Format: double */
            successRate?: number;
            /** Format: int32 */
            avgExecutionTimeMs?: number;
        };
        "IntelliPM.Application.Agent.Queries.GetAgentAuditLogsResponse": {
            logs?: components["schemas"]["IntelliPM.Application.Agent.Queries.AgentExecutionLogDto"][];
            /** Format: int32 */
            totalCount?: number;
            /** Format: int32 */
            page?: number;
            /** Format: int32 */
            pageSize?: number;
            /** Format: int32 */
            totalPages?: number;
        };
        "IntelliPM.Application.Attachments.Queries.AttachmentDto": {
            /** Format: int32 */
            id?: number;
            entityType?: string;
            /** Format: int32 */
            entityId?: number;
            fileName?: string;
            fileExtension?: string;
            /** Format: int64 */
            fileSizeBytes?: number;
            contentType?: string;
            /** Format: int32 */
            uploadedById?: number;
            uploadedBy?: string;
            /** Format: date-time */
            uploadedAt?: string;
        };
        "IntelliPM.Application.Backlog.Queries.BacklogTaskDto": {
            /** Format: int32 */
            id?: number;
            title?: string;
            description?: string;
            priority?: string;
            status?: string;
            /** Format: int32 */
            storyPoints?: number | null;
            /** Format: int32 */
            assigneeId?: number | null;
            assigneeName?: string | null;
            /** Format: date-time */
            createdAt?: string;
            /** Format: int32 */
            priorityOrder?: number;
        };
        "IntelliPM.Application.Comments.Commands.AddCommentResponse": {
            /** Format: int32 */
            commentId?: number;
            /** Format: int32 */
            authorId?: number;
            authorName?: string;
            content?: string;
            /** Format: date-time */
            createdAt?: string;
            mentionedUserIds?: number[];
        };
        "IntelliPM.Application.Comments.Queries.CommentDto": {
            /** Format: int32 */
            id?: number;
            entityType?: string;
            /** Format: int32 */
            entityId?: number;
            content?: string;
            /** Format: int32 */
            authorId?: number;
            authorName?: string;
            /** Format: date-time */
            createdAt?: string;
            /** Format: date-time */
            updatedAt?: string | null;
            isEdited?: boolean;
            /** Format: int32 */
            parentCommentId?: number | null;
        };
        "IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.DTOs.MemberAIQuotaDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]": {
            items?: components["schemas"]["IntelliPM.Application.AI.DTOs.MemberAIQuotaDto"][];
            /** Format: int32 */
            page?: number;
            /** Format: int32 */
            pageSize?: number;
            /** Format: int32 */
            totalCount?: number;
            /** Format: int32 */
            readonly totalPages?: number;
            readonly hasPrevious?: boolean;
            readonly hasNext?: boolean;
        };
        "IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.DTOs.OrganizationAIQuotaDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]": {
            items?: components["schemas"]["IntelliPM.Application.AI.DTOs.OrganizationAIQuotaDto"][];
            /** Format: int32 */
            page?: number;
            /** Format: int32 */
            pageSize?: number;
            /** Format: int32 */
            totalCount?: number;
            /** Format: int32 */
            readonly totalPages?: number;
            readonly hasPrevious?: boolean;
            readonly hasNext?: boolean;
        };
        "IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.AIDecisionLogDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]": {
            items?: components["schemas"]["IntelliPM.Application.AI.Queries.AIDecisionLogDto"][];
            /** Format: int32 */
            page?: number;
            /** Format: int32 */
            pageSize?: number;
            /** Format: int32 */
            totalCount?: number;
            /** Format: int32 */
            readonly totalPages?: number;
            readonly hasPrevious?: boolean;
            readonly hasNext?: boolean;
        };
        "IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.AIQuotaDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]": {
            items?: components["schemas"]["IntelliPM.Application.AI.Queries.AIQuotaDto"][];
            /** Format: int32 */
            page?: number;
            /** Format: int32 */
            pageSize?: number;
            /** Format: int32 */
            totalCount?: number;
            /** Format: int32 */
            readonly totalPages?: number;
            readonly hasPrevious?: boolean;
            readonly hasNext?: boolean;
        };
        "IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.AdminAiQuotaMemberDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]": {
            items?: components["schemas"]["IntelliPM.Application.AI.Queries.AdminAiQuotaMemberDto"][];
            /** Format: int32 */
            page?: number;
            /** Format: int32 */
            pageSize?: number;
            /** Format: int32 */
            totalCount?: number;
            /** Format: int32 */
            readonly totalPages?: number;
            readonly hasPrevious?: boolean;
            readonly hasNext?: boolean;
        };
        "IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.AI.Queries.DailyUsageHistoryDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]": {
            items?: components["schemas"]["IntelliPM.Application.AI.Queries.DailyUsageHistoryDto"][];
            /** Format: int32 */
            page?: number;
            /** Format: int32 */
            pageSize?: number;
            /** Format: int32 */
            totalCount?: number;
            /** Format: int32 */
            readonly totalPages?: number;
            readonly hasPrevious?: boolean;
            readonly hasNext?: boolean;
        };
        "IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Admin.AuditLogs.Queries.AuditLogDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]": {
            items?: components["schemas"]["IntelliPM.Application.Admin.AuditLogs.Queries.AuditLogDto"][];
            /** Format: int32 */
            page?: number;
            /** Format: int32 */
            pageSize?: number;
            /** Format: int32 */
            totalCount?: number;
            /** Format: int32 */
            readonly totalPages?: number;
            readonly hasPrevious?: boolean;
            readonly hasNext?: boolean;
        };
        "IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Admin.DeadLetterQueue.Queries.DeadLetterMessageDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]": {
            items?: components["schemas"]["IntelliPM.Application.Admin.DeadLetterQueue.Queries.DeadLetterMessageDto"][];
            /** Format: int32 */
            page?: number;
            /** Format: int32 */
            pageSize?: number;
            /** Format: int32 */
            totalCount?: number;
            /** Format: int32 */
            readonly totalPages?: number;
            readonly hasPrevious?: boolean;
            readonly hasNext?: boolean;
        };
        "IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Backlog.Queries.BacklogTaskDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]": {
            items?: components["schemas"]["IntelliPM.Application.Backlog.Queries.BacklogTaskDto"][];
            /** Format: int32 */
            page?: number;
            /** Format: int32 */
            pageSize?: number;
            /** Format: int32 */
            totalCount?: number;
            /** Format: int32 */
            readonly totalPages?: number;
            readonly hasPrevious?: boolean;
            readonly hasNext?: boolean;
        };
        "IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Identity.DTOs.UserListDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]": {
            items?: components["schemas"]["IntelliPM.Application.Identity.DTOs.UserListDto"][];
            /** Format: int32 */
            page?: number;
            /** Format: int32 */
            pageSize?: number;
            /** Format: int32 */
            totalCount?: number;
            /** Format: int32 */
            readonly totalPages?: number;
            readonly hasPrevious?: boolean;
            readonly hasNext?: boolean;
        };
        "IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Organizations.Queries.OrganizationDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]": {
            items?: components["schemas"]["IntelliPM.Application.Organizations.Queries.OrganizationDto"][];
            /** Format: int32 */
            page?: number;
            /** Format: int32 */
            pageSize?: number;
            /** Format: int32 */
            totalCount?: number;
            /** Format: int32 */
            readonly totalPages?: number;
            readonly hasPrevious?: boolean;
            readonly hasNext?: boolean;
        };
        "IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Permissions.DTOs.MemberPermissionDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]": {
            items?: components["schemas"]["IntelliPM.Application.Permissions.DTOs.MemberPermissionDto"][];
            /** Format: int32 */
            page?: number;
            /** Format: int32 */
            pageSize?: number;
            /** Format: int32 */
            totalCount?: number;
            /** Format: int32 */
            readonly totalPages?: number;
            readonly hasPrevious?: boolean;
            readonly hasNext?: boolean;
        };
        "IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Projections.Queries.ProjectOverviewReadModelDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]": {
            items?: components["schemas"]["IntelliPM.Application.Projections.Queries.ProjectOverviewReadModelDto"][];
            /** Format: int32 */
            page?: number;
            /** Format: int32 */
            pageSize?: number;
            /** Format: int32 */
            totalCount?: number;
            /** Format: int32 */
            readonly totalPages?: number;
            readonly hasPrevious?: boolean;
            readonly hasNext?: boolean;
        };
        "IntelliPM.Application.Common.Models.PagedResponse`1[[IntelliPM.Application.Projects.Queries.ProjectListDto, IntelliPM.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]": {
            items?: components["schemas"]["IntelliPM.Application.Projects.Queries.ProjectListDto"][];
            /** Format: int32 */
            page?: number;
            /** Format: int32 */
            pageSize?: number;
            /** Format: int32 */
            totalCount?: number;
            /** Format: int32 */
            readonly totalPages?: number;
            readonly hasPrevious?: boolean;
            readonly hasNext?: boolean;
        };
        "IntelliPM.Application.DTOs.Agent.AgentResponse": {
            content?: string;
            status?: string;
            requiresApproval?: boolean;
            /** Format: double */
            executionCostUsd?: number;
            /** Format: int32 */
            executionTimeMs?: number;
            toolsCalled?: string[];
            /** Format: date-time */
            timestamp?: string;
            errorMessage?: string | null;
            metadata?: {
                [key: string]: unknown;
            } | null;
            /** Format: int32 */
            promptTokens?: number;
            /** Format: int32 */
            completionTokens?: number;
            /** Format: int32 */
            readonly totalTokens?: number;
            model?: string;
        };
        "IntelliPM.Application.Defects.Commands.CreateDefectResponse": {
            /** Format: int32 */
            id?: number;
            title?: string;
            severity?: string;
            status?: string;
        };
        "IntelliPM.Application.Defects.Commands.UpdateDefectResponse": {
            /** Format: int32 */
            id?: number;
            title?: string;
            severity?: string;
            status?: string;
            /** Format: date-time */
            updatedAt?: string;
        };
        "IntelliPM.Application.Defects.Queries.DefectDetailDto": {
            /** Format: int32 */
            id?: number;
            title?: string;
            description?: string;
            severity?: string;
            status?: string;
            /** Format: int32 */
            projectId?: number;
            /** Format: int32 */
            userStoryId?: number | null;
            userStoryTitle?: string | null;
            /** Format: int32 */
            sprintId?: number | null;
            sprintName?: string | null;
            /** Format: int32 */
            reportedById?: number | null;
            reportedByName?: string | null;
            /** Format: int32 */
            assignedToId?: number | null;
            assignedToName?: string | null;
            foundInEnvironment?: string | null;
            stepsToReproduce?: string | null;
            resolution?: string | null;
            /** Format: date-time */
            reportedAt?: string;
            /** Format: date-time */
            resolvedAt?: string | null;
            /** Format: date-time */
            updatedAt?: string;
        };
        "IntelliPM.Application.Defects.Queries.DefectDto": {
            /** Format: int32 */
            id?: number;
            title?: string;
            description?: string;
            severity?: string;
            status?: string;
            /** Format: int32 */
            userStoryId?: number | null;
            userStoryTitle?: string | null;
            /** Format: int32 */
            assignedToId?: number | null;
            assignedToName?: string | null;
            /** Format: int32 */
            reportedById?: number | null;
            reportedByName?: string | null;
            foundInEnvironment?: string | null;
            /** Format: date-time */
            reportedAt?: string;
            /** Format: date-time */
            resolvedAt?: string | null;
            /** Format: date-time */
            updatedAt?: string;
        };
        "IntelliPM.Application.Defects.Queries.GetProjectDefectsResponse": {
            defects?: components["schemas"]["IntelliPM.Application.Defects.Queries.DefectDto"][];
            /** Format: int32 */
            total?: number;
        };
        "IntelliPM.Application.FeatureFlags.Commands.CreateFeatureFlagCommand": {
            name?: string;
            description?: string | null;
            isEnabled?: boolean;
            /** Format: int32 */
            organizationId?: number | null;
        };
        "IntelliPM.Application.FeatureFlags.Queries.FeatureFlagDto": {
            /** Format: uuid */
            id?: string;
            name?: string;
            isEnabled?: boolean;
            /** Format: int32 */
            organizationId?: number | null;
            description?: string | null;
            /** Format: date-time */
            createdAt?: string;
            /** Format: date-time */
            updatedAt?: string | null;
            isGlobal?: boolean;
            isOrganizationSpecific?: boolean;
        };
        "IntelliPM.Application.Features.Milestones.DTOs.MilestoneDto": {
            /** Format: int32 */
            id?: number;
            /** Format: int32 */
            projectId?: number;
            name?: string;
            description?: string;
            type?: string;
            status?: string;
            /** Format: date-time */
            dueDate?: string;
            /** Format: date-time */
            completedAt?: string | null;
            /** Format: int32 */
            progress?: number;
            /** Format: int32 */
            daysUntilDue?: number;
            isOverdue?: boolean;
            /** Format: date-time */
            createdAt?: string;
            createdByName?: string;
        };
        "IntelliPM.Application.Features.Milestones.DTOs.MilestoneStatisticsDto": {
            /** Format: int32 */
            totalMilestones?: number;
            /** Format: int32 */
            completedMilestones?: number;
            /** Format: int32 */
            missedMilestones?: number;
            /** Format: int32 */
            upcomingMilestones?: number;
            /** Format: int32 */
            pendingMilestones?: number;
            /** Format: int32 */
            inProgressMilestones?: number;
            /** Format: double */
            completionRate?: number;
        };
        "IntelliPM.Application.Features.Releases.DTOs.QualityGateDto": {
            /** Format: int32 */
            id?: number;
            /** Format: int32 */
            releaseId?: number;
            type?: string;
            status?: string;
            isRequired?: boolean;
            /** Format: double */
            threshold?: number | null;
            /** Format: double */
            actualValue?: number | null;
            message?: string;
            details?: string | null;
            /** Format: date-time */
            checkedAt?: string | null;
            checkedByName?: string | null;
        };
        "IntelliPM.Application.Features.Releases.DTOs.ReleaseDto": {
            /** Format: int32 */
            id?: number;
            /** Format: int32 */
            projectId?: number;
            name?: string;
            version?: string;
            description?: string | null;
            type?: string;
            status?: string;
            /** Format: date-time */
            plannedDate?: string;
            /** Format: date-time */
            actualReleaseDate?: string | null;
            releaseNotes?: string | null;
            changeLog?: string | null;
            isPreRelease?: boolean;
            tagName?: string | null;
            /** Format: int32 */
            sprintCount?: number;
            /** Format: int32 */
            completedTasksCount?: number;
            /** Format: int32 */
            totalTasksCount?: number;
            overallQualityStatus?: string | null;
            qualityGates?: components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.QualityGateDto"][] | null;
            /** Format: date-time */
            createdAt?: string;
            createdByName?: string;
            releasedByName?: string | null;
            sprints?: components["schemas"]["IntelliPM.Application.Features.Releases.DTOs.ReleaseSprintDto"][] | null;
        };
        "IntelliPM.Application.Features.Releases.DTOs.ReleaseSprintDto": {
            /** Format: int32 */
            id?: number;
            name?: string;
            /** Format: date-time */
            startDate?: string | null;
            /** Format: date-time */
            endDate?: string | null;
            status?: string;
            /** Format: int32 */
            completedTasksCount?: number;
            /** Format: int32 */
            totalTasksCount?: number;
            /** Format: int32 */
            completionPercentage?: number;
        };
        "IntelliPM.Application.Features.Releases.DTOs.ReleaseStatisticsDto": {
            /** Format: int32 */
            totalReleases?: number;
            /** Format: int32 */
            deployedReleases?: number;
            /** Format: int32 */
            plannedReleases?: number;
            /** Format: int32 */
            failedReleases?: number;
            /** Format: double */
            averageLeadTime?: number;
        };
        "IntelliPM.Application.Identity.Commands.AcceptInviteResponse": {
            /** Format: int32 */
            userId?: number;
            username?: string;
            email?: string;
            accessToken?: string;
            refreshToken?: string;
        };
        "IntelliPM.Application.Identity.Commands.ActivateUserResponse": {
            /** Format: int32 */
            userId?: number;
            isActive?: boolean;
            username?: string;
            email?: string;
        };
        "IntelliPM.Application.Identity.Commands.DeactivateUserResponse": {
            /** Format: int32 */
            userId?: number;
            isActive?: boolean;
            username?: string;
            email?: string;
        };
        "IntelliPM.Application.Identity.Commands.InviteUserResponse": {
            /** Format: int32 */
            invitationId?: number;
            email?: string;
            token?: string;
            /** Format: date-time */
            expiresAt?: string;
        };
        "IntelliPM.Application.Identity.Commands.RequestPasswordResetResponse": {
            success?: boolean;
            message?: string;
        };
        "IntelliPM.Application.Identity.Commands.ResetPasswordResponse": {
            success?: boolean;
            message?: string;
        };
        "IntelliPM.Application.Identity.DTOs.UserListDto": {
            /** Format: int32 */
            id?: number;
            username?: string;
            email?: string;
            firstName?: string | null;
            lastName?: string | null;
            /** @enum {string} */
            role?: "User" | "Admin" | "SuperAdmin";
            isActive?: boolean;
            /** Format: int32 */
            organizationId?: number;
            organizationName?: string;
            /** Format: date-time */
            createdAt?: string;
            /** Format: date-time */
            lastLoginAt?: string | null;
        };
        "IntelliPM.Application.Identity.Queries.CurrentUserDto": {
            /** Format: int32 */
            userId?: number;
            username?: string;
            email?: string;
            firstName?: string | null;
            lastName?: string | null;
            /** @enum {string} */
            globalRole?: "User" | "Admin" | "SuperAdmin";
            /** Format: int32 */
            organizationId?: number;
            permissions?: string[];
        };
        "IntelliPM.Application.Identity.Queries.ValidateInviteTokenResponse": {
            email?: string;
            organizationName?: string;
        };
        "IntelliPM.Application.Notifications.Queries.GetNotificationsResponse": {
            notifications?: components["schemas"]["IntelliPM.Application.Notifications.Queries.NotificationDto"][];
            /** Format: int32 */
            unreadCount?: number;
        };
        "IntelliPM.Application.Notifications.Queries.GetUnreadNotificationCountResponse": {
            /** Format: int32 */
            unreadCount?: number;
        };
        "IntelliPM.Application.Notifications.Queries.NotificationDto": {
            /** Format: int32 */
            id?: number;
            type?: string;
            message?: string;
            entityType?: string | null;
            /** Format: int32 */
            entityId?: number | null;
            /** Format: int32 */
            projectId?: number | null;
            isRead?: boolean;
            /** Format: date-time */
            createdAt?: string;
        };
        "IntelliPM.Application.Organizations.Commands.CreateOrganizationCommand": {
            name?: string;
            code?: string;
        };
        "IntelliPM.Application.Organizations.Commands.CreateOrganizationResponse": {
            /** Format: int32 */
            organizationId?: number;
            name?: string;
            code?: string;
            /** Format: date-time */
            createdAt?: string;
        };
        "IntelliPM.Application.Organizations.Commands.DeleteOrganizationResponse": {
            /** Format: int32 */
            organizationId?: number;
            success?: boolean;
            message?: string;
        };
        "IntelliPM.Application.Organizations.Commands.UpdateOrganizationCommand": {
            /** Format: int32 */
            organizationId?: number;
            name?: string;
            code?: string;
        };
        "IntelliPM.Application.Organizations.Commands.UpdateOrganizationResponse": {
            /** Format: int32 */
            organizationId?: number;
            name?: string;
            code?: string;
            /** Format: date-time */
            updatedAt?: string;
        };
        "IntelliPM.Application.Organizations.Commands.UpdateUserGlobalRoleResponse": {
            /** Format: int32 */
            userId?: number;
            /** @enum {string} */
            globalRole?: "User" | "Admin" | "SuperAdmin";
            message?: string;
        };
        "IntelliPM.Application.Organizations.DTOs.OrganizationPermissionPolicyDto": {
            /** Format: int32 */
            id?: number;
            /** Format: int32 */
            organizationId?: number;
            organizationName?: string;
            organizationCode?: string;
            allowedPermissions?: string[];
            isActive?: boolean;
            /** Format: date-time */
            createdAt?: string;
            /** Format: date-time */
            updatedAt?: string | null;
        };
        "IntelliPM.Application.Organizations.DTOs.UpdateOrganizationPermissionPolicyRequest": {
            allowedPermissions?: string[];
            isActive?: boolean | null;
        };
        "IntelliPM.Application.Organizations.Queries.OrganizationDto": {
            /** Format: int32 */
            id?: number;
            name?: string;
            code?: string;
            /** Format: date-time */
            createdAt?: string;
            /** Format: date-time */
            updatedAt?: string | null;
            /** Format: int32 */
            userCount?: number;
        };
        "IntelliPM.Application.Permissions.DTOs.MemberPermissionDto": {
            /** Format: int32 */
            userId?: number;
            email?: string;
            firstName?: string;
            lastName?: string;
            fullName?: string;
            globalRole?: string;
            /** Format: int32 */
            organizationId?: number;
            organizationName?: string;
            permissions?: string[];
            permissionIds?: number[];
        };
        "IntelliPM.Application.Permissions.DTOs.UpdateMemberPermissionRequest": {
            globalRole?: string | null;
            permissionIds?: number[] | null;
        };
        "IntelliPM.Application.Permissions.Queries.PermissionDto": {
            /** Format: int32 */
            id?: number;
            name?: string;
            category?: string;
            description?: string | null;
        };
        "IntelliPM.Application.Permissions.Queries.PermissionsMatrixDto": {
            permissions?: components["schemas"]["IntelliPM.Application.Permissions.Queries.PermissionDto"][];
            rolePermissions?: {
                [key: string]: number[];
            };
        };
        "IntelliPM.Application.Projections.Commands.RebuildProjectionResponse": {
            /** Format: int32 */
            projectionsRebuilt?: number;
            rebuildDetails?: string[];
            duration?: components["schemas"]["System.TimeSpan"];
            success?: boolean;
            error?: string | null;
        };
        "IntelliPM.Application.Projections.Queries.ProjectOverviewReadModelDto": {
            /** Format: int32 */
            projectId?: number;
            projectName?: string;
            projectType?: string;
            status?: string;
            /** Format: int32 */
            ownerId?: number;
            ownerName?: string;
            /** Format: int32 */
            totalMembers?: number;
            /** Format: int32 */
            activeMembers?: number;
            teamMembers?: components["schemas"]["IntelliPM.Domain.Entities.MemberSummaryDto"][];
            /** Format: int32 */
            totalSprints?: number;
            /** Format: int32 */
            activeSprintsCount?: number;
            /** Format: int32 */
            completedSprintsCount?: number;
            /** Format: int32 */
            currentSprintId?: number | null;
            currentSprintName?: string | null;
            /** Format: int32 */
            totalTasks?: number;
            /** Format: int32 */
            completedTasks?: number;
            /** Format: int32 */
            inProgressTasks?: number;
            /** Format: int32 */
            todoTasks?: number;
            /** Format: int32 */
            blockedTasks?: number;
            /** Format: int32 */
            overdueTasks?: number;
            /** Format: int32 */
            totalStoryPoints?: number;
            /** Format: int32 */
            completedStoryPoints?: number;
            /** Format: int32 */
            remainingStoryPoints?: number;
            /** Format: int32 */
            totalDefects?: number;
            /** Format: int32 */
            openDefects?: number;
            /** Format: int32 */
            criticalDefects?: number;
            /** Format: double */
            averageVelocity?: number;
            /** Format: double */
            lastSprintVelocity?: number;
            velocityTrend?: components["schemas"]["IntelliPM.Domain.Entities.VelocityTrendDto"][];
            /** Format: double */
            projectHealth?: number;
            healthStatus?: string;
            riskFactors?: string[];
            /** Format: date-time */
            lastActivityAt?: string;
            /** Format: int32 */
            activitiesLast7Days?: number;
            /** Format: int32 */
            activitiesLast30Days?: number;
            /** Format: double */
            overallProgress?: number;
            /** Format: double */
            sprintProgress?: number;
            /** Format: int32 */
            daysUntilNextMilestone?: number;
            /** Format: date-time */
            lastUpdated?: string;
            /** Format: int32 */
            version?: number;
        };
        "IntelliPM.Application.Projections.Queries.SprintSummaryReadModelDto": {
            /** Format: int32 */
            sprintId?: number;
            sprintName?: string;
            status?: string;
            /** Format: date-time */
            startDate?: string;
            /** Format: date-time */
            endDate?: string;
            /** Format: int32 */
            plannedCapacity?: number | null;
            /** Format: int32 */
            totalTasks?: number;
            /** Format: int32 */
            completedTasks?: number;
            /** Format: int32 */
            inProgressTasks?: number;
            /** Format: int32 */
            todoTasks?: number;
            /** Format: int32 */
            totalStoryPoints?: number;
            /** Format: int32 */
            completedStoryPoints?: number;
            /** Format: int32 */
            inProgressStoryPoints?: number;
            /** Format: int32 */
            remainingStoryPoints?: number;
            /** Format: double */
            completionPercentage?: number;
            /** Format: double */
            velocityPercentage?: number;
            /** Format: double */
            capacityUtilization?: number;
            /** Format: int32 */
            estimatedDaysRemaining?: number;
            isOnTrack?: boolean;
            /** Format: double */
            averageVelocity?: number;
            burndownData?: components["schemas"]["IntelliPM.Domain.Entities.BurndownPointDto"][];
            /** Format: date-time */
            lastUpdated?: string;
            /** Format: int32 */
            version?: number;
        };
        "IntelliPM.Application.Projections.Queries.TaskBoardReadModelDto": {
            /** Format: int32 */
            projectId?: number;
            /** Format: int32 */
            todoCount?: number;
            /** Format: int32 */
            inProgressCount?: number;
            /** Format: int32 */
            doneCount?: number;
            /** Format: int32 */
            totalTaskCount?: number;
            /** Format: int32 */
            todoStoryPoints?: number;
            /** Format: int32 */
            inProgressStoryPoints?: number;
            /** Format: int32 */
            doneStoryPoints?: number;
            /** Format: int32 */
            totalStoryPoints?: number;
            todoTasks?: components["schemas"]["IntelliPM.Domain.Entities.TaskSummaryDto"][];
            inProgressTasks?: components["schemas"]["IntelliPM.Domain.Entities.TaskSummaryDto"][];
            doneTasks?: components["schemas"]["IntelliPM.Domain.Entities.TaskSummaryDto"][];
            /** Format: date-time */
            lastUpdated?: string;
            /** Format: int32 */
            version?: number;
        };
        "IntelliPM.Application.Projects.Commands.AssignTeamToProjectResponse": {
            /** Format: int32 */
            projectId?: number;
            /** Format: int32 */
            teamId?: number;
            assignedMembers?: components["schemas"]["IntelliPM.Application.Projects.Commands.AssignedMemberDto"][];
        };
        "IntelliPM.Application.Projects.Commands.AssignedMemberDto": {
            /** Format: int32 */
            userId?: number;
            username?: string;
            /** @enum {string} */
            role?: "ProductOwner" | "ScrumMaster" | "Developer" | "Tester" | "Viewer" | "Manager";
            alreadyMember?: boolean;
        };
        "IntelliPM.Application.Projects.Commands.CreateProjectResponse": {
            /** Format: int32 */
            id?: number;
            name?: string;
            description?: string;
            type?: string;
        };
        "IntelliPM.Application.Projects.Commands.UpdateProjectResponse": {
            /** Format: int32 */
            id?: number;
            name?: string;
            description?: string;
            type?: string;
            status?: string;
            /** Format: int32 */
            sprintDurationDays?: number;
            /** Format: date-time */
            updatedAt?: string;
        };
        "IntelliPM.Application.Projects.Queries.GetProjectByIdResponse": {
            /** Format: int32 */
            id?: number;
            name?: string;
            description?: string;
            type?: string;
            status?: string;
            members?: components["schemas"]["IntelliPM.Application.Projects.Queries.ProjectMemberDto"][];
            /** Format: date-time */
            createdAt?: string;
        };
        "IntelliPM.Application.Projects.Queries.ProjectAssignedTeamDto": {
            /** Format: int32 */
            teamId?: number;
            teamName?: string;
            teamDescription?: string | null;
            /** Format: date-time */
            assignedAt?: string;
            /** Format: int32 */
            assignedById?: number | null;
            assignedByName?: string | null;
            isActive?: boolean;
        };
        "IntelliPM.Application.Projects.Queries.ProjectListDto": {
            /** Format: int32 */
            id?: number;
            name?: string;
            description?: string;
            type?: string;
            status?: string;
            /** Format: date-time */
            createdAt?: string;
            members?: components["schemas"]["IntelliPM.Application.Projects.Queries.ProjectMemberListDto"][];
        };
        "IntelliPM.Application.Projects.Queries.ProjectMemberDto": {
            /** Format: int32 */
            id?: number;
            /** Format: int32 */
            userId?: number;
            userName?: string;
            email?: string;
            /** @enum {string} */
            role?: "ProductOwner" | "ScrumMaster" | "Developer" | "Tester" | "Viewer" | "Manager";
            /** Format: date-time */
            invitedAt?: string;
            invitedByName?: string;
        };
        "IntelliPM.Application.Projects.Queries.ProjectMemberListDto": {
            /** Format: int32 */
            userId?: number;
            firstName?: string;
            lastName?: string;
            email?: string;
            avatar?: string | null;
        };
        "IntelliPM.Application.Queries.Metrics.BurndownDayData": {
            /** Format: int32 */
            day?: number;
            /** Format: int32 */
            ideal?: number;
            /** Format: int32 */
            actual?: number;
        };
        "IntelliPM.Application.Queries.Metrics.DefectSeverityData": {
            severity?: string;
            /** Format: int32 */
            count?: number;
        };
        "IntelliPM.Application.Queries.Metrics.DefectsBySeverityResponse": {
            defects?: components["schemas"]["IntelliPM.Application.Queries.Metrics.DefectSeverityData"][];
        };
        "IntelliPM.Application.Queries.Metrics.MetricsSummaryDto": {
            /** Format: int32 */
            totalProjects?: number;
            /** Format: int32 */
            totalTasks?: number;
            /** Format: int32 */
            completedTasks?: number;
            /** Format: int32 */
            inProgressTasks?: number;
            /** Format: int32 */
            blockedTasks?: number;
            /** Format: int32 */
            todoTasks?: number;
            /** Format: int32 */
            openTasks?: number;
            /** Format: double */
            completionPercentage?: number;
            /** Format: double */
            averageCompletionTimeHours?: number;
            /** Format: int32 */
            totalSprints?: number;
            /** Format: int32 */
            activeSprints?: number;
            /** Format: double */
            velocity?: number;
            /** Format: int32 */
            defectsCount?: number;
            /** Format: int32 */
            totalDefects?: number;
            /** Format: int32 */
            totalAgentExecutions?: number;
            /** Format: double */
            agentSuccessRate?: number;
            /** Format: int32 */
            averageAgentResponseTimeMs?: number;
            trends?: components["schemas"]["IntelliPM.Application.Queries.Metrics.TrendData"];
        };
        "IntelliPM.Application.Queries.Metrics.SprintBurndownResponse": {
            days?: components["schemas"]["IntelliPM.Application.Queries.Metrics.BurndownDayData"][];
        };
        "IntelliPM.Application.Queries.Metrics.SprintVelocityChartResponse": {
            sprints?: components["schemas"]["IntelliPM.Application.Queries.Metrics.SprintVelocityData"][];
        };
        "IntelliPM.Application.Queries.Metrics.SprintVelocityData": {
            /** Format: int32 */
            number?: number;
            /** Format: int32 */
            storyPoints?: number;
            /** Format: date-time */
            completedDate?: string;
        };
        "IntelliPM.Application.Queries.Metrics.TaskDistributionData": {
            status?: string;
            /** Format: int32 */
            count?: number;
        };
        "IntelliPM.Application.Queries.Metrics.TaskDistributionResponse": {
            distribution?: components["schemas"]["IntelliPM.Application.Queries.Metrics.TaskDistributionData"][];
        };
        "IntelliPM.Application.Queries.Metrics.TeamVelocityData": {
            /** Format: date-time */
            date?: string;
            /** Format: int32 */
            storyPoints?: number;
            /** Format: int32 */
            sprintNumber?: number;
        };
        "IntelliPM.Application.Queries.Metrics.TeamVelocityResponse": {
            velocity?: components["schemas"]["IntelliPM.Application.Queries.Metrics.TeamVelocityData"][];
        };
        "IntelliPM.Application.Queries.Metrics.TrendData": {
            /** Format: double */
            projectsTrend?: number;
            /** Format: double */
            sprintsTrend?: number;
            /** Format: double */
            openTasksTrend?: number;
            /** Format: double */
            blockedTasksTrend?: number;
            /** Format: double */
            defectsTrend?: number;
            /** Format: double */
            velocityTrend?: number;
        };
        "IntelliPM.Application.Reports.DTOs.AIDecisionRoleReportDto": {
            role?: string;
            /** Format: int32 */
            decisionsApproved?: number;
            /** Format: int32 */
            decisionsRejected?: number;
            /** Format: int32 */
            decisionsPending?: number;
            /** Format: double */
            averageResponseTimeHours?: number;
            /** Format: int32 */
            uniqueApprovers?: number;
            /** Format: double */
            averageConfidenceScore?: number;
        };
        "IntelliPM.Application.Reports.DTOs.RoleActivityReportDto": {
            role?: string;
            actionType?: string;
            /** Format: int32 */
            count?: number;
            /** Format: date-time */
            lastPerformed?: string | null;
            /** Format: int32 */
            uniqueUsers?: number;
        };
        "IntelliPM.Application.Reports.DTOs.WorkflowRoleReportDto": {
            role?: string;
            fromStatus?: string;
            toStatus?: string;
            entityType?: string;
            /** Format: int32 */
            transitionCount?: number;
            /** Format: date-time */
            lastTransition?: string | null;
            /** Format: int32 */
            uniqueUsers?: number;
        };
        "IntelliPM.Application.Search.Queries.SearchResponse": {
            results?: components["schemas"]["IntelliPM.Application.Search.Queries.SearchResultDto"][];
        };
        "IntelliPM.Application.Search.Queries.SearchResultDto": {
            type?: string;
            /** Format: int32 */
            id?: number;
            title?: string;
            description?: string | null;
            subtitle?: string | null;
            url?: string | null;
        };
        "IntelliPM.Application.Settings.Commands.SendTestEmailResponse": {
            success?: boolean;
            message?: string;
        };
        "IntelliPM.Application.Settings.Commands.UpdateSettingResponse": {
            key?: string;
            value?: string;
            category?: string;
        };
        "IntelliPM.Application.Sprints.Commands.AddTaskToSprintResponse": {
            /** Format: int32 */
            sprintId?: number;
            sprintName?: string;
            addedTasks?: components["schemas"]["IntelliPM.Application.Sprints.Commands.TaskAddedDto"][];
            capacity?: components["schemas"]["IntelliPM.Application.Sprints.Commands.SprintCapacityDto"];
            isOverCapacity?: boolean;
            capacityWarning?: string | null;
        };
        "IntelliPM.Application.Sprints.Commands.AssignTasksToSprintResponse": {
            /** Format: int32 */
            sprintId?: number;
            /** Format: int32 */
            assignedCount?: number;
        };
        "IntelliPM.Application.Sprints.Commands.CompleteSprintResponse": {
            /** Format: int32 */
            id?: number;
            status?: string;
            /** Format: date-time */
            endDate?: string;
            /** Format: date-time */
            updatedAt?: string;
            /** Format: int32 */
            completedTasksCount?: number;
            /** Format: int32 */
            totalTasksCount?: number;
            /** Format: int32 */
            velocity?: number;
            /** Format: double */
            completionRate?: number;
        };
        "IntelliPM.Application.Sprints.Commands.RemoveTaskFromSprintResponse": {
            /** Format: int32 */
            sprintId?: number;
            sprintName?: string;
            removedTasks?: components["schemas"]["IntelliPM.Application.Sprints.Commands.TaskRemovedDto"][];
            updatedCapacity?: components["schemas"]["IntelliPM.Application.Sprints.Commands.SprintCapacityDto"];
        };
        "IntelliPM.Application.Sprints.Commands.SprintCapacityDto": {
            /** Format: int32 */
            totalStoryPoints?: number;
            /** Format: int32 */
            plannedCapacity?: number;
            /** Format: int32 */
            remainingCapacity?: number;
            /** Format: double */
            capacityUtilization?: number;
        };
        "IntelliPM.Application.Sprints.Commands.StartSprintResponse": {
            /** Format: int32 */
            id?: number;
            status?: string;
            /** Format: date-time */
            startDate?: string;
            /** Format: date-time */
            updatedAt?: string;
        };
        "IntelliPM.Application.Sprints.Commands.TaskAddedDto": {
            /** Format: int32 */
            taskId?: number;
            title?: string;
            /** Format: int32 */
            storyPoints?: number | null;
            wasAlreadyInSprint?: boolean;
        };
        "IntelliPM.Application.Sprints.Commands.TaskRemovedDto": {
            /** Format: int32 */
            taskId?: number;
            title?: string;
            /** Format: int32 */
            storyPoints?: number | null;
            wasInSprint?: boolean;
        };
        "IntelliPM.Application.Sprints.Queries.GetSprintsByProjectResponse": {
            sprints?: components["schemas"]["IntelliPM.Application.Sprints.Queries.SprintListDto"][];
        };
        "IntelliPM.Application.Sprints.Queries.SprintDetailDto": {
            /** Format: int32 */
            id?: number;
            /** Format: int32 */
            projectId?: number;
            projectName?: string;
            name?: string;
            /** Format: date-time */
            startDate?: string;
            /** Format: date-time */
            endDate?: string;
            goal?: string | null;
            status?: string;
            tasks?: components["schemas"]["IntelliPM.Application.Sprints.Queries.SprintTaskDto"][];
            /** Format: date-time */
            createdAt?: string;
            /** Format: date-time */
            updatedAt?: string;
        };
        "IntelliPM.Application.Sprints.Queries.SprintDto": {
            /** Format: int32 */
            id?: number;
            /** Format: int32 */
            projectId?: number;
            projectName?: string;
            /** Format: int32 */
            number?: number;
            goal?: string;
            /** Format: date-time */
            startDate?: string | null;
            /** Format: date-time */
            endDate?: string | null;
            status?: string;
            /** Format: int32 */
            taskCount?: number;
            /** Format: date-time */
            createdAt?: string;
        };
        "IntelliPM.Application.Sprints.Queries.SprintListDto": {
            /** Format: int32 */
            id?: number;
            name?: string;
            /** Format: date-time */
            startDate?: string;
            /** Format: date-time */
            endDate?: string;
            goal?: string | null;
            status?: string;
            /** Format: int32 */
            taskCount?: number;
            /** Format: date-time */
            createdAt?: string;
        };
        "IntelliPM.Application.Sprints.Queries.SprintTaskDto": {
            /** Format: int32 */
            id?: number;
            title?: string;
            status?: string;
            priority?: string;
            /** Format: int32 */
            storyPoints?: number | null;
        };
        "IntelliPM.Application.Sprints.Queries.SprintVelocityDto": {
            /** Format: int32 */
            sprintId?: number;
            sprintName?: string;
            /** Format: date-time */
            startDate?: string;
            /** Format: date-time */
            endDate?: string | null;
            /** Format: int32 */
            completedStoryPoints?: number;
            /** Format: int32 */
            plannedStoryPoints?: number;
            /** Format: int32 */
            totalTasks?: number;
            /** Format: int32 */
            completedTasks?: number;
            /** Format: double */
            completionRate?: number;
        };
        "IntelliPM.Application.Sprints.Queries.SprintVelocityResponse": {
            /** Format: int32 */
            projectId?: number;
            sprints?: components["schemas"]["IntelliPM.Application.Sprints.Queries.SprintVelocityDto"][];
            /** Format: double */
            averageVelocity?: number;
            /** Format: int32 */
            totalCompletedStoryPoints?: number;
        };
        "IntelliPM.Application.Tasks.Commands.AssignTaskResponse": {
            /** Format: int32 */
            id?: number;
            /** Format: int32 */
            assigneeId?: number | null;
            /** Format: date-time */
            updatedAt?: string;
        };
        "IntelliPM.Application.Tasks.Commands.ChangeTaskStatusResponse": {
            /** Format: int32 */
            id?: number;
            status?: string;
            /** Format: date-time */
            updatedAt?: string;
        };
        "IntelliPM.Application.Tasks.DTOs.DependencyGraphDto": {
            nodes?: components["schemas"]["IntelliPM.Application.Tasks.DTOs.DependencyGraphNodeDto"][];
            edges?: components["schemas"]["IntelliPM.Application.Tasks.DTOs.DependencyGraphEdgeDto"][];
        };
        "IntelliPM.Application.Tasks.DTOs.DependencyGraphEdgeDto": {
            /** Format: int32 */
            id?: number;
            /** Format: int32 */
            sourceTaskId?: number;
            /** Format: int32 */
            dependentTaskId?: number;
            dependencyType?: string;
            label?: string;
        };
        "IntelliPM.Application.Tasks.DTOs.DependencyGraphNodeDto": {
            /** Format: int32 */
            taskId?: number;
            title?: string;
            status?: string;
            /** Format: int32 */
            assigneeId?: number | null;
            assigneeName?: string | null;
        };
        "IntelliPM.Application.Tasks.DTOs.TaskDependencyDto": {
            /** Format: int32 */
            id?: number;
            /** Format: int32 */
            sourceTaskId?: number;
            sourceTaskTitle?: string;
            /** Format: int32 */
            dependentTaskId?: number;
            dependentTaskTitle?: string;
            dependencyType?: string;
            /** Format: date-time */
            createdAt?: string;
            createdByName?: string;
        };
        "IntelliPM.Application.Tasks.Queries.GetBlockedTasksResponse": {
            tasks?: components["schemas"]["IntelliPM.Application.Tasks.Queries.TaskDto"][];
        };
        "IntelliPM.Application.Tasks.Queries.GetTasksByProjectResponse": {
            tasks?: components["schemas"]["IntelliPM.Application.Tasks.Queries.TaskDto"][];
        };
        "IntelliPM.Application.Tasks.Queries.TaskDto": {
            /** Format: int32 */
            id?: number;
            /** Format: int32 */
            projectId?: number;
            projectName?: string;
            title?: string;
            description?: string;
            status?: string;
            priority?: string;
            /** Format: int32 */
            storyPoints?: number | null;
            /** Format: int32 */
            assigneeId?: number | null;
            assigneeName?: string | null;
            /** Format: int32 */
            sprintId?: number | null;
            sprintName?: string | null;
            /** Format: int32 */
            createdById?: number;
            createdByName?: string;
            /** Format: int32 */
            updatedById?: number | null;
            updatedByName?: string | null;
            /** Format: date-time */
            createdAt?: string;
            /** Format: date-time */
            updatedAt?: string;
        };
        "IntelliPM.Application.Teams.Commands.UpdateTeamCapacityResponse": {
            /** Format: int32 */
            id?: number;
            /** Format: int32 */
            capacity?: number;
            /** Format: date-time */
            updatedAt?: string;
        };
        "IntelliPM.Application.Teams.Queries.GetAllTeamsResponse": {
            teams?: components["schemas"]["IntelliPM.Application.Teams.Queries.TeamSummaryDto"][];
        };
        "IntelliPM.Application.Teams.Queries.TeamCapacityDto": {
            /** Format: int32 */
            teamId?: number;
            teamName?: string;
            /** Format: int32 */
            totalCapacity?: number;
            /** Format: int32 */
            assignedStoryPoints?: number;
            /** Format: int32 */
            availableCapacity?: number;
            /** Format: int32 */
            activeSprintId?: number | null;
            activeSprintName?: string | null;
        };
        "IntelliPM.Application.Teams.Queries.TeamDto": {
            /** Format: int32 */
            id?: number;
            name?: string;
            /** Format: int32 */
            capacity?: number;
            members?: components["schemas"]["IntelliPM.Application.Teams.Queries.TeamMemberDto"][];
            /** Format: date-time */
            createdAt?: string;
            /** Format: date-time */
            updatedAt?: string;
        };
        "IntelliPM.Application.Teams.Queries.TeamMemberDto": {
            /** Format: int32 */
            userId?: number;
            username?: string;
            email?: string;
            firstName?: string | null;
            lastName?: string | null;
        };
        "IntelliPM.Application.Teams.Queries.TeamSummaryDto": {
            /** Format: int32 */
            id?: number;
            name?: string;
            /** Format: int32 */
            capacity?: number;
            /** Format: int32 */
            memberCount?: number;
            /** Format: date-time */
            createdAt?: string;
        };
        "IntelliPM.Application.Users.Commands.BulkUpdateUsersStatusResponse": {
            /** Format: int32 */
            successCount?: number;
            /** Format: int32 */
            failureCount?: number;
            errors?: string[];
        };
        "IntelliPM.Application.Users.Commands.DeleteUserResponse": {
            success?: boolean;
        };
        "IntelliPM.Application.Users.Commands.UpdateUserResponse": {
            /** Format: int32 */
            id?: number;
            username?: string;
            email?: string;
            firstName?: string;
            lastName?: string;
            globalRole?: string;
        };
        "IntelliPM.Domain.Entities.BurndownPointDto": {
            /** Format: date-time */
            date?: string;
            /** Format: int32 */
            remainingStoryPoints?: number;
            /** Format: int32 */
            idealRemainingPoints?: number;
        };
        "IntelliPM.Domain.Entities.MemberSummaryDto": {
            /** Format: int32 */
            userId?: number;
            username?: string;
            role?: string;
            /** Format: int32 */
            tasksAssigned?: number;
            /** Format: int32 */
            tasksCompleted?: number;
        };
        "IntelliPM.Domain.Entities.TaskSummaryDto": {
            /** Format: int32 */
            id?: number;
            title?: string;
            priority?: string;
            /** Format: int32 */
            storyPoints?: number | null;
            /** Format: int32 */
            assigneeId?: number | null;
            assigneeName?: string | null;
            assigneeAvatar?: string | null;
            /** Format: date-time */
            dueDate?: string | null;
            /** Format: int32 */
            displayOrder?: number;
        };
        "IntelliPM.Domain.Entities.VelocityTrendDto": {
            sprintName?: string;
            /** Format: int32 */
            velocity?: number;
            /** Format: date-time */
            date?: string;
        };
        "Microsoft.AspNetCore.Mvc.ProblemDetails": {
            type?: string | null;
            title?: string | null;
            /** Format: int32 */
            status?: number | null;
            detail?: string | null;
            instance?: string | null;
        } & {
            [key: string]: unknown;
        };
        "System.TimeSpan": {
            /** Format: int64 */
            ticks?: number;
            /** Format: int32 */
            readonly days?: number;
            /** Format: int32 */
            readonly hours?: number;
            /** Format: int32 */
            readonly milliseconds?: number;
            /** Format: int32 */
            readonly microseconds?: number;
            /** Format: int32 */
            readonly nanoseconds?: number;
            /** Format: int32 */
            readonly minutes?: number;
            /** Format: int32 */
            readonly seconds?: number;
            /** Format: double */
            readonly totalDays?: number;
            /** Format: double */
            readonly totalHours?: number;
            /** Format: double */
            readonly totalMilliseconds?: number;
            /** Format: double */
            readonly totalMicroseconds?: number;
            /** Format: double */
            readonly totalNanoseconds?: number;
            /** Format: double */
            readonly totalMinutes?: number;
            /** Format: double */
            readonly totalSeconds?: number;
        };
    };
    responses: never;
    parameters: never;
    requestBodies: never;
    headers: never;
    pathItems: never;
}
export type $defs = Record<string, never>;
export type operations = Record<string, never>;
