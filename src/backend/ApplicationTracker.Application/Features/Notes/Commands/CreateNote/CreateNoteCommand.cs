using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Notes.Commands.CreateNote;

public record CreateNoteCommand(Guid JobApplicationId, Guid UserId, string Content) : IRequest<Result<CreateNoteResponse>>;

public record CreateNoteResponse(Guid Id, string Content, DateTime CreatedAt);
