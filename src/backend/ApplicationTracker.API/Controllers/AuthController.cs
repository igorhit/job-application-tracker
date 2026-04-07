using ApplicationTracker.Application.Features.Auth.Commands.Login;
using ApplicationTracker.Application.Features.Auth.Commands.Logout;
using ApplicationTracker.Application.Features.Auth.Commands.Refresh;
using ApplicationTracker.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApplicationTracker.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegisterCommand(request.Email, request.Password, request.Name), ct);
        if (result.IsFailed)
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });

        return CreatedAtAction(nameof(Register), new { id = result.Value.UserId }, result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginCommand(request.Email, request.Password), ct);
        if (result.IsFailed)
            return Unauthorized(new { errors = result.Errors.Select(e => e.Message) });

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), ct);
        if (result.IsFailed)
            return Unauthorized(new { errors = result.Errors.Select(e => e.Message) });

        return Ok(result.Value);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new InvalidOperationException());

        await _mediator.Send(new LogoutCommand(userId), ct);
        return NoContent();
    }
}

public record RegisterRequest(string Email, string Password, string Name);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
