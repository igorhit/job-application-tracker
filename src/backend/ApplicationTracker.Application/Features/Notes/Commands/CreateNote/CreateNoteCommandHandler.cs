using ApplicationTracker.Domain.Entities;
using ApplicationTracker.Domain.Errors;
using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Notes.Commands.CreateNote;

public class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, Result<CreateNoteResponse>>
{
    private readonly IApplicationNoteRepository _notes;
    private readonly IJobApplicationRepository _applications;

    public CreateNoteCommandHandler(IApplicationNoteRepository notes, IJobApplicationRepository applications)
    {
        _notes = notes;
        _applications = applications;
    }

    public async Task<Result<CreateNoteResponse>> Handle(CreateNoteCommand request, CancellationToken ct)
    {
        var application = await _applications.GetByIdAsync(request.JobApplicationId, request.UserId, ct);
        if (application is null)
            return Result.Fail<CreateNoteResponse>(DomainErrors.Note.ApplicationNotFound);

        var note = ApplicationNote.Create(request.JobApplicationId, request.UserId, request.Content);

        await _notes.AddAsync(note, ct);
        await _notes.SaveChangesAsync(ct);

        return Result.Ok(new CreateNoteResponse(note.Id, note.Content, note.CreatedAt));
    }
}
