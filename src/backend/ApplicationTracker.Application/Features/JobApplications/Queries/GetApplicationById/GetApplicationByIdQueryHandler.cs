using ApplicationTracker.Application.Features.JobApplications.Queries.GetApplications;
using ApplicationTracker.Domain.Errors;
using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.JobApplications.Queries.GetApplicationById;

public class GetApplicationByIdQueryHandler : IRequestHandler<GetApplicationByIdQuery, Result<JobApplicationDto>>
{
    private readonly IJobApplicationRepository _applications;

    public GetApplicationByIdQueryHandler(IJobApplicationRepository applications)
    {
        _applications = applications;
    }

    public async Task<Result<JobApplicationDto>> Handle(GetApplicationByIdQuery request, CancellationToken ct)
    {
        var a = await _applications.GetByIdAsync(request.Id, request.UserId, ct);
        if (a is null)
            return Result.Fail<JobApplicationDto>(DomainErrors.JobApplication.NotFound);

        return Result.Ok(new JobApplicationDto(
            a.Id,
            a.CompanyId,
            a.Company.Name,
            a.JobTitle,
            a.Status,
            a.Status.ToString(),
            a.JobUrl,
            a.Location,
            a.SalaryExpectation,
            a.AppliedAt,
            a.NextActionAt,
            a.NextActionNote,
            a.Notes.Count,
            a.CreatedAt));
    }
}
