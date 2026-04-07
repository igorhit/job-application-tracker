using ApplicationTracker.Domain.Interfaces;
using ApplicationTracker.Domain.Enums;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Dashboard.Queries.GetDashboard;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, Result<DashboardDto>>
{
    private readonly IJobApplicationRepository _applications;

    public GetDashboardQueryHandler(IJobApplicationRepository applications)
    {
        _applications = applications;
    }

    public async Task<Result<DashboardDto>> Handle(GetDashboardQuery request, CancellationToken ct)
    {
        var applications = await _applications.GetFilteredAsync(
            request.UserId,
            query: null,
            status: null,
            companyId: null,
            sortBy: JobApplicationSortBy.AppliedAtDesc,
            ct);

        var byStatus = applications
            .GroupBy(a => a.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var upcoming = applications
            .Where(a => a.NextActionAt.HasValue && a.NextActionAt.Value >= DateTime.UtcNow)
            .OrderBy(a => a.NextActionAt)
            .Take(5)
            .Select(a => new UpcomingActionDto(
                a.Id,
                a.JobTitle,
                a.Company.Name,
                a.NextActionAt!.Value,
                a.NextActionNote))
            .ToList()
            .AsReadOnly();

        return Result.Ok(new DashboardDto(applications.Count, byStatus, upcoming));
    }
}
