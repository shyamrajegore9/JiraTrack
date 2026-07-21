using JiraTrack.Models.DTOs.Reports;

namespace JiraTrack.Repository.Interfaces;

public interface IReportRepository
{
    Task<DeveloperReportDto> GetDeveloperReportAsync(int userId, List<int> projectIds, ReportFilterRequest filter, CancellationToken cancellationToken = default);
    Task<BugReportDto> GetBugReportAsync(List<int> projectIds, ReportFilterRequest filter, CancellationToken cancellationToken = default);
    Task<ProjectReportDto> GetProjectReportAsync(int projectId, CancellationToken cancellationToken = default);
    Task<TimeTrackingReportDto> GetTimeTrackingReportAsync(List<int> projectIds, ReportFilterRequest filter, CancellationToken cancellationToken = default);
}
