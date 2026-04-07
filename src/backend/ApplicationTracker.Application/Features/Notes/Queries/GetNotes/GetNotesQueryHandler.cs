using ApplicationTracker.Domain.Errors;
using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Notes.Queries.GetNotes;

public class GetNotesQueryHandler : IRequestHandler<GetNotesQuery, Result<IReadOnlyList<NoteDto>>>
{
    private readonly IApplicationNoteRepository _notes;
    private readonly IJobApplicationRepository _applications;

    public GetNotesQueryHandler(IApplicationNoteRepository notes, IJobApplicationRepository applications)
    {
        _notes = notes;
        _applications = applications;
    }

    public async Task<Result<IReadOnlyList<NoteDto>>> Handle(GetNotesQuery request, CancellationToken ct)
    {
        var application = await _applications.GetByIdAsync(request.JobApplicationId, request.UserId, ct);
        if (application is null)
            return Result.Fail<IReadOnlyList<NoteDto>>(DomainErrors.Note.ApplicationNotFound);

        var notes = await _notes.GetByApplicationAsync(request.JobApplicationId, request.UserId, ct);

        var dtos = notes
            .Select(n => new NoteDto(n.Id, n.Content, n.CreatedAt))
            .ToList()
            .AsReadOnly();

        return Result.Ok<IReadOnlyList<NoteDto>>(dtos);
    }
}
