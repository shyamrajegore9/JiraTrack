using FluentValidation;
using JiraTrack.Models.DTOs.Kanban;
using JiraTrack.Models.Enums;

namespace JiraTrack.Models.Validators.Kanban;

public class MoveKanbanCardRequestValidator : AbstractValidator<MoveKanbanCardRequest>
{
    public MoveKanbanCardRequestValidator()
    {
        RuleFor(x => x.TaskId).GreaterThan(0);
        RuleFor(x => x.ToStatus).Must(s => Enum.TryParse<TaskItemStatus>(s, true, out _))
            .WithMessage("Invalid status");
        RuleFor(x => x.NewSortOrder).GreaterThanOrEqualTo(0);
    }
}

public class ReorderKanbanCardsRequestValidator : AbstractValidator<ReorderKanbanCardsRequest>
{
    public ReorderKanbanCardsRequestValidator()
    {
        RuleFor(x => x.Status).Must(s => Enum.TryParse<TaskItemStatus>(s, true, out _))
            .WithMessage("Invalid status");
        RuleFor(x => x.TaskIds).NotEmpty();
    }
}
