using ApplicationTracker.Application.Features.JobApplications.Commands.CreateApplication;
using ApplicationTracker.Application.Features.JobApplications.Commands.DeleteApplication;
using ApplicationTracker.Application.Features.JobApplications.Commands.UpdateApplication;
using ApplicationTracker.Application.Features.JobApplications.Queries.GetApplicationById;
using ApplicationTracker.Application.Features.JobApplications.Queries.GenerateStudyAssistant;
using ApplicationTracker.Application.Features.JobApplications.Queries.GetApplications;
using ApplicationTracker.Domain.Enums;
using ApplicationTracker.Domain.Errors;
using FluentResults;
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
    public async Task<IActionResult> GetAll([FromQuery] GetApplicationsRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetApplicationsQuery(
            UserId,
            request.Q,
            request.Status,
            request.CompanyId,
            request.SortBy), ct);
        return Ok(result.Value);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] ApplicationStatus? status, [FromQuery] Guid? companyId, [FromQuery] JobApplicationSortBy sortBy = JobApplicationSortBy.AppliedAtDesc, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { errors = new[] { "Query parameter 'q' is required" } });

        var result = await _mediator.Send(new GetApplicationsQuery(UserId, q, status, companyId, sortBy), ct);
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
            request.NextActionNote,
            request.Requirements), ct);

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
            request.NextActionNote,
            request.Requirements), ct);

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

    [HttpPost("{id:guid}/study-assistant")]
    public async Task<IActionResult> GenerateStudyAssistant(Guid id, [FromBody] GenerateStudyAssistantRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateStudyAssistantQuery(id, UserId, request.Mode), ct);
        if (result.IsSuccess)
            return Ok(result.Value);

        if (HasErrorCode(result, "ai_not_configured"))
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { errors = new[] { "A integração opcional de IA não está configurada neste ambiente." } });

        if (HasErrorCode(result, "ai_unsupported_mode"))
            return BadRequest(new { errors = new[] { "Modo de prompt inválido." } });

        if (HasErrorCode(result, "ai_generation_failed"))
            return StatusCode(StatusCodes.Status502BadGateway, new { errors = new[] { "Não foi possível gerar conteúdo com IA no momento." } });

        if (result.Errors.Any(error => error.Message == DomainErrors.JobApplication.NotFound.Message))
            return NotFound(new { errors = result.Errors.Select(e => e.Message) });

        return BadRequest(new { errors = result.Errors.Select(e => e.Message) });
    }

    private static bool HasErrorCode(ResultBase result, string code)
    {
        return result.Errors.Any(error =>
            error.Metadata.TryGetValue("code", out var value) &&
            string.Equals(value?.ToString(), code, StringComparison.OrdinalIgnoreCase));
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
    string? NextActionNote,
    IReadOnlyList<string>? Requirements);

public record UpdateApplicationRequest(
    string JobTitle,
    ApplicationStatus Status,
    string? JobUrl,
    string? Location,
    decimal? SalaryExpectation,
    DateTime AppliedAt,
    DateTime? NextActionAt,
    string? NextActionNote,
    IReadOnlyList<string>? Requirements);

public record GenerateStudyAssistantRequest(string Mode);

public record GetApplicationsRequest(
    string? Q,
    ApplicationStatus? Status,
    Guid? CompanyId,
    JobApplicationSortBy SortBy = JobApplicationSortBy.AppliedAtDesc);
