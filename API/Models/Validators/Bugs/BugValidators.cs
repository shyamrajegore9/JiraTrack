using FluentValidation;
using JiraTrack.Models.DTOs.Bugs;
using JiraTrack.Models.Enums;

namespace JiraTrack.Models.Validators.Bugs;

public class CreateBugRequestValidator : AbstractValidator<CreateBugRequest>
{
    public CreateBugRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Status).Must(s => Enum.TryParse<BugItemStatus>(s, true, out _)).WithMessage("Invalid status");
        RuleFor(x => x.Severity).Must(s => Enum.TryParse<BugSeverity>(s, true, out _)).WithMessage("Invalid severity");
        RuleFor(x => x.Priority).Must(p => Enum.TryParse<TaskPriority>(p, true, out _)).WithMessage("Invalid priority");
        RuleFor(x => x.Environment).MaximumLength(100);
        RuleFor(x => x.Browser).MaximumLength(100);
        RuleFor(x => x.OperatingSystem).MaximumLength(100);
    }
}

public class UpdateBugRequestValidator : AbstractValidator<UpdateBugRequest>
{
    public UpdateBugRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Severity).Must(s => Enum.TryParse<BugSeverity>(s, true, out _)).WithMessage("Invalid severity");
        RuleFor(x => x.Priority).Must(p => Enum.TryParse<TaskPriority>(p, true, out _)).WithMessage("Invalid priority");
        RuleFor(x => x.Environment).MaximumLength(100);
        RuleFor(x => x.Browser).MaximumLength(100);
        RuleFor(x => x.OperatingSystem).MaximumLength(100);
    }
}

public class UpdateBugStatusRequestValidator : AbstractValidator<UpdateBugStatusRequest>
{
    public UpdateBugStatusRequestValidator()
    {
        RuleFor(x => x.Status).Must(s => Enum.TryParse<BugItemStatus>(s, true, out _)).WithMessage("Invalid status");
    }
}
