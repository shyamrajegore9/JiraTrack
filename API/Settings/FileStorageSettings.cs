namespace JiraTrack.Settings;

public class FileStorageSettings
{
    public const string SectionName = "FileStorage";
    public string StoragePath { get; set; } = "UploadedFiles";
    public long MaxImageSizeBytes { get; set; } = 10 * 1024 * 1024;
    public long MaxDocumentSizeBytes { get; set; } = 25 * 1024 * 1024;
    public long MaxVideoSizeBytes { get; set; } = 100 * 1024 * 1024;
}
