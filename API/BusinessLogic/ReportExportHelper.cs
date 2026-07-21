using ClosedXML.Excel;
using JiraTrack.Models.DTOs.Dashboard;
using JiraTrack.Models.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace JiraTrack.BusinessLogic;

public static class ReportExportHelper
{
    static ReportExportHelper()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] ExportDeveloperPdf(DeveloperReportDto report, ReportFilterRequest filter) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Header().Text("Developer Report").FontSize(20).Bold();
                page.Content().Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text($"Developer: {report.FullName} (@{report.UserName})");
                    col.Item().Text($"Period: {FormatPeriod(filter)}");
                    col.Item().PaddingTop(10).Text($"Tasks Completed: {report.TasksCompleted}");
                    col.Item().Text($"Hours Logged: {report.HoursLogged:0.##}");
                    col.Item().Text($"Bugs Fixed: {report.BugsFixed}");

                    if (report.CompletedTasks.Count > 0)
                    {
                        col.Item().PaddingTop(16).Text("Completed Tasks").Bold();
                        foreach (var task in report.CompletedTasks.Take(25))
                            col.Item().Text($"• {task.TaskKey} — {task.Title} ({task.ProjectName})");
                    }
                });
                page.Footer().AlignCenter().Text($"Generated {DateTime.UtcNow:u}").FontSize(9);
            });
        }).GeneratePdf();

    public static byte[] ExportDeveloperExcel(DeveloperReportDto report, ReportFilterRequest filter)
    {
        using var workbook = new XLWorkbook();
        var summary = workbook.Worksheets.Add("Summary");
        summary.Cell(1, 1).Value = "Developer Report";
        summary.Cell(3, 1).Value = "Developer";
        summary.Cell(3, 2).Value = report.FullName;
        summary.Cell(4, 1).Value = "Period";
        summary.Cell(4, 2).Value = FormatPeriod(filter);
        summary.Cell(6, 1).Value = "Tasks Completed";
        summary.Cell(6, 2).Value = report.TasksCompleted;
        summary.Cell(7, 1).Value = "Hours Logged";
        summary.Cell(7, 2).Value = report.HoursLogged;
        summary.Cell(8, 1).Value = "Bugs Fixed";
        summary.Cell(8, 2).Value = report.BugsFixed;

        var tasks = workbook.Worksheets.Add("Tasks");
        tasks.Cell(1, 1).Value = "Task Key";
        tasks.Cell(1, 2).Value = "Title";
        tasks.Cell(1, 3).Value = "Project";
        tasks.Cell(1, 4).Value = "Completed Date";
        for (var i = 0; i < report.CompletedTasks.Count; i++)
        {
            var row = report.CompletedTasks[i];
            tasks.Cell(i + 2, 1).Value = row.TaskKey;
            tasks.Cell(i + 2, 2).Value = row.Title;
            tasks.Cell(i + 2, 3).Value = row.ProjectName;
            tasks.Cell(i + 2, 4).Value = row.CompletedDate;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public static byte[] ExportBugPdf(BugReportDto report, ReportFilterRequest filter) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Header().Text("Bug Report").FontSize(20).Bold();
                page.Content().Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text($"Period: {FormatPeriod(filter)}");
                    col.Item().Text($"Total Bugs: {report.TotalBugs}");
                    col.Item().PaddingTop(10).Text("By Severity").Bold();
                    foreach (var slice in report.BySeverity)
                        col.Item().Text($"• {slice.Label}: {slice.Value}");
                    col.Item().PaddingTop(10).Text("By Status").Bold();
                    foreach (var slice in report.ByStatus)
                        col.Item().Text($"• {slice.Label}: {slice.Value}");
                    col.Item().PaddingTop(10).Text("By Environment").Bold();
                    foreach (var slice in report.ByEnvironment)
                        col.Item().Text($"• {slice.Label}: {slice.Value}");
                });
                page.Footer().AlignCenter().Text($"Generated {DateTime.UtcNow:u}").FontSize(9);
            });
        }).GeneratePdf();

    public static byte[] ExportBugExcel(BugReportDto report, ReportFilterRequest filter)
    {
        using var workbook = new XLWorkbook();
        var summary = workbook.Worksheets.Add("Summary");
        summary.Cell(1, 1).Value = "Bug Report";
        summary.Cell(3, 1).Value = "Period";
        summary.Cell(3, 2).Value = FormatPeriod(filter);
        summary.Cell(4, 1).Value = "Total Bugs";
        summary.Cell(4, 2).Value = report.TotalBugs;

        WriteChartSheet(workbook, "By Severity", report.BySeverity);
        WriteChartSheet(workbook, "By Status", report.ByStatus);
        WriteChartSheet(workbook, "By Environment", report.ByEnvironment);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void WriteChartSheet(XLWorkbook workbook, string name, List<ChartSliceDto> slices)
    {
        var ws = workbook.Worksheets.Add(name);
        ws.Cell(1, 1).Value = "Label";
        ws.Cell(1, 2).Value = "Count";
        for (var i = 0; i < slices.Count; i++)
        {
            ws.Cell(i + 2, 1).Value = slices[i].Label;
            ws.Cell(i + 2, 2).Value = slices[i].Value;
        }
    }

    private static string FormatPeriod(ReportFilterRequest filter)
    {
        if (!filter.StartDate.HasValue && !filter.EndDate.HasValue) return "All time";
        return $"{filter.StartDate?.ToString("yyyy-MM-dd") ?? "..."} to {filter.EndDate?.ToString("yyyy-MM-dd") ?? "..."}";
    }
}
