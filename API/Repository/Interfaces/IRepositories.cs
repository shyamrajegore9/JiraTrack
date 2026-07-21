using System.Linq.Expressions;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Users;
using JiraTrack.Models.Entities;

namespace JiraTrack.Repository.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void SoftDelete(T entity, int deletedBy);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IGenericRepository<Role> Roles { get; }
    IProjectRepository Projects { get; }
    ITaskRepository Tasks { get; }
    IBugRepository Bugs { get; }
    ISprintRepository Sprints { get; }
    ICommentRepository Comments { get; }
    INotificationRepository Notifications { get; }
    IDashboardRepository Dashboard { get; }
    IReportRepository Reports { get; }
    ISearchRepository Search { get; }
    IAuditRepository Audit { get; }
    IAttachmentRepository Attachments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithRolesAsync(int id, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(RefreshToken token, string? replacedByHash = null, CancellationToken cancellationToken = default);
    Task RevokeAllUserRefreshTokensAsync(int userId, CancellationToken cancellationToken = default);
    Task AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    Task<PasswordResetToken?> GetPasswordResetTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<PagedResponse<User>> GetPagedAsync(UserFilterRequest filter, CancellationToken cancellationToken = default);
    Task<List<User>> GetLookupAsync(string? searchTerm, CancellationToken cancellationToken = default);
    Task SetUserRolesAsync(int userId, IEnumerable<int> roleIds, CancellationToken cancellationToken = default);
}
