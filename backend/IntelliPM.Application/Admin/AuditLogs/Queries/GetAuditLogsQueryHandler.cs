using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Models;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace IntelliPM.Application.Admin.AuditLogs.Queries;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedResponse<AuditLogDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetAuditLogsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResponse<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        // Ensure only admins can access audit logs
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only administrators can view audit logs.");
        }

        var auditLogRepo = _unitOfWork.Repository<AuditLog>();
        var query = auditLogRepo.Query()
            .AsNoTracking()
            .Include(al => al.User)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.Action))
        {
            query = query.Where(al => al.Action.Contains(request.Action));
        }

        if (!string.IsNullOrEmpty(request.EntityType))
        {
            query = query.Where(al => al.EntityType == request.EntityType);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(al => al.UserId == request.UserId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(al => al.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(al => al.CreatedAt <= request.EndDate.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100));

        var auditLogs = await query
            .OrderByDescending(al => al.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(al => new AuditLogDto(
                al.Id,
                al.UserId,
                al.User != null ? al.User.Username : null,
                al.Action,
                al.EntityType,
                al.EntityId,
                al.EntityName,
                al.Changes,
                al.IpAddress,
                al.UserAgent,
                al.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResponse<AuditLogDto>(
            auditLogs,
            page,
            pageSize,
            totalCount
        );
    }
}

