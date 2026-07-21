namespace JiraTrack.Models.Enums;

public enum FileType
{
    Image = 1,
    Document = 2,
    Video = 3
}

public static class AttachmentEntityTypes
{
    public const string Task = "Task";
    public const string Bug = "Bug";
    public const string Comment = "Comment";
    public const string User = "User";

    public static readonly string[] All = [Task, Bug, Comment, User];
}
