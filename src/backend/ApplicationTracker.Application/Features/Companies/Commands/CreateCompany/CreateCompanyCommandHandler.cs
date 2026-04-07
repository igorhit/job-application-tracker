using ApplicationTracker.Domain.Entities;
using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Companies.Commands.CreateCompany;

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Result<CreateCompanyResponse>>
{
    private readonly ICompanyRepository _companies;

    public CreateCompanyCommandHandler(ICompanyRepository companies)
    {
        _companies = companies;
    }

    public async Task<Result<CreateCompanyResponse>> Handle(CreateCompanyCommand request, CancellationToken ct)
    {
        var company = Company.Create(request.UserId, request.Name, request.Website, request.Notes);

        await _companies.AddAsync(company, ct);
        await _companies.SaveChangesAsync(ct);

        return Result.Ok(new CreateCompanyResponse(company.Id, company.Name));
    }
}
