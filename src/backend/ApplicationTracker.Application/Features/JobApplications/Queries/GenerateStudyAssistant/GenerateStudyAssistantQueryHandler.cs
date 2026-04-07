using ApplicationTracker.Application.Abstractions.Ai;
using ApplicationTracker.Domain.Errors;
using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.JobApplications.Queries.GenerateStudyAssistant;

public class GenerateStudyAssistantQueryHandler : IRequestHandler<GenerateStudyAssistantQuery, Result<GenerateStudyAssistantResponse>>
{
    private readonly IAiTextGenerationService _aiTextGenerationService;
    private readonly IJobApplicationRepository _applications;

    public GenerateStudyAssistantQueryHandler(
        IAiTextGenerationService aiTextGenerationService,
        IJobApplicationRepository applications)
    {
        _aiTextGenerationService = aiTextGenerationService;
        _applications = applications;
    }

    public async Task<Result<GenerateStudyAssistantResponse>> Handle(GenerateStudyAssistantQuery request, CancellationToken ct)
    {
        if (!_aiTextGenerationService.IsEnabled)
            return Result.Fail<GenerateStudyAssistantResponse>(DomainErrors.Ai.NotConfigured);

        if (!StudyPromptModes.IsValid(request.Mode))
            return Result.Fail<GenerateStudyAssistantResponse>(DomainErrors.Ai.UnsupportedMode);

        var application = await _applications.GetByIdAsync(request.ApplicationId, request.UserId, ct);
        if (application is null)
            return Result.Fail<GenerateStudyAssistantResponse>(DomainErrors.JobApplication.NotFound);

        var normalizedMode = StudyPromptModes.Normalize(request.Mode);
        var prompt = StudyAssistantPromptBuilder.BuildUserPrompt(application, normalizedMode);
        var result = await _aiTextGenerationService.GenerateTextAsync(
            new AiTextGenerationRequest(StudyAssistantPromptBuilder.SystemPrompt, prompt),
            ct);

        if (result.IsFailed)
            return Result.Fail<GenerateStudyAssistantResponse>(result.Errors);

        return Result.Ok(new GenerateStudyAssistantResponse(
            normalizedMode,
            _aiTextGenerationService.ProviderName,
            _aiTextGenerationService.ModelName,
            result.Value,
            DateTime.UtcNow));
    }
}
