using ApplicationTracker.Application.Features.Dashboard.Queries.GetDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApplicationTracker.API.Controllers;

[ApiController]
[Route("dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator) => _mediator = mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new InvalidOperationException());

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDashboardQuery(UserId), ct);
        return Ok(result.Value);
    }
}
