using FluentValidation;
using JiraTrack.Models.DTOs.Comments;
using JiraTrack.Models.Enums;

namespace JiraTrack.Models.Validators.Comments;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.EntityType)
            .Must(t => CommentEntityTypes.All.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid entity type");
        RuleFor(x => x.EntityId).GreaterThan(0);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(10000);
    }
}

public class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentRequestValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(10000);
    }
}

public class AddReactionRequestValidator : AbstractValidator<AddReactionRequest>
{
    private static readonly string[] AllowedEmojis = ["👍", "❤️", "😄", "🎉", "👀", "🚀"];

    public AddReactionRequestValidator()
    {
        RuleFor(x => x.Emoji).NotEmpty().MaximumLength(10)
            .Must(e => AllowedEmojis.Contains(e))
            .WithMessage("Invalid emoji");
    }
}
