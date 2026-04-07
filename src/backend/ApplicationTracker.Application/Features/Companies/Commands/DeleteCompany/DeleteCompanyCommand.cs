using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Companies.Commands.DeleteCompany;

public record DeleteCompanyCommand(Guid Id, Guid UserId) : IRequest<Result>;
