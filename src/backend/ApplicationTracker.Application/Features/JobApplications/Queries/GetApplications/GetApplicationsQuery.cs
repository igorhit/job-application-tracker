using ApplicationTracker.Domain.Enums;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.JobApplications.Queries.GetApplications;

public record GetApplicationsQuery(Guid UserId) : IRequest<Result<IReadOnlyList<JobApplicationDto>>>;

public record JobApplicationDto(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    string JobTitle,
    ApplicationStatus Status,
    string StatusLabel,
    string? JobUrl,
    string? Location,
    decimal? SalaryExpectation,
    DateTime AppliedAt,
    DateTime? NextActionAt,
    string? NextActionNote,
    int NoteCount,
    DateTime CreatedAt);
