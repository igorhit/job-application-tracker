using ApplicationTracker.Domain.Errors;
using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.JobApplications.Commands.DeleteApplication;

public class DeleteApplicationCommandHandler : IRequestHandler<DeleteApplicationCommand, Result>
{
    private readonly IJobApplicationRepository _applications;

    public DeleteApplicationCommandHandler(IJobApplicationRepository applications)
    {
        _applications = applications;
    }

    public async Task<Result> Handle(DeleteApplicationCommand request, CancellationToken ct)
    {
        var application = await _applications.GetByIdAsync(request.Id, request.UserId, ct);
        if (application is null)
            return Result.Fail(DomainErrors.JobApplication.NotFound);

        _applications.Delete(application);
        await _applications.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
