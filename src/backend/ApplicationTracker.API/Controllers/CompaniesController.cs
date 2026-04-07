using ApplicationTracker.Application.Features.Companies.Commands.CreateCompany;
using ApplicationTracker.Application.Features.Companies.Commands.DeleteCompany;
using ApplicationTracker.Application.Features.Companies.Commands.UpdateCompany;
using ApplicationTracker.Application.Features.Companies.Queries.GetCompanies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApplicationTracker.API.Controllers;

[ApiController]
[Route("companies")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompaniesController(IMediator mediator) => _mediator = mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new InvalidOperationException());

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCompaniesQuery(UserId), ct);
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCompanyRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateCompanyCommand(UserId, request.Name, request.Website, request.Notes), ct);
        if (result.IsFailed)
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });

        return CreatedAtAction(nameof(GetAll), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateCompanyCommand(id, UserId, request.Name, request.Website, request.Notes), ct);
        if (result.IsFailed)
            return NotFound(new { errors = result.Errors.Select(e => e.Message) });

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteCompanyCommand(id, UserId), ct);
        if (result.IsFailed)
            return NotFound(new { errors = result.Errors.Select(e => e.Message) });

        return NoContent();
    }
}

public record CreateCompanyRequest(string Name, string? Website, string? Notes);
public record UpdateCompanyRequest(string Name, string? Website, string? Notes);
