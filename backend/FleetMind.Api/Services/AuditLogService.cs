using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Audit;
using FleetMind.Api.Repositories;
using System;

namespace FleetMind.Api.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AuditLogService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<AuditLogDto>> GetAuditLogsAsync(AuditLogQueryDto query)
    {
        query.PageSize = Math.Min(query.PageSize, 100);

        var (items, totalCount) = await _unitOfWork.AuditLogs.GetPagedAsync(query);

        return new PagedResultDto<AuditLogDto>
        {
            Items = _mapper.Map<List<AuditLogDto>>(items),
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}
