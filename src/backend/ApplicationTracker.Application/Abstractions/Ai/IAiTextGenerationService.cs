using FluentResults;

namespace ApplicationTracker.Application.Abstractions.Ai;

public interface IAiTextGenerationService
{
    bool IsEnabled { get; }
    string ProviderName { get; }
    string ModelName { get; }
    string AvailabilityMessage { get; }
    Task<Result<string>> GenerateTextAsync(AiTextGenerationRequest request, CancellationToken ct);
}

public sealed record AiTextGenerationRequest(string SystemPrompt, string UserPrompt);
