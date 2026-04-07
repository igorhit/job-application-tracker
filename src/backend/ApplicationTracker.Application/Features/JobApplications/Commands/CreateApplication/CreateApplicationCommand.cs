using ApplicationTracker.Domain.Enums;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.JobApplications.Commands.CreateApplication;

public record CreateApplicationCommand(
    Guid UserId,
    Guid CompanyId,
    string JobTitle,
    ApplicationStatus Status,
    string? JobUrl,
    string? Location,
    decimal? SalaryExpectation,
    DateTime AppliedAt,
    DateTime? NextActionAt,
    string? NextActionNote) : IRequest<Result<CreateApplicationResponse>>;

public record CreateApplicationResponse(Guid Id, string JobTitle, string CompanyName);
