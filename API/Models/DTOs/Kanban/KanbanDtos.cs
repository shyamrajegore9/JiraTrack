namespace JiraTrack.Models.DTOs.Kanban;

public class KanbanBoardDto
{
    public int ProjectId { get; set; }
    public List<KanbanColumnDto> Columns { get; set; } = [];
}

public class KanbanColumnDto
{
    public string Status { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<KanbanCardDto> Cards { get; set; } = [];
}

public class KanbanCardDto
{
    public int Id { get; set; }
    public string TaskKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? AssigneeName { get; set; }
    public int? AssigneeId { get; set; }
    public List<string> Labels { get; set; } = [];
    public decimal? StoryPoints { get; set; }
    public int SortOrder { get; set; }
}

public class KanbanFilterRequest
{
    public int? AssigneeId { get; set; }
    public int? SprintId { get; set; }
    public int? LabelId { get; set; }
}

public class MoveKanbanCardRequest
{
    public int TaskId { get; set; }
    public string ToStatus { get; set; } = string.Empty;
    public int NewSortOrder { get; set; }
}

public class ReorderKanbanCardsRequest
{
    public string Status { get; set; } = string.Empty;
    public List<int> TaskIds { get; set; } = [];
}

public class KanbanCardMovedEvent
{
    public int ProjectId { get; set; }
    public int TaskId { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class KanbanCardUpdatedEvent
{
    public int ProjectId { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<int> TaskIds { get; set; } = [];
}

public class KanbanCardAddedEvent
{
    public int ProjectId { get; set; }
    public KanbanCardDto Card { get; set; } = null!;
}
