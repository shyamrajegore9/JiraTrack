using JiraTrack.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JiraTrack.Repository;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
    public DbSet<TimeLog> TimeLogs => Set<TimeLog>();
    public DbSet<Bug> Bugs => Set<Bug>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.HasIndex(e => new { e.IsActive, e.IsDeleted });
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.UserName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TimeZone).HasMaxLength(50).HasDefaultValue("UTC");
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.HasOne(e => e.User).WithMany(u => u.UserRoles).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Role).WithMany(r => r.UserRoles).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.TokenHash);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.TokenHash).HasMaxLength(500).IsRequired();
            entity.HasOne(e => e.User).WithMany(u => u.RefreshTokens).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasIndex(e => e.TokenHash);
            entity.Property(e => e.TokenHash).HasMaxLength(500).IsRequired();
            entity.HasOne(e => e.User).WithMany(u => u.PasswordResetTokens).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasIndex(e => e.Key).IsUnique();
            entity.HasIndex(e => e.LeadUserId);
            entity.HasIndex(e => e.IsArchived);
            entity.Property(e => e.Key).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.LeadUser).WithMany(u => u.LedProjects).HasForeignKey(e => e.LeadUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.UserId }).IsUnique();
            entity.Property(e => e.ProjectRole).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.Project).WithMany(p => p.Members).HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany(u => u.ProjectMemberships).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.TaskKey }).IsUnique();
            entity.HasIndex(e => new { e.ProjectId, e.Status });
            entity.HasIndex(e => e.AssigneeId);
            entity.HasIndex(e => e.ParentTaskId);
            entity.HasIndex(e => e.SprintId);
            entity.Property(e => e.TaskKey).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.Project).WithMany(p => p.Tasks).HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Assignee).WithMany(u => u.AssignedTasks).HasForeignKey(e => e.AssigneeId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Reporter).WithMany(u => u.ReportedTasks).HasForeignKey(e => e.ReporterId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ParentTask).WithMany(t => t.Subtasks).HasForeignKey(e => e.ParentTaskId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Sprint).WithMany(s => s.Tasks).HasForeignKey(e => e.SprintId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Sprint>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.Status });
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Goal).HasMaxLength(500);
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.Project).WithMany(p => p.Sprints).HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Label>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.Name }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(7).HasDefaultValue("#757575");
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.Project).WithMany(p => p.Labels).HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TaskLabel>(entity =>
        {
            entity.HasKey(e => new { e.TaskId, e.LabelId });
            entity.HasOne(e => e.Task).WithMany(t => t.TaskLabels).HasForeignKey(e => e.TaskId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Label).WithMany(l => l.TaskLabels).HasForeignKey(e => e.LabelId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChecklistItem>(entity =>
        {
            entity.Property(e => e.Text).HasMaxLength(500).IsRequired();
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.Task).WithMany(t => t.ChecklistItems).HasForeignKey(e => e.TaskId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TimeLog>(entity =>
        {
            entity.HasIndex(e => e.TaskId);
            entity.HasIndex(e => new { e.UserId, e.WorkDate });
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.Task).WithMany(t => t.TimeLogs).HasForeignKey(e => e.TaskId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Bug>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.BugKey }).IsUnique();
            entity.HasIndex(e => new { e.ProjectId, e.Status });
            entity.HasIndex(e => e.DeveloperId);
            entity.HasIndex(e => e.Severity);
            entity.Property(e => e.BugKey).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Environment).HasMaxLength(100);
            entity.Property(e => e.Browser).HasMaxLength(100);
            entity.Property(e => e.OperatingSystem).HasMaxLength(100);
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.Project).WithMany(p => p.Bugs).HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Developer).WithMany(u => u.AssignedBugs).HasForeignKey(e => e.DeveloperId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Tester).WithMany(u => u.TestedBugs).HasForeignKey(e => e.TesterId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Reporter).WithMany(u => u.ReportedBugs).HasForeignKey(e => e.ReporterId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.ParentCommentId);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.EntityType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ParentComment).WithMany(c => c.Replies).HasForeignKey(e => e.ParentCommentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CommentMention>(entity =>
        {
            entity.HasKey(e => new { e.CommentId, e.UserId });
            entity.HasOne(e => e.Comment).WithMany(c => c.Mentions).HasForeignKey(e => e.CommentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CommentReaction>(entity =>
        {
            entity.HasKey(e => new { e.CommentId, e.UserId, e.Emoji });
            entity.Property(e => e.Emoji).HasMaxLength(10);
            entity.HasOne(e => e.Comment).WithMany(c => c.Reactions).HasForeignKey(e => e.CommentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.IsRead });
            entity.HasIndex(e => e.CreatedDate);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(500).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(20);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(20).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.Property(e => e.EntityType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.StoredFileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100).IsRequired();
            entity.HasOne(e => e.UploadedByUser).WithMany().HasForeignKey(e => e.UploadedBy).OnDelete(DeleteBehavior.Restrict);
        });

        SeedRoles(modelBuilder);
    }

    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "System Administrator", CreatedDate = seedDate },
            new Role { Id = 2, Name = "ProjectManager", Description = "Project Manager", CreatedDate = seedDate },
            new Role { Id = 3, Name = "Developer", Description = "Software Developer", CreatedDate = seedDate },
            new Role { Id = 4, Name = "QA", Description = "Quality Assurance", CreatedDate = seedDate },
            new Role { Id = 5, Name = "Viewer", Description = "Read-only Viewer", CreatedDate = seedDate }
        );
    }
}
