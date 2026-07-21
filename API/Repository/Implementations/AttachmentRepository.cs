using JiraTrack.Models.DTOs.Files;
using JiraTrack.Models.Entities;
using JiraTrack.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JiraTrack.Repository.Implementations;

public class AttachmentRepository : IAttachmentRepository
{
    private readonly ApplicationDbContext _context;

    public AttachmentRepository(ApplicationDbContext context) => _context = context;

    public Task<Attachment?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Attachments
            .Include(a => a.UploadedByUser)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<List<AttachmentDto>> GetByEntityAsync(
        string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        return await _context.Attachments
            .AsNoTracking()
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.UploadedDate)
            .Select(a => new AttachmentDto
            {
                Id = a.Id,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                FileType = a.FileType,
                UploadedBy = a.UploadedBy,
                UploadedByName = a.UploadedByUser.FirstName + " " + a.UploadedByUser.LastName,
                UploadedDate = a.UploadedDate
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<Attachment> AddAsync(Attachment attachment, CancellationToken cancellationToken = default)
    {
        await _context.Attachments.AddAsync(attachment, cancellationToken);
        return attachment;
    }

    public void Delete(Attachment attachment) => _context.Attachments.Remove(attachment);
}
