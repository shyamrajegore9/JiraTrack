using JiraTrack.Models.Entities;
using JiraTrack.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace JiraTrack.Repository.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private IUserRepository? _users;
    private IGenericRepository<Role>? _roles;
    private IProjectRepository? _projects;
    private ITaskRepository? _tasks;
    private IBugRepository? _bugs;
    private ISprintRepository? _sprints;
    private ICommentRepository? _comments;
    private INotificationRepository? _notifications;
    private IDashboardRepository? _dashboard;
    private IReportRepository? _reports;
    private ISearchRepository? _search;
    private IAuditRepository? _audit;
    private IAttachmentRepository? _attachments;

    public UnitOfWork(ApplicationDbContext context) => _context = context;

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IGenericRepository<Role> Roles => _roles ??= new GenericRepository<Role>(_context);
    public IProjectRepository Projects => _projects ??= new ProjectRepository(_context);
    public ITaskRepository Tasks => _tasks ??= new TaskRepository(_context);
    public IBugRepository Bugs => _bugs ??= new BugRepository(_context);
    public ISprintRepository Sprints => _sprints ??= new SprintRepository(_context);
    public ICommentRepository Comments => _comments ??= new CommentRepository(_context);
    public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_context);
    public IDashboardRepository Dashboard => _dashboard ??= new DashboardRepository(_context);
    public IReportRepository Reports => _reports ??= new ReportRepository(_context);
    public ISearchRepository Search => _search ??= new SearchRepository(_context);
    public IAuditRepository Audit => _audit ??= new AuditRepository(_context);
    public IAttachmentRepository Attachments => _attachments ??= new AttachmentRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
