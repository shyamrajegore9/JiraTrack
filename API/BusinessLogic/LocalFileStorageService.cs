using JiraTrack.Models.Enums;
using JiraTrack.Settings;
using Microsoft.Extensions.Options;

namespace JiraTrack.BusinessLogic;

public interface IFileStorageService
{
    Task<(string StoredFileName, string RelativePath)> SaveAsync(
        Stream stream, string originalFileName, FileType fileType, CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
    string GetAbsolutePath(string relativePath);
}

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly string _rootPath;

    public LocalFileStorageService(IOptions<FileStorageSettings> settings, IWebHostEnvironment environment)
    {
        _settings = settings.Value;
        _rootPath = Path.IsPathRooted(_settings.StoragePath)
            ? _settings.StoragePath
            : Path.Combine(environment.ContentRootPath, _settings.StoragePath);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<(string StoredFileName, string RelativePath)> SaveAsync(
        Stream stream, string originalFileName, FileType fileType, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var folder = Path.Combine(DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"), fileType.ToString());
        var relativePath = Path.Combine(folder, storedFileName).Replace('\\', '/');
        var absolutePath = GetAbsolutePath(relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        await using var fileStream = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fileStream, cancellationToken);

        return (storedFileName, relativePath);
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = GetAbsolutePath(relativePath);
        if (!File.Exists(absolutePath))
            throw new NotFoundException("File not found on disk.");

        Stream stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = GetAbsolutePath(relativePath);
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);

        return Task.CompletedTask;
    }

    public string GetAbsolutePath(string relativePath) =>
        Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
}
