using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Companies.Commands.UpdateCompany;

public record UpdateCompanyCommand(Guid Id, Guid UserId, string Name, string? Website, string? Notes) : IRequest<Result>;
