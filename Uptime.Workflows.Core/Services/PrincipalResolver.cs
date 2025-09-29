using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Services;

/// <summary>
/// Resolves Principal records by SID, creating placeholder entries as needed.
/// </summary>
public interface IPrincipalResolver
{
    Task<Principal?> TryResolveBySidAsync(PrincipalSid sid, CancellationToken ct);

    /// <summary>
    /// Ensures the principal for the given SID exists in the DB (and cache) and returns it.
    /// </summary>
    Task<Principal> ResolveBySidAsync(PrincipalSid sid, CancellationToken ct);

    /// <summary>
    /// Ensures all principals for the given SIDs exist in the DB (and cache).
    /// Does not return data; use for cache-priming before mass task creation.
    /// </summary>
    Task EnsurePrincipalsCachedAsync(IEnumerable<PrincipalSid> sids, CancellationToken ct);

    /// <summary>
    /// Ensures all principals for the given SIDs exist in the DB (and cache), and returns their DTOs.
    /// Use for logging, notifications, or recipient list scenarios.
    /// </summary>
    Task<IReadOnlyList<Principal>> GetPrincipalsBySidAsync(IEnumerable<PrincipalSid> sids, CancellationToken ct);
}

/// <summary>
/// Implements IPrincipalResolver using EF Core and a per-request memory cache.
/// </summary>
public sealed class PrincipalResolver : IPrincipalResolver
{
    private readonly Dictionary<string, Principal> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly IDbContextFactory<WorkflowDbContext> _factory;
    private readonly ILogger<PrincipalResolver> _logger;

    private static string Normalize(string sid) => sid.Trim();

    public PrincipalResolver(IDbContextFactory<WorkflowDbContext> factory, ILogger<PrincipalResolver> logger)
    {
        _cache[Principal.System.Sid.Value] = Principal.System;
        _factory = factory;
        _logger = logger;
    }

    public async Task<Principal?> TryResolveBySidAsync(PrincipalSid sid, CancellationToken ct)
    {
        string key = Normalize(sid.Value);
        if (_cache.TryGetValue(key, out Principal? cached))
            return cached;

        await using WorkflowDbContext db = await _factory.CreateDbContextAsync(ct);
        WorkflowPrincipal? entity = await db.Set<WorkflowPrincipal>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ExternalId == key, ct);

        if (entity is null)
            return null;

        Principal dto = ToDto(entity);
        _cache[key] = dto;
        return dto;
    }

    public async Task<Principal> ResolveBySidAsync(PrincipalSid sid, CancellationToken ct)
    {
        string key = Normalize(sid.Value);
        if (_cache.TryGetValue(key, out Principal? cached))
            return cached;

        await using WorkflowDbContext db = await _factory.CreateDbContextAsync(ct);
        await EnsurePrincipalsCachedInternalAsync([key], db, ct);
        return _cache.TryGetValue(key, out Principal? principal) 
            ? principal 
            : throw new InvalidOperationException($"Failed to resolve or create principal for SID '{sid.Value}'.");
    }

    public async Task EnsurePrincipalsCachedAsync(IEnumerable<PrincipalSid> sids, CancellationToken ct)
    {
        await using WorkflowDbContext db = await _factory.CreateDbContextAsync(ct);
        await EnsurePrincipalsCachedInternalAsync(sids.Select(ps => Normalize(ps.Value)), db, ct);
    }

    public async Task<IReadOnlyList<Principal>> GetPrincipalsBySidAsync(IEnumerable<PrincipalSid> sids, CancellationToken ct)
    {
        return await GetOrCreatePrincipalsInternalAsync(sids.Select(ps => Normalize(ps.Value)), ct);
    }

    private async Task EnsurePrincipalsCachedInternalAsync(IEnumerable<string> sids, WorkflowDbContext db, CancellationToken ct)
    {
        List<string> sidList = sids
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(sid => !_cache.ContainsKey(sid)) // Only work with those not already cached
            .ToList();

        if (sidList.Count == 0)
            return;

        // Try get all from DB
        List<WorkflowPrincipal> foundEntities = await db.Set<WorkflowPrincipal>()
            .AsNoTracking()
            .Where(p => sidList.Contains(p.ExternalId))
            .ToListAsync(ct);

        var foundSids = new HashSet<string>(foundEntities.Select(e => e.ExternalId), StringComparer.OrdinalIgnoreCase);

        foreach (WorkflowPrincipal entity in foundEntities)
        {
            Principal dto = ToDto(entity);
            _cache[entity.ExternalId] = dto;
        }

        // Create missing
        List<string> missingSids = sidList.Where(sid => !foundSids.Contains(sid)).ToList();
        foreach (string sid in missingSids)
        {
            Principal principal = await CreatePrincipalIfNotExistsAsync(sid, db, ct);
            // Already added to cache inside helper
        }
    }

    private async Task<IReadOnlyList<Principal>> GetOrCreatePrincipalsInternalAsync(IEnumerable<string> sids, CancellationToken ct)
    {
        await using WorkflowDbContext db = await _factory.CreateDbContextAsync(ct);

        // Try cache first for all SIDs
        List<string> sidList = sids
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var results = new List<Principal>();
        var toQuery = new List<string>();

        foreach (string sid in sidList)
        {
            if (_cache.TryGetValue(sid, out Principal? cached))
                results.Add(cached);
            else
                toQuery.Add(sid);
        }

        if (toQuery.Count > 0)
            results.AddRange(await ResolvePrincipalsBySidAsync(toQuery, db, ct));

        return results;
    }
    
    private async Task<List<Principal>> ResolvePrincipalsBySidAsync(IEnumerable<string> sids, WorkflowDbContext db, CancellationToken ct)
    {
        List<string> sidList = sids
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var results = new List<Principal>(sidList.Count);

        if (sidList.Count == 0)
            return results;

        // Try get all from DB
        List<WorkflowPrincipal> foundEntities = await db.Set<WorkflowPrincipal>()
            .AsNoTracking()
            .Where(p => sidList.Contains(p.ExternalId))
            .ToListAsync(ct);

        var foundSids = new HashSet<string>(foundEntities.Select(e => e.ExternalId), StringComparer.OrdinalIgnoreCase);

        foreach (WorkflowPrincipal entity in foundEntities)
        {
            Principal dto = ToDto(entity);
            _cache[entity.ExternalId] = dto;
            results.Add(dto);
        }

        // Create missing
        List<string> missingSids = sidList.Where(sid => !foundSids.Contains(sid)).ToList();
        foreach (string sid in missingSids)
        {
            Principal principal = await CreatePrincipalIfNotExistsAsync(sid, db, ct);
            results.Add(principal);
        }

        return results;
    }

    private async Task<Principal> CreatePrincipalIfNotExistsAsync(string sid, WorkflowDbContext db, CancellationToken ct)
    {
        try
        {
            var newEntity = new WorkflowPrincipal
            {
                ExternalId = sid,
                Name = sid,
                Email = null
            };

            db.Add(newEntity);
            await db.SaveChangesAsync(ct);

            Principal created = ToDto(newEntity);
            _cache[sid] = created;
            _logger.LogInformation("Created new Principal: SID={Sid} Id={Id}", sid, created.Id);
            return created;
        }
        catch (DbUpdateException)
        {
            // Likely concurrent insert: reload from DB
            WorkflowPrincipal? reloaded = await db.Set<WorkflowPrincipal>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ExternalId == sid, ct);

            if (reloaded == null)
                throw new InvalidOperationException($"Failed to resolve or create principal for SID '{sid}'.");

            Principal dto = ToDto(reloaded);
            _cache[sid] = dto;
            _logger.LogInformation("Concurrent insert, loaded Principal: SID={Sid} Id={Id}", sid, dto.Id);
            return dto;
        }
    }

    /// <summary>
    /// Maps a WorkflowPrincipal entity to a Principal DTO.
    /// </summary>
    private static Principal ToDto(WorkflowPrincipal entity)
    {
        return new Principal
        {
            Id = (PrincipalId)entity.Id,
            Sid = (PrincipalSid)entity.ExternalId,
            Name = entity.Name,
            Email = entity.Email
        };
    }
}