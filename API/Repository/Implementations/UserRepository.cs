using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Users;
using JiraTrack.Models.Entities;
using JiraTrack.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JiraTrack.Repository.Implementations;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByIdWithRolesAsync(int id, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        await Context.RefreshTokens
            .Include(r => r.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash, cancellationToken);

    public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default) =>
        await Context.RefreshTokens.AddAsync(token, cancellationToken);

    public async Task RevokeRefreshTokenAsync(RefreshToken token, string? replacedByHash = null, CancellationToken cancellationToken = default)
    {
        token.RevokedAt = DateTime.UtcNow;
        token.ReplacedByTokenHash = replacedByHash;
        Context.RefreshTokens.Update(token);
        await Task.CompletedTask;
    }

    public async Task RevokeAllUserRefreshTokensAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tokens = await Context.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }
    }

    public async Task AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken = default) =>
        await Context.PasswordResetTokens.AddAsync(token, cancellationToken);

    public async Task<PasswordResetToken?> GetPasswordResetTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        await Context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.IsUsed, cancellationToken);

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);

    public async Task<PagedResponse<User>> GetPagedAsync(UserFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(u =>
                u.Email.Contains(term) ||
                u.UserName.Contains(term) ||
                u.FirstName.Contains(term) ||
                u.LastName.Contains(term));
        }

        if (filter.RoleId.HasValue)
            query = query.Where(u => u.UserRoles.Any(ur => ur.RoleId == filter.RoleId.Value));

        if (filter.IsActive.HasValue)
            query = query.Where(u => u.IsActive == filter.IsActive.Value);

        query = ApplySorting(query, filter.SortBy, filter.SortDirection);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<User>
        {
            Items = items,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<List<User>> GetLookupAsync(string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(u => u.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(u =>
                u.Email.Contains(term) ||
                u.FirstName.Contains(term) ||
                u.LastName.Contains(term));
        }

        return await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Take(50)
            .ToListAsync(cancellationToken);
    }

    public async Task SetUserRolesAsync(int userId, IEnumerable<int> roleIds, CancellationToken cancellationToken = default)
    {
        var existing = await Context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync(cancellationToken);
        Context.UserRoles.RemoveRange(existing);

        foreach (var roleId in roleIds.Distinct())
        {
            await Context.UserRoles.AddAsync(new UserRole { UserId = userId, RoleId = roleId }, cancellationToken);
        }
    }

    private static IQueryable<User> ApplySorting(IQueryable<User> query, string sortBy, string sortDirection)
    {
        var desc = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLowerInvariant() switch
        {
            "email" => desc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "username" => desc ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName),
            "firstname" => desc ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            "lastname" => desc ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
            "isactive" => desc ? query.OrderByDescending(u => u.IsActive) : query.OrderBy(u => u.IsActive),
            _ => desc ? query.OrderByDescending(u => u.CreatedDate) : query.OrderBy(u => u.CreatedDate)
        };
    }
}
