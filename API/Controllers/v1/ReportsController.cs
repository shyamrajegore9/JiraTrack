using Asp.Versioning;
using JiraTrack.BusinessLogic;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Reports;
using JiraTrack.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraTrack.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService) => _reportService = reportService;

    [HttpGet("developer")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse<DeveloperReportDto>>> GetDeveloperReport(
        [FromQuery] ReportFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetDeveloperReportAsync(filter, cancellationToken);
        return Ok(ApiResponse<DeveloperReportDto>.Ok(result));
    }

    [HttpGet("bugs")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager, AppRoles.QA)]
    public async Task<ActionResult<ApiResponse<BugReportDto>>> GetBugReport(
        [FromQuery] ReportFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetBugReportAsync(filter, cancellationToken);
        return Ok(ApiResponse<BugReportDto>.Ok(result));
    }

    [HttpGet("sprint/{sprintId:int}")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse<SprintReportDto>>> GetSprintReport(
        int sprintId, [FromQuery] int projectId, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetSprintReportAsync(sprintId, projectId, cancellationToken);
        return Ok(ApiResponse<SprintReportDto>.Ok(result));
    }

    [HttpGet("project/{projectId:int}")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse<ProjectReportDto>>> GetProjectReport(
        int projectId, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetProjectReportAsync(projectId, cancellationToken);
        return Ok(ApiResponse<ProjectReportDto>.Ok(result));
    }

    [HttpGet("time-tracking")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse<TimeTrackingReportDto>>> GetTimeTrackingReport(
        [FromQuery] ReportFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetTimeTrackingReportAsync(filter, cancellationToken);
        return Ok(ApiResponse<TimeTrackingReportDto>.Ok(result));
    }

    [HttpGet("developer/export/pdf")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<IActionResult> ExportDeveloperPdf([FromQuery] ReportFilterRequest filter, CancellationToken cancellationToken)
    {
        var bytes = await _reportService.ExportDeveloperPdfAsync(filter, cancellationToken);
        return File(bytes, "application/pdf", "developer-report.pdf");
    }

    [HttpGet("developer/export/excel")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<IActionResult> ExportDeveloperExcel([FromQuery] ReportFilterRequest filter, CancellationToken cancellationToken)
    {
        var bytes = await _reportService.ExportDeveloperExcelAsync(filter, cancellationToken);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "developer-report.xlsx");
    }

    [HttpGet("bugs/export/pdf")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<IActionResult> ExportBugPdf([FromQuery] ReportFilterRequest filter, CancellationToken cancellationToken)
    {
        var bytes = await _reportService.ExportBugPdfAsync(filter, cancellationToken);
        return File(bytes, "application/pdf", "bug-report.pdf");
    }

    [HttpGet("bugs/export/excel")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<IActionResult> ExportBugExcel([FromQuery] ReportFilterRequest filter, CancellationToken cancellationToken)
    {
        var bytes = await _reportService.ExportBugExcelAsync(filter, cancellationToken);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "bug-report.xlsx");
    }
}
