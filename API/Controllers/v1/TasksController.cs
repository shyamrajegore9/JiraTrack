using Asp.Versioning;
using JiraTrack.BusinessLogic;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Tasks;
using JiraTrack.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraTrack.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects/{projectId:int}")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly TaskService _taskService;

    public TasksController(TaskService taskService) => _taskService = taskService;

    [HttpGet("tasks")]
    public async Task<ActionResult<ApiResponse<PagedResponse<TaskListDto>>>> GetTasks(
        int projectId, [FromQuery] TaskFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetTasksAsync(projectId, filter, cancellationToken);
        return Ok(ApiResponse<PagedResponse<TaskListDto>>.Ok(result));
    }

    [HttpGet("tasks/{id:int}")]
    public async Task<ActionResult<ApiResponse<TaskDetailDto>>> GetTask(int projectId, int id, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetTaskByIdAsync(projectId, id, cancellationToken);
        return Ok(ApiResponse<TaskDetailDto>.Ok(result));
    }

    [HttpPost("tasks")]
    public async Task<ActionResult<ApiResponse<TaskDetailDto>>> CreateTask(
        int projectId, [FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.CreateTaskAsync(projectId, request, cancellationToken);
        return CreatedAtAction(nameof(GetTask), new { projectId, id = result.Id }, ApiResponse<TaskDetailDto>.Ok(result, "Task created"));
    }

    [HttpPut("tasks/{id:int}")]
    public async Task<ActionResult<ApiResponse<TaskDetailDto>>> UpdateTask(
        int projectId, int id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.UpdateTaskAsync(projectId, id, request, cancellationToken);
        return Ok(ApiResponse<TaskDetailDto>.Ok(result, "Task updated"));
    }

    [HttpDelete("tasks/{id:int}")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse>> DeleteTask(int projectId, int id, CancellationToken cancellationToken)
    {
        await _taskService.DeleteTaskAsync(projectId, id, cancellationToken);
        return Ok(ApiResponse.Ok("Task deleted"));
    }

    [HttpPatch("tasks/{id:int}/status")]
    public async Task<ActionResult<ApiResponse<TaskDetailDto>>> UpdateStatus(
        int projectId, int id, [FromBody] UpdateTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.UpdateStatusAsync(projectId, id, request, cancellationToken);
        return Ok(ApiResponse<TaskDetailDto>.Ok(result, "Status updated"));
    }

    [HttpPatch("tasks/{id:int}/assign")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse<TaskDetailDto>>> AssignTask(
        int projectId, int id, [FromBody] AssignTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.AssignTaskAsync(projectId, id, request, cancellationToken);
        return Ok(ApiResponse<TaskDetailDto>.Ok(result, "Task assigned"));
    }

    [HttpGet("tasks/{id:int}/subtasks")]
    public async Task<ActionResult<ApiResponse<List<TaskListDto>>>> GetSubtasks(int projectId, int id, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetSubtasksAsync(projectId, id, cancellationToken);
        return Ok(ApiResponse<List<TaskListDto>>.Ok(result));
    }

    [HttpPost("tasks/{id:int}/subtasks")]
    public async Task<ActionResult<ApiResponse<TaskDetailDto>>> CreateSubtask(
        int projectId, int id, [FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.CreateSubtaskAsync(projectId, id, request, cancellationToken);
        return Ok(ApiResponse<TaskDetailDto>.Ok(result, "Subtask created"));
    }

    [HttpGet("tasks/{id:int}/checklist")]
    public async Task<ActionResult<ApiResponse<List<ChecklistItemDto>>>> GetChecklist(int projectId, int id, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetChecklistAsync(projectId, id, cancellationToken);
        return Ok(ApiResponse<List<ChecklistItemDto>>.Ok(result));
    }

    [HttpPost("tasks/{id:int}/checklist")]
    public async Task<ActionResult<ApiResponse<ChecklistItemDto>>> AddChecklistItem(
        int projectId, int id, [FromBody] CreateChecklistItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.AddChecklistItemAsync(projectId, id, request, cancellationToken);
        return Ok(ApiResponse<ChecklistItemDto>.Ok(result, "Checklist item added"));
    }

    [HttpPut("tasks/{id:int}/checklist/{itemId:int}")]
    public async Task<ActionResult<ApiResponse<ChecklistItemDto>>> UpdateChecklistItem(
        int projectId, int id, int itemId, [FromBody] UpdateChecklistItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.UpdateChecklistItemAsync(projectId, id, itemId, request, cancellationToken);
        return Ok(ApiResponse<ChecklistItemDto>.Ok(result, "Checklist item updated"));
    }

    [HttpDelete("tasks/{id:int}/checklist/{itemId:int}")]
    public async Task<ActionResult<ApiResponse>> DeleteChecklistItem(int projectId, int id, int itemId, CancellationToken cancellationToken)
    {
        await _taskService.DeleteChecklistItemAsync(projectId, id, itemId, cancellationToken);
        return Ok(ApiResponse.Ok("Checklist item deleted"));
    }

    [HttpPut("tasks/{id:int}/labels")]
    public async Task<ActionResult<ApiResponse<TaskDetailDto>>> SetLabels(
        int projectId, int id, [FromBody] SetTaskLabelsRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.SetLabelsAsync(projectId, id, request, cancellationToken);
        return Ok(ApiResponse<TaskDetailDto>.Ok(result, "Labels updated"));
    }

    [HttpPost("tasks/{id:int}/time-logs")]
    public async Task<ActionResult<ApiResponse<TimeLogDto>>> AddTimeLog(
        int projectId, int id, [FromBody] CreateTimeLogRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.AddTimeLogAsync(projectId, id, request, cancellationToken);
        return Ok(ApiResponse<TimeLogDto>.Ok(result, "Time logged"));
    }

    [HttpGet("tasks/{id:int}/time-logs")]
    public async Task<ActionResult<ApiResponse<List<TimeLogDto>>>> GetTimeLogs(int projectId, int id, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetTimeLogsAsync(projectId, id, cancellationToken);
        return Ok(ApiResponse<List<TimeLogDto>>.Ok(result));
    }

    [HttpGet("labels")]
    public async Task<ActionResult<ApiResponse<List<LabelDto>>>> GetLabels(int projectId, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetLabelsAsync(projectId, cancellationToken);
        return Ok(ApiResponse<List<LabelDto>>.Ok(result));
    }

    [HttpPost("labels")]
    public async Task<ActionResult<ApiResponse<LabelDto>>> CreateLabel(
        int projectId, [FromBody] CreateLabelRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.CreateLabelAsync(projectId, request, cancellationToken);
        return Ok(ApiResponse<LabelDto>.Ok(result, "Label created"));
    }
}
