namespace JiraTrack.Models.Enums;

public static class CommentEntityTypes
{
    public const string Task = "Task";
    public const string Bug = "Bug";

    public static readonly string[] All = [Task, Bug];
}
