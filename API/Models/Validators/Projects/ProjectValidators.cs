using FluentValidation;
using JiraTrack.Models.DTOs.Projects;

namespace JiraTrack.Models.Validators.Projects;

public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(10)
            .Matches("^[A-Z0-9]+$").WithMessage("Key must be uppercase alphanumeric");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.LeadUserId).GreaterThan(0);
    }
}

public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.LeadUserId).GreaterThan(0);
    }
}

public class AddProjectMemberRequestValidator : AbstractValidator<AddProjectMemberRequest>
{
    public AddProjectMemberRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.ProjectRole)
            .NotEmpty()
            .Must(r => ProjectRoles.All.Contains(r))
            .WithMessage("Invalid project role");
    }
}

public class UpdateProjectSettingsRequestValidator : AbstractValidator<UpdateProjectSettingsRequest>
{
    public UpdateProjectSettingsRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
    }
}
