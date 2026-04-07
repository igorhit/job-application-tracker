using ApplicationTracker.Domain.Entities;
using ApplicationTracker.Domain.Errors;
using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.JobApplications.Commands.CreateApplication;

public class CreateApplicationCommandHandler : IRequestHandler<CreateApplicationCommand, Result<CreateApplicationResponse>>
{
    private readonly IJobApplicationRepository _applications;
    private readonly ICompanyRepository _companies;

    public CreateApplicationCommandHandler(IJobApplicationRepository applications, ICompanyRepository companies)
    {
        _applications = applications;
        _companies = companies;
    }

    public async Task<Result<CreateApplicationResponse>> Handle(CreateApplicationCommand request, CancellationToken ct)
    {
        var company = await _companies.GetByIdAsync(request.CompanyId, request.UserId, ct);
        if (company is null)
            return Result.Fail<CreateApplicationResponse>(DomainErrors.JobApplication.CompanyNotFound);

        var application = JobApplication.Create(
            request.UserId,
            request.CompanyId,
            request.JobTitle,
            request.Status,
            request.JobUrl,
            request.Location,
            request.SalaryExpectation,
            request.AppliedAt,
            request.NextActionAt,
            request.NextActionNote,
            request.Requirements);

        await _applications.AddAsync(application, ct);
        await _applications.SaveChangesAsync(ct);

        return Result.Ok(new CreateApplicationResponse(application.Id, application.JobTitle, company.Name));
    }
}
