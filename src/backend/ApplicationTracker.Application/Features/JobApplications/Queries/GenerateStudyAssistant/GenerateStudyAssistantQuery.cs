using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.JobApplications.Queries.GenerateStudyAssistant;

public record GenerateStudyAssistantQuery(Guid ApplicationId, Guid UserId, string Mode)
    : IRequest<Result<GenerateStudyAssistantResponse>>;

public record GenerateStudyAssistantResponse(
    string Mode,
    string Provider,
    string Model,
    string Content,
    DateTime GeneratedAtUtc);
