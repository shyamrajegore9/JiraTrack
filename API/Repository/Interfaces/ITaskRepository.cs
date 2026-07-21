using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Kanban;
using JiraTrack.Models.DTOs.Tasks;
using JiraTrack.Models.Entities;
using JiraTrack.Models.Enums;

namespace JiraTrack.Repository.Interfaces;

public interface ITaskRepository : IGenericRepository<TaskItem>
{
    Task<PagedResponse<TaskItem>> GetPagedAsync(int projectId, TaskFilterRequest filter, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByIdWithDetailsAsync(int projectId, int taskId, CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetSubtasksAsync(int parentTaskId, CancellationToken cancellationToken = default);
    Task SetLabelsAsync(int taskId, IEnumerable<int> labelIds, CancellationToken cancellationToken = default);
    Task<List<Label>> GetProjectLabelsAsync(int projectId, CancellationToken cancellationToken = default);
    Task<Label?> GetLabelByIdAsync(int projectId, int labelId, CancellationToken cancellationToken = default);
    Task<Label> AddLabelAsync(Label label, CancellationToken cancellationToken = default);
    Task<List<ChecklistItem>> GetChecklistAsync(int taskId, CancellationToken cancellationToken = default);
    Task<ChecklistItem?> GetChecklistItemAsync(int taskId, int itemId, CancellationToken cancellationToken = default);
    Task AddChecklistItemAsync(ChecklistItem item, CancellationToken cancellationToken = default);
    Task AddTimeLogAsync(TimeLog timeLog, CancellationToken cancellationToken = default);
    Task<List<TimeLog>> GetTimeLogsAsync(int taskId, CancellationToken cancellationToken = default);
    Task<(int Total, int Open, int Done)> GetTaskCountsAsync(int projectId, CancellationToken cancellationToken = default);
    Task<string> GenerateTaskKeyAsync(int projectId, CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetKanbanTasksAsync(int projectId, KanbanFilterRequest filter, CancellationToken cancellationToken = default);
    Task UpdateSortOrdersAsync(int projectId, TaskItemStatus status, IList<int> taskIds, CancellationToken cancellationToken = default);
}
