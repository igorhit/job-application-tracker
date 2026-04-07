using ApplicationTracker.Application.Abstractions.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationTracker.API.Controllers;

[ApiController]
[Route("ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiTextGenerationService _aiTextGenerationService;

    public AiController(IAiTextGenerationService aiTextGenerationService)
    {
        _aiTextGenerationService = aiTextGenerationService;
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new AiStatusResponse(
            _aiTextGenerationService.IsEnabled,
            _aiTextGenerationService.ProviderName,
            _aiTextGenerationService.ModelName,
            _aiTextGenerationService.AvailabilityMessage));
    }
}

public record AiStatusResponse(
    bool Enabled,
    string Provider,
    string Model,
    string Message);
