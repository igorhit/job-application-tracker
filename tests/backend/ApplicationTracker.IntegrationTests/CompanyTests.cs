using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ApplicationTracker.IntegrationTests;

public class CompanyTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;

    public CompanyTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient client, string token)> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var email = $"company_{Guid.NewGuid()}@example.com";

        await client.PostAsJsonAsync("/auth/register", new
        {
            email,
            password = "Password123!",
            name = "Company Test User"
        });

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "Password123!"
        });

        var body = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);
        return (client, body.AccessToken);
    }

    [Fact]
    public async Task GetCompanies_Authenticated_ReturnsEmptyList()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/companies");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var companies = await response.Content.ReadFromJsonAsync<List<object>>();
        companies.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateCompany_ValidRequest_Returns201()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/companies", new
        {
            name = "Acme Corp",
            website = "https://acme.com",
            notes = "Great company"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateCompany_Unauthenticated_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/companies", new
        {
            name = "Should Fail"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteCompany_NotOwner_Returns404()
    {
        var (client1, _) = await CreateAuthenticatedClientAsync();
        var (client2, _) = await CreateAuthenticatedClientAsync();

        var createResponse = await client1.PostAsJsonAsync("/companies", new
        {
            name = "User1 Company"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        // User2 tries to delete User1's company
        var deleteResponse = await client2.DeleteAsync($"/companies/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private record LoginResponse(string AccessToken, string RefreshToken, string Email, string Name);
    private record CreatedResponse(Guid Id, string Name);
}
