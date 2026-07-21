using FluentValidation;
using JiraTrack.Models.DTOs.Tasks;
using JiraTrack.Models.Enums;

namespace JiraTrack.Models.Validators.Tasks;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Status).Must(BeValidStatus).WithMessage("Invalid status");
        RuleFor(x => x.Priority).Must(BeValidPriority).WithMessage("Invalid priority");
        RuleFor(x => x.StoryPoints).GreaterThanOrEqualTo(0).When(x => x.StoryPoints.HasValue);
        RuleFor(x => x.EstimatedHours).GreaterThanOrEqualTo(0).When(x => x.EstimatedHours.HasValue);
    }

    private static bool BeValidStatus(string status) =>
        Enum.TryParse<TaskItemStatus>(status, true, out _);

    private static bool BeValidPriority(string priority) =>
        Enum.TryParse<TaskPriority>(priority, true, out _);
}

public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Priority).Must(p => Enum.TryParse<TaskPriority>(p, true, out _)).WithMessage("Invalid priority");
        RuleFor(x => x.ActualHours).GreaterThanOrEqualTo(0);
    }
}

public class UpdateTaskStatusRequestValidator : AbstractValidator<UpdateTaskStatusRequest>
{
    public UpdateTaskStatusRequestValidator()
    {
        RuleFor(x => x.Status).Must(s => Enum.TryParse<TaskItemStatus>(s, true, out _)).WithMessage("Invalid status");
    }
}

public class CreateChecklistItemRequestValidator : AbstractValidator<CreateChecklistItemRequest>
{
    public CreateChecklistItemRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
    }
}

public class CreateTimeLogRequestValidator : AbstractValidator<CreateTimeLogRequest>
{
    public CreateTimeLogRequestValidator()
    {
        RuleFor(x => x.Hours).GreaterThan(0);
        RuleFor(x => x.WorkDate).NotEmpty();
    }
}

public class CreateLabelRequestValidator : AbstractValidator<CreateLabelRequest>
{
    public CreateLabelRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Color).Matches("^#[0-9A-Fa-f]{6}$").WithMessage("Color must be hex format #RRGGBB");
    }
}
