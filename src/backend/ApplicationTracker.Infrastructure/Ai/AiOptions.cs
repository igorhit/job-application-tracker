namespace ApplicationTracker.Infrastructure.Ai;

public sealed class AiOptions
{
    public bool Enabled { get; set; }
    public string Provider { get; set; } = "OpenAI";
    public string Model { get; set; } = "gpt-5-mini";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";
    public string? ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 45;
}
