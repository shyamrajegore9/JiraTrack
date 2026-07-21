using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Projects;
using JiraTrack.Models.Entities;

namespace JiraTrack.Repository.Interfaces;

public interface IProjectRepository : IGenericRepository<Project>
{
    Task<Project?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<Project?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResponse<Project>> GetPagedAsync(ProjectFilterRequest filter, int? userId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<bool> IsMemberAsync(int projectId, int userId, CancellationToken cancellationToken = default);
    Task<List<ProjectMember>> GetMembersAsync(int projectId, CancellationToken cancellationToken = default);
    Task<ProjectMember?> GetMemberAsync(int projectId, int userId, CancellationToken cancellationToken = default);
    Task AddMemberAsync(ProjectMember member, CancellationToken cancellationToken = default);
    void RemoveMember(ProjectMember member);
}
