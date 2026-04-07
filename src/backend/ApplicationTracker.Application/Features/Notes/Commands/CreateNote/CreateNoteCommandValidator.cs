using FluentValidation;

namespace ApplicationTracker.Application.Features.Notes.Commands.CreateNote;

public class CreateNoteCommandValidator : AbstractValidator<CreateNoteCommand>
{
    public CreateNoteCommandValidator()
    {
        RuleFor(x => x.JobApplicationId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(5000);
    }
}
