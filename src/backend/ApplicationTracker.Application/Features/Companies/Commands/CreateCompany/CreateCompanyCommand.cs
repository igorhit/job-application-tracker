using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Companies.Commands.CreateCompany;

public record CreateCompanyCommand(Guid UserId, string Name, string? Website, string? Notes) : IRequest<Result<CreateCompanyResponse>>;

public record CreateCompanyResponse(Guid Id, string Name);
