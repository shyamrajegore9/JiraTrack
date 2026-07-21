using JiraTrack.Models.Entities;
using JiraTrack.Models.Enums;

namespace JiraTrack.Repository.Interfaces;

public interface ISprintRepository : IGenericRepository<Sprint>
{
    Task<List<Sprint>> GetByProjectAsync(int projectId, CancellationToken cancellationToken = default);
    Task<Sprint?> GetByIdWithTasksAsync(int projectId, int sprintId, CancellationToken cancellationToken = default);
    Task<Sprint?> GetActiveSprintAsync(int projectId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveSprintAsync(int projectId, int? excludeSprintId = null, CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetBacklogTasksAsync(int sprintId, CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetSprintTasksAsync(int sprintId, CancellationToken cancellationToken = default);
}
