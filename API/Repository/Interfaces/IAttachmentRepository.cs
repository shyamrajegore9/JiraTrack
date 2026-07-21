using JiraTrack.Models.DTOs.Files;
using JiraTrack.Models.Entities;

namespace JiraTrack.Repository.Interfaces;

public interface IAttachmentRepository
{
    Task<Attachment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<AttachmentDto>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken = default);
    Task<Attachment> AddAsync(Attachment attachment, CancellationToken cancellationToken = default);
    void Delete(Attachment attachment);
}
