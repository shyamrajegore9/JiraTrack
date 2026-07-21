using JiraTrack.Models.Enums;

namespace JiraTrack.BusinessLogic;

public static class FileValidationHelper
{
    private static readonly Dictionary<string, FileType> ExtensionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = FileType.Image,
        [".jpeg"] = FileType.Image,
        [".png"] = FileType.Image,
        [".gif"] = FileType.Image,
        [".webp"] = FileType.Image,
        [".pdf"] = FileType.Document,
        [".docx"] = FileType.Document,
        [".xlsx"] = FileType.Document,
        [".mp4"] = FileType.Video,
        [".webm"] = FileType.Video
    };

    private static readonly Dictionary<FileType, HashSet<string>> AllowedContentTypes = new()
    {
        [FileType.Image] = ["image/jpeg", "image/png", "image/gif", "image/webp"],
        [FileType.Document] = [
            "application/pdf",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        ],
        [FileType.Video] = ["video/mp4", "video/webm"]
    };

    public static bool TryGetFileType(string fileName, out FileType fileType)
    {
        var ext = Path.GetExtension(fileName);
        return ExtensionMap.TryGetValue(ext, out fileType);
    }

    public static void ValidateContentType(FileType fileType, string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            throw new BusinessException("Content type is required.");

        if (!AllowedContentTypes[fileType].Contains(contentType.Split(';')[0].Trim(), StringComparer.OrdinalIgnoreCase))
            throw new BusinessException($"Content type '{contentType}' is not allowed for {fileType} files.");
    }

    public static async Task ValidateMagicBytesAsync(FileType fileType, Stream stream, CancellationToken cancellationToken)
    {
        if (!stream.CanSeek)
            throw new BusinessException("Unable to validate file content.");

        var position = stream.Position;
        var header = new byte[8];
        var read = await stream.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
        stream.Position = position;

        if (read < 4)
            throw new BusinessException("File content is too small.");

        var valid = fileType switch
        {
            FileType.Image => IsJpeg(header) || IsPng(header) || IsGif(header) || IsWebp(header, read),
            FileType.Document => IsPdf(header) || IsZip(header),
            FileType.Video => IsMp4(header) || IsWebm(header),
            _ => false
        };

        if (!valid)
            throw new BusinessException("File content does not match the declared file type.");
    }

    public static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "file" : name;
    }

    private static bool IsJpeg(byte[] h) => h[0] == 0xFF && h[1] == 0xD8;
    private static bool IsPng(byte[] h) => h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47;
    private static bool IsGif(byte[] h) => h[0] == 0x47 && h[1] == 0x49 && h[2] == 0x46;
    private static bool IsWebp(byte[] h, int read) => read >= 12 && h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x46;
    private static bool IsPdf(byte[] h) => h[0] == 0x25 && h[1] == 0x50 && h[2] == 0x44 && h[3] == 0x46;
    private static bool IsZip(byte[] h) => h[0] == 0x50 && h[1] == 0x4B;
    private static bool IsMp4(byte[] h) => h[4] == 0x66 && h[5] == 0x74 && h[6] == 0x79 && h[7] == 0x70;
    private static bool IsWebm(byte[] h) => h[0] == 0x1A && h[1] == 0x45 && h[2] == 0xDF && h[3] == 0xA3;
}
