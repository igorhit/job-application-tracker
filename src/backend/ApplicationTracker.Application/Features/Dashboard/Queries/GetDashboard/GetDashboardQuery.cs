using ApplicationTracker.Domain.Enums;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Dashboard.Queries.GetDashboard;

public record GetDashboardQuery(Guid UserId) : IRequest<Result<DashboardDto>>;

public record DashboardDto(
    int Total,
    Dictionary<string, int> ByStatus,
    IReadOnlyList<UpcomingActionDto> UpcomingActions);

public record UpcomingActionDto(
    Guid ApplicationId,
    string JobTitle,
    string CompanyName,
    DateTime NextActionAt,
    string? NextActionNote);
