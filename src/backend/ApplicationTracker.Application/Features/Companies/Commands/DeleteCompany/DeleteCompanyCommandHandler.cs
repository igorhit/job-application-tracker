using ApplicationTracker.Domain.Errors;
using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Companies.Commands.DeleteCompany;

public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, Result>
{
    private readonly ICompanyRepository _companies;

    public DeleteCompanyCommandHandler(ICompanyRepository companies)
    {
        _companies = companies;
    }

    public async Task<Result> Handle(DeleteCompanyCommand request, CancellationToken ct)
    {
        var company = await _companies.GetByIdAsync(request.Id, request.UserId, ct);
        if (company is null)
            return Result.Fail(DomainErrors.Company.NotFound);

        _companies.Delete(company);
        await _companies.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
