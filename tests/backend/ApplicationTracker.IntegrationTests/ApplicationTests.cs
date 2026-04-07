using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ApplicationTracker.IntegrationTests;

public class ApplicationTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;

    public ApplicationTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var email = $"applications_{Guid.NewGuid()}@example.com";

        await client.PostAsJsonAsync("/auth/register", new
        {
            email,
            password = "Password123!",
            name = "Application Test User"
        });

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "Password123!"
        });

        var body = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);
        return client;
    }

    [Fact]
    public async Task GetApplications_FilteredByStatusCompanyAndQuery_ReturnsExpectedItem()
    {
        var client = await CreateAuthenticatedClientAsync();

        var company1 = await CreateCompanyAsync(client, "Acme");
        var company2 = await CreateCompanyAsync(client, "Globex");

        await CreateApplicationAsync(client, company1.Id, "Backend Engineer", 1, "Remote", DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(5));
        await CreateApplicationAsync(client, company1.Id, "Platform Engineer", 2, "Hybrid", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(2));
        await CreateApplicationAsync(client, company2.Id, "Frontend Engineer", 2, "On-site", DateTime.UtcNow, DateTime.UtcNow.AddDays(1));

        var response = await client.GetAsync($"/applications?q=Platform&status=Interview&companyId={company1.Id}&sortBy=AppliedAtDesc");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var applications = await response.Content.ReadFromJsonAsync<List<ApplicationResponse>>();

        applications.Should().HaveCount(1);
        applications![0].JobTitle.Should().Be("Platform Engineer");
        applications[0].CompanyId.Should().Be(company1.Id);
        applications[0].StatusLabel.Should().Be("Interview");
    }

    [Fact]
    public async Task GetApplications_SortByNextActionAsc_ReturnsSoonestActionFirst()
    {
        var client = await CreateAuthenticatedClientAsync();
        var company = await CreateCompanyAsync(client, "Acme");

        await CreateApplicationAsync(client, company.Id, "No Action", 1, "Remote", DateTime.UtcNow.AddDays(-3), null);
        await CreateApplicationAsync(client, company.Id, "Later Action", 1, "Remote", DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(5));
        await CreateApplicationAsync(client, company.Id, "Soon Action", 1, "Remote", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        var response = await client.GetAsync("/applications?sortBy=NextActionAsc");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var applications = await response.Content.ReadFromJsonAsync<List<ApplicationResponse>>();

        applications.Should().NotBeNull();
        applications![0].JobTitle.Should().Be("Soon Action");
        applications[1].JobTitle.Should().Be("Later Action");
        applications[2].JobTitle.Should().Be("No Action");
    }

    [Fact]
    public async Task GetApplications_Unauthenticated_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/applications");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAiStatus_DefaultConfiguration_ReturnsDisabled()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/ai/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var status = await response.Content.ReadFromJsonAsync<AiStatusResponse>();

        status.Should().NotBeNull();
        status!.Enabled.Should().BeFalse();
        status.Provider.Should().Be("OpenAI");
        status.Model.Should().Be("gpt-5-mini");
    }

    [Fact]
    public async Task GenerateStudyAssistant_WhenAiIsDisabled_Returns503()
    {
        var client = await CreateAuthenticatedClientAsync();
        var company = await CreateCompanyAsync(client, "Acme");

        var createResponse = await client.PostAsJsonAsync("/applications", new
        {
            companyId = company.Id,
            jobTitle = "Backend Engineer",
            status = 1,
            location = "Remote",
            appliedAt = DateTime.UtcNow,
            requirements = new[] { "ASP.NET Core", "SQL" }
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedApplicationResponse>();

        var response = await client.PostAsJsonAsync($"/applications/{created!.Id}/study-assistant", new
        {
            mode = "quick-study"
        });

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Errors.Should().Contain("A integração opcional de IA não está configurada neste ambiente.");
    }

    [Fact]
    public async Task GetApplicationById_ReturnsStructuredRequirements()
    {
        var client = await CreateAuthenticatedClientAsync();
        var company = await CreateCompanyAsync(client, "Acme");

        var createResponse = await client.PostAsJsonAsync("/applications", new
        {
            companyId = company.Id,
            jobTitle = "Backend Engineer",
            status = 1,
            location = "Remote",
            appliedAt = DateTime.UtcNow,
            requirements = new[] { "ASP.NET Core", "SQL", "Testes automatizados" }
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedApplicationResponse>();

        var response = await client.GetAsync($"/applications/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var application = await response.Content.ReadFromJsonAsync<ApplicationDetailResponse>();

        application.Should().NotBeNull();
        application!.Requirements.Should().HaveCount(3);
        application.Requirements[0].Content.Should().Be("ASP.NET Core");
        application.Requirements[1].Content.Should().Be("SQL");
        application.Requirements[2].Content.Should().Be("Testes automatizados");
    }

    [Fact]
    public async Task UpdateApplication_ReplacesRequirementsSuccessfully()
    {
        var client = await CreateAuthenticatedClientAsync();
        var company = await CreateCompanyAsync(client, "Acme");

        var createResponse = await client.PostAsJsonAsync("/applications", new
        {
            companyId = company.Id,
            jobTitle = "Backend Engineer",
            status = 1,
            location = "Remote",
            appliedAt = DateTime.UtcNow,
            requirements = new[] { "ASP.NET Core", "SQL" }
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedApplicationResponse>();

        var updateResponse = await client.PutAsJsonAsync($"/applications/{created!.Id}", new
        {
            jobTitle = "Backend Engineer Updated",
            status = 2,
            jobUrl = "https://example.com/jobs/backend",
            location = "Hybrid",
            salaryExpectation = 9000,
            appliedAt = DateTime.UtcNow,
            nextActionAt = DateTime.UtcNow.AddDays(3),
            nextActionNote = "Technical interview scheduled",
            requirements = new[] { "ASP.NET Core", "Entity Framework", "Testes automatizados" }
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/applications/{created.Id}");
        var application = await getResponse.Content.ReadFromJsonAsync<ApplicationDetailResponse>();

        application.Should().NotBeNull();
        application!.Requirements.Should().HaveCount(3);
        application.Requirements.Select(r => r.Content).Should().BeEquivalentTo(
            ["ASP.NET Core", "Entity Framework", "Testes automatizados"],
            options => options.WithStrictOrdering());
    }

    private static async Task<CreatedCompanyResponse> CreateCompanyAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/companies", new
        {
            name
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<CreatedCompanyResponse>())!;
    }

    private static async Task CreateApplicationAsync(
        HttpClient client,
        Guid companyId,
        string jobTitle,
        int status,
        string location,
        DateTime appliedAt,
        DateTime? nextActionAt)
    {
        var response = await client.PostAsJsonAsync("/applications", new
        {
            companyId,
            jobTitle,
            status,
            location,
            appliedAt,
            nextActionAt,
            nextActionNote = nextActionAt is null ? null : $"Follow up for {jobTitle}"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private record LoginResponse(string AccessToken, string RefreshToken, string Email, string Name);
    private record CreatedCompanyResponse(Guid Id, string Name);
    private record CreatedApplicationResponse(Guid Id, string JobTitle, string CompanyName);
    private record ApplicationResponse(Guid Id, Guid CompanyId, string CompanyName, string JobTitle, int Status, string StatusLabel);
    private record ApplicationDetailResponse(Guid Id, List<ApplicationRequirementResponse> Requirements);
    private record ApplicationRequirementResponse(Guid Id, string Content, int DisplayOrder);
    private record AiStatusResponse(bool Enabled, string Provider, string Model, string Message);
    private record ErrorResponse(List<string> Errors);
}
