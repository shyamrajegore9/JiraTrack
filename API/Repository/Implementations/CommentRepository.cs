using JiraTrack.Models.Entities;
using JiraTrack.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JiraTrack.Repository.Implementations;

public class CommentRepository : GenericRepository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<Comment>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(c => c.User)
            .Include(c => c.Mentions).ThenInclude(m => m.User)
            .Include(c => c.Reactions).ThenInclude(r => r.User)
            .Where(c => c.EntityType == entityType && c.EntityId == entityId)
            .OrderBy(c => c.CreatedDate)
            .ToListAsync(cancellationToken);

    public async Task<Comment?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(c => c.User)
            .Include(c => c.Mentions).ThenInclude(m => m.User)
            .Include(c => c.Reactions).ThenInclude(r => r.User)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task AddMentionsAsync(int commentId, IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        foreach (var userId in userIds.Distinct())
        {
            await Context.Set<CommentMention>().AddAsync(new CommentMention
            {
                CommentId = commentId,
                UserId = userId
            }, cancellationToken);
        }
    }

    public async Task RemoveMentionsAsync(int commentId, CancellationToken cancellationToken = default)
    {
        var mentions = await Context.Set<CommentMention>().Where(m => m.CommentId == commentId).ToListAsync(cancellationToken);
        Context.Set<CommentMention>().RemoveRange(mentions);
    }

    public async Task<CommentReaction?> GetReactionAsync(int commentId, int userId, string emoji, CancellationToken cancellationToken = default) =>
        await Context.Set<CommentReaction>()
            .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId && r.Emoji == emoji, cancellationToken);

    public async Task AddReactionAsync(CommentReaction reaction, CancellationToken cancellationToken = default) =>
        await Context.Set<CommentReaction>().AddAsync(reaction, cancellationToken);

    public async Task RemoveReactionAsync(CommentReaction reaction, CancellationToken cancellationToken = default) =>
        Context.Set<CommentReaction>().Remove(reaction);

    public async Task<List<CommentReaction>> GetReactionsAsync(int commentId, CancellationToken cancellationToken = default) =>
        await Context.Set<CommentReaction>()
            .Include(r => r.User)
            .Where(r => r.CommentId == commentId)
            .ToListAsync(cancellationToken);
}
