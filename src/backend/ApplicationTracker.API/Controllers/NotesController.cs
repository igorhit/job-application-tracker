using ApplicationTracker.Application.Features.Notes.Commands.CreateNote;
using ApplicationTracker.Application.Features.Notes.Commands.DeleteNote;
using ApplicationTracker.Application.Features.Notes.Queries.GetNotes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApplicationTracker.API.Controllers;

[ApiController]
[Route("applications/{applicationId:guid}/notes")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotesController(IMediator mediator) => _mediator = mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new InvalidOperationException());

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid applicationId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetNotesQuery(applicationId, UserId), ct);
        if (result.IsFailed)
            return NotFound(new { errors = result.Errors.Select(e => e.Message) });

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid applicationId, [FromBody] CreateNoteRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateNoteCommand(applicationId, UserId, request.Content), ct);
        if (result.IsFailed)
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });

        return CreatedAtAction(nameof(GetAll), new { applicationId }, result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid applicationId, Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteNoteCommand(id, UserId), ct);
        if (result.IsFailed)
            return NotFound(new { errors = result.Errors.Select(e => e.Message) });

        return NoContent();
    }
}

public record CreateNoteRequest(string Content);
