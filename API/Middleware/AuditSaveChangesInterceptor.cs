using System.Security.Claims;
using System.Text.Json;
using JiraTrack.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace JiraTrack.Middleware;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> ExcludedProperties = ["PasswordHash", "RowVersion"];
    private static readonly HashSet<Type> ExcludedTypes =
    [
        typeof(AuditLog),
        typeof(RefreshToken),
        typeof(PasswordResetToken)
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly List<EntityEntry<BaseEntity>> _pendingCreates = [];
    private bool _savingAudit;

    public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (!_savingAudit)
            CaptureChanges(eventData.Context, includeAdded: false);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (!_savingAudit)
            CaptureChanges(eventData.Context, includeAdded: false);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        PersistPendingCreates(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await PersistPendingCreatesAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private void CaptureChanges(DbContext? context, bool includeAdded)
    {
        if (context == null) return;

        var (userId, ipAddress) = GetAuditContext();

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (ExcludedTypes.Contains(entry.Entity.GetType())) continue;

            if (entry.State == EntityState.Added)
            {
                if (includeAdded)
                    AddAuditLog(context, entry, "Create", userId, ipAddress);
                else
                    _pendingCreates.Add(entry);

                continue;
            }

            var action = ResolveAction(entry);
            if (action == null) continue;

            AddAuditLog(context, entry, action, userId, ipAddress);
        }
    }

    private void PersistPendingCreates(DbContext? context)
    {
        if (context == null || _pendingCreates.Count == 0) return;

        var (userId, ipAddress) = GetAuditContext();

        foreach (var entry in _pendingCreates)
            AddAuditLog(context, entry, "Create", userId, ipAddress);

        _pendingCreates.Clear();

        if (context.ChangeTracker.HasChanges())
        {
            _savingAudit = true;
            try
            {
                context.SaveChanges();
            }
            finally
            {
                _savingAudit = false;
            }
        }
    }

    private async Task PersistPendingCreatesAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context == null || _pendingCreates.Count == 0) return;

        var (userId, ipAddress) = GetAuditContext();

        foreach (var entry in _pendingCreates)
            AddAuditLog(context, entry, "Create", userId, ipAddress);

        _pendingCreates.Clear();

        if (context.ChangeTracker.HasChanges())
        {
            _savingAudit = true;
            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                _savingAudit = false;
            }
        }
    }

    private (int? UserId, string? IpAddress) GetAuditContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        int? userId = null;

        if (httpContext?.User.Identity?.IsAuthenticated == true)
        {
            var claim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(claim, out var id))
                userId = id;
        }

        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
        return (userId, ipAddress);
    }

    private static string? ResolveAction(EntityEntry entry)
    {
        return entry.State switch
        {
            EntityState.Deleted => "Delete",
            EntityState.Modified when IsSoftDelete(entry) => "Delete",
            EntityState.Modified => "Update",
            _ => null
        };
    }

    private static bool IsSoftDelete(EntityEntry entry)
    {
        var property = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "IsDeleted");
        return property is { IsModified: true, CurrentValue: true, OriginalValue: false };
    }

    private static void AddAuditLog(
        DbContext context,
        EntityEntry<BaseEntity> entry,
        string action,
        int? userId,
        string? ipAddress)
    {
        var entityType = entry.Entity.GetType().Name;
        var entityId = entry.Entity.Id;

        string? oldValues = null;
        string? newValues = null;

        if (action == "Create")
            newValues = SerializeValues(entry, ValueSource.Current);
        else if (action == "Delete")
            oldValues = SerializeValues(entry, ValueSource.Original);
        else
        {
            oldValues = SerializeChangedValues(entry, ValueSource.Original);
            newValues = SerializeChangedValues(entry, ValueSource.Current);
        }

        context.Set<AuditLog>().Add(new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            UserId = userId,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        });
    }

    private static string? SerializeValues(EntityEntry entry, ValueSource source)
    {
        var values = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (ShouldSkipProperty(property)) continue;
            values[property.Metadata.Name] = source == ValueSource.Original
                ? property.OriginalValue
                : property.CurrentValue;
        }

        return values.Count == 0 ? null : JsonSerializer.Serialize(values, JsonOptions);
    }

    private static string? SerializeChangedValues(EntityEntry entry, ValueSource source)
    {
        var values = new Dictionary<string, object?>();

        foreach (var property in entry.Properties.Where(p => p.IsModified))
        {
            if (ShouldSkipProperty(property)) continue;
            values[property.Metadata.Name] = source == ValueSource.Original
                ? property.OriginalValue
                : property.CurrentValue;
        }

        return values.Count == 0 ? null : JsonSerializer.Serialize(values, JsonOptions);
    }

    private static bool ShouldSkipProperty(PropertyEntry property) =>
        property.Metadata.IsPrimaryKey() ||
        ExcludedProperties.Contains(property.Metadata.Name) ||
        property.Metadata.IsForeignKey();

    private enum ValueSource
    {
        Original,
        Current
    }
}
