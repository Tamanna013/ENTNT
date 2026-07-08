using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetMind.Api.Data;
using FleetMind.Api.Models;
using FleetMind.Api.DTOs.Documents;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Repositories;

public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
{
    public DocumentRepository(FleetMindDbContext context) : base(context)
    {
    }

    public async Task<(List<Document> items, int totalCount)> GetPagedAsync(DocumentQueryDto query)
    {
        var dbQuery = _dbSet
            .Include(d => d.Versions)
                .ThenInclude(v => v.Attachment)
            .Include(d => d.Versions)
                .ThenInclude(v => v.UploadedByUser)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            dbQuery = dbQuery.Where(d => d.Category == query.Category);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            dbQuery = dbQuery.Where(d => d.EntityName == query.EntityName);
        }

        if (query.EntityId.HasValue)
        {
            dbQuery = dbQuery.Where(d => d.EntityId == query.EntityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            dbQuery = dbQuery.Where(d => d.Title.Contains(query.SearchTerm) || 
                                        (d.Description != null && d.Description.Contains(query.SearchTerm)));
        }

        var totalCount = await dbQuery.CountAsync();

        if (query.SortBy?.ToLower() == "title")
        {
            dbQuery = query.SortDescending
                ? dbQuery.OrderByDescending(d => d.Title)
                : dbQuery.OrderBy(d => d.Title);
        }
        else
        {
            // Default sort by CreatedAt
            dbQuery = query.SortDescending
                ? dbQuery.OrderByDescending(d => d.CreatedAt)
                : dbQuery.OrderBy(d => d.CreatedAt);
        }

        var items = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddVersionAsync(DocumentVersion version)
    {
        await _context.Set<DocumentVersion>().AddAsync(version);
    }

    public async Task<List<DocumentVersion>> GetVersionsAsync(Guid documentId)
    {
        return await _context.Set<DocumentVersion>()
            .Where(v => v.DocumentId == documentId && !v.IsDeleted)
            .Include(v => v.Attachment)
            .Include(v => v.UploadedByUser)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();
    }

    public async Task<DocumentVersion?> GetVersionAsync(Guid documentId, int versionNumber)
    {
        return await _context.Set<DocumentVersion>()
            .Include(v => v.Attachment)
            .FirstOrDefaultAsync(v => v.DocumentId == documentId && v.VersionNumber == versionNumber && !v.IsDeleted);
    }
}
