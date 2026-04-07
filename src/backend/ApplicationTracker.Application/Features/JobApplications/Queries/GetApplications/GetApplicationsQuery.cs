using ApplicationTracker.Domain.Enums;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.JobApplications.Queries.GetApplications;

public record GetApplicationsQuery(
    Guid UserId,
    string? Query = null,
    ApplicationStatus? Status = null,
    Guid? CompanyId = null,
    JobApplicationSortBy SortBy = JobApplicationSortBy.AppliedAtDesc)
    : IRequest<Result<IReadOnlyList<JobApplicationDto>>>;

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
    IReadOnlyList<ApplicationRequirementDto> Requirements,
    int NoteCount,
    DateTime CreatedAt);

public record ApplicationRequirementDto(
    Guid Id,
    string Content,
    int DisplayOrder);
