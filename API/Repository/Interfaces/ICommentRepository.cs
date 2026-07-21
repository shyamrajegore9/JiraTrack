using JiraTrack.Models.Entities;

namespace JiraTrack.Repository.Interfaces;

public interface ICommentRepository : IGenericRepository<Comment>
{
    Task<List<Comment>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken = default);
    Task<Comment?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task AddMentionsAsync(int commentId, IEnumerable<int> userIds, CancellationToken cancellationToken = default);
    Task RemoveMentionsAsync(int commentId, CancellationToken cancellationToken = default);
    Task<CommentReaction?> GetReactionAsync(int commentId, int userId, string emoji, CancellationToken cancellationToken = default);
    Task AddReactionAsync(CommentReaction reaction, CancellationToken cancellationToken = default);
    Task RemoveReactionAsync(CommentReaction reaction, CancellationToken cancellationToken = default);
    Task<List<CommentReaction>> GetReactionsAsync(int commentId, CancellationToken cancellationToken = default);
}
