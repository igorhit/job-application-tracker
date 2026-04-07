using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Companies.Queries.GetCompanies;

public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, Result<IReadOnlyList<CompanyDto>>>
{
    private readonly ICompanyRepository _companies;

    public GetCompaniesQueryHandler(ICompanyRepository companies)
    {
        _companies = companies;
    }

    public async Task<Result<IReadOnlyList<CompanyDto>>> Handle(GetCompaniesQuery request, CancellationToken ct)
    {
        var companies = await _companies.GetAllByUserAsync(request.UserId, ct);

        var dtos = companies
            .Select(c => new CompanyDto(
                c.Id,
                c.Name,
                c.Website,
                c.Notes,
                c.CreatedAt,
                c.JobApplications.Count))
            .ToList()
            .AsReadOnly();

        return Result.Ok<IReadOnlyList<CompanyDto>>(dtos);
    }
}
