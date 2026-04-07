using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Companies.Queries.GetCompanies;

public record GetCompaniesQuery(Guid UserId) : IRequest<Result<IReadOnlyList<CompanyDto>>>;

public record CompanyDto(Guid Id, string Name, string? Website, string? Notes, DateTime CreatedAt, int ApplicationCount);
