using ApplicationTracker.Domain.Errors;
using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Notes.Commands.DeleteNote;

public class DeleteNoteCommandHandler : IRequestHandler<DeleteNoteCommand, Result>
{
    private readonly IApplicationNoteRepository _notes;

    public DeleteNoteCommandHandler(IApplicationNoteRepository notes)
    {
        _notes = notes;
    }

    public async Task<Result> Handle(DeleteNoteCommand request, CancellationToken ct)
    {
        var note = await _notes.GetByIdAsync(request.Id, request.UserId, ct);
        if (note is null)
            return Result.Fail(DomainErrors.Note.NotFound);

        _notes.Delete(note);
        await _notes.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
