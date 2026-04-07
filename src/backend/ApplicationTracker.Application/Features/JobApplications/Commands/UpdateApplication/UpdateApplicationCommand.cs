using ApplicationTracker.Domain.Enums;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.JobApplications.Commands.UpdateApplication;

public record UpdateApplicationCommand(
    Guid Id,
    Guid UserId,
    string JobTitle,
    ApplicationStatus Status,
    string? JobUrl,
    string? Location,
    decimal? SalaryExpectation,
    DateTime AppliedAt,
    DateTime? NextActionAt,
    string? NextActionNote,
    IReadOnlyList<string>? Requirements) : IRequest<Result>;
