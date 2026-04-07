using ApplicationTracker.Application.Features.JobApplications.Commands.CreateApplication;
using ApplicationTracker.Application.Features.JobApplications.Commands.DeleteApplication;
using ApplicationTracker.Application.Features.JobApplications.Commands.UpdateApplication;
using ApplicationTracker.Application.Features.JobApplications.Queries.GetApplicationById;
using ApplicationTracker.Application.Features.JobApplications.Queries.GetApplications;
using ApplicationTracker.Application.Features.JobApplications.Queries.SearchApplications;
using ApplicationTracker.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApplicationTracker.API.Controllers;

[ApiController]
[Route("applications")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicationsController(IMediator mediator) => _mediator = mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new InvalidOperationException());

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetApplicationsQuery(UserId), ct);
        return Ok(result.Value);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { errors = new[] { "Query parameter 'q' is required" } });

        var result = await _mediator.Send(new SearchApplicationsQuery(UserId, q), ct);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetApplicationByIdQuery(id, UserId), ct);
        if (result.IsFailed)
            return NotFound();

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateApplicationRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateApplicationCommand(
            UserId,
            request.CompanyId,
            request.JobTitle,
            request.Status,
            request.JobUrl,
            request.Location,
            request.SalaryExpectation,
            request.AppliedAt,
            request.NextActionAt,
            request.NextActionNote), ct);

        if (result.IsFailed)
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateApplicationRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateApplicationCommand(
            id,
            UserId,
            request.JobTitle,
            request.Status,
            request.JobUrl,
            request.Location,
            request.SalaryExpectation,
            request.AppliedAt,
            request.NextActionAt,
            request.NextActionNote), ct);

        if (result.IsFailed)
            return NotFound(new { errors = result.Errors.Select(e => e.Message) });

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteApplicationCommand(id, UserId), ct);
        if (result.IsFailed)
            return NotFound(new { errors = result.Errors.Select(e => e.Message) });

        return NoContent();
    }
}

public record CreateApplicationRequest(
    Guid CompanyId,
    string JobTitle,
    ApplicationStatus Status,
    string? JobUrl,
    string? Location,
    decimal? SalaryExpectation,
    DateTime AppliedAt,
    DateTime? NextActionAt,
    string? NextActionNote);

public record UpdateApplicationRequest(
    string JobTitle,
    ApplicationStatus Status,
    string? JobUrl,
    string? Location,
    decimal? SalaryExpectation,
    DateTime AppliedAt,
    DateTime? NextActionAt,
    string? NextActionNote);
