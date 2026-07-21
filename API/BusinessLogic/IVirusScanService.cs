namespace JiraTrack.BusinessLogic;

public interface IVirusScanService
{
    Task ScanAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}

public class NoOpVirusScanService : IVirusScanService
{
    public Task ScanAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
