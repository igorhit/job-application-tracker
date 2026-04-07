using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Notes.Commands.DeleteNote;

public record DeleteNoteCommand(Guid Id, Guid UserId) : IRequest<Result>;
