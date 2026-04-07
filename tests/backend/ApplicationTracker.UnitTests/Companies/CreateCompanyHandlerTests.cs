using ApplicationTracker.Application.Features.Companies.Commands.CreateCompany;
using ApplicationTracker.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace ApplicationTracker.UnitTests.Companies;

public class CreateCompanyHandlerTests
{
    private readonly ICompanyRepository _companies = Substitute.For<ICompanyRepository>();

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var handler = new CreateCompanyCommandHandler(_companies);
        var result = await handler.Handle(
            new CreateCompanyCommand(userId, "Acme Corp", "https://acme.com", null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Acme Corp");
        await _companies.Received(1).AddAsync(Arg.Any<ApplicationTracker.Domain.Entities.Company>(), Arg.Any<CancellationToken>());
    }
}
