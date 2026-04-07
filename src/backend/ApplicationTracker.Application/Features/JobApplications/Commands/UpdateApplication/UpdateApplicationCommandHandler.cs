using ApplicationTracker.Domain.Errors;
using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.JobApplications.Commands.UpdateApplication;

public class UpdateApplicationCommandHandler : IRequestHandler<UpdateApplicationCommand, Result>
{
    private readonly IJobApplicationRepository _applications;

    public UpdateApplicationCommandHandler(IJobApplicationRepository applications)
    {
        _applications = applications;
    }

    public async Task<Result> Handle(UpdateApplicationCommand request, CancellationToken ct)
    {
        var application = await _applications.GetByIdAsync(request.Id, request.UserId, ct);
        if (application is null)
            return Result.Fail(DomainErrors.JobApplication.NotFound);

        application.Update(
            request.JobTitle,
            request.Status,
            request.JobUrl,
            request.Location,
            request.SalaryExpectation,
            request.AppliedAt,
            request.NextActionAt,
            request.NextActionNote);

        _applications.ReplaceRequirements(application, request.Requirements);

        await _applications.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
