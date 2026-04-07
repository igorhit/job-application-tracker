using ApplicationTracker.Application.Abstractions.Ai;
using ApplicationTracker.Domain.Errors;
using FluentResults;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace ApplicationTracker.Infrastructure.Ai;

public sealed class OpenAiTextGenerationService : IAiTextGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;

    public OpenAiTextGenerationService(HttpClient httpClient, IOptions<AiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public bool IsEnabled =>
        _options.Enabled &&
        IsSupportedProvider &&
        !string.IsNullOrWhiteSpace(_options.ApiKey);

    public string ProviderName => string.IsNullOrWhiteSpace(_options.Provider) ? "OpenAI" : _options.Provider.Trim();

    public string ModelName => string.IsNullOrWhiteSpace(_options.Model) ? "gpt-5-mini" : _options.Model.Trim();

    public string AvailabilityMessage
    {
        get
        {
            if (!_options.Enabled)
                return "Integração opcional de IA desabilitada. Defina Ai__Enabled=true no backend para habilitar.";

            if (!IsSupportedProvider)
                return $"O provider configurado ({ProviderName}) ainda não é suportado por esta versão do projeto.";

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
                return "Configure Ai__ApiKey no backend para habilitar a geração com IA.";

            return $"Integração habilitada via {ProviderName} usando o modelo {ModelName}.";
        }
    }

    public async Task<Result<string>> GenerateTextAsync(AiTextGenerationRequest request, CancellationToken ct)
    {
        if (!IsEnabled)
            return Result.Fail<string>(DomainErrors.Ai.NotConfigured);

        var payload = new
        {
            model = ModelName,
            instructions = request.SystemPrompt,
            input = request.UserPrompt,
        };

        try
        {
            using var response = await _httpClient.PostAsJsonAsync("responses", payload, ct);
            if (!response.IsSuccessStatusCode)
                return Result.Fail<string>(DomainErrors.Ai.GenerationFailed);

            var responseBody = await response.Content.ReadAsStringAsync(ct);
            using var document = JsonDocument.Parse(responseBody);
            var generatedText = TryExtractGeneratedText(document.RootElement);

            if (string.IsNullOrWhiteSpace(generatedText))
                return Result.Fail<string>(DomainErrors.Ai.GenerationFailed);

            return Result.Ok(generatedText.Trim());
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return Result.Fail<string>(DomainErrors.Ai.GenerationFailed);
        }
    }

    private bool IsSupportedProvider =>
        string.Equals(ProviderName, "OpenAI", StringComparison.OrdinalIgnoreCase);

    private static string? TryExtractGeneratedText(JsonElement root)
    {
        if (root.TryGetProperty("output_text", out var outputTextElement) &&
            outputTextElement.ValueKind == JsonValueKind.String)
        {
            return outputTextElement.GetString();
        }

        if (!root.TryGetProperty("output", out var outputElement) || outputElement.ValueKind != JsonValueKind.Array)
            return null;

        var parts = new List<string>();

        foreach (var item in outputElement.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var contentElement) || contentElement.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var contentItem in contentElement.EnumerateArray())
            {
                if (contentItem.TryGetProperty("text", out var textElement) && textElement.ValueKind == JsonValueKind.String)
                {
                    var text = textElement.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                        parts.Add(text);
                }
            }
        }

        return parts.Count > 0 ? string.Join("\n\n", parts) : null;
    }
}
