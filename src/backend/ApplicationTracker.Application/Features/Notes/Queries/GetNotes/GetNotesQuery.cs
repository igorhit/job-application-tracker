using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Notes.Queries.GetNotes;

public record GetNotesQuery(Guid JobApplicationId, Guid UserId) : IRequest<Result<IReadOnlyList<NoteDto>>>;

public record NoteDto(Guid Id, string Content, DateTime CreatedAt);
