using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayorClaims.Application.Security;
using PayorClaims.Infrastructure.Persistence;
using Xunit;

namespace PayorClaims.Tests.Integration;

public class AuthAndMaskingTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _factory;

    public AuthAndMaskingTests(WebAppFixture factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClientWithToken(string? role = null, Guid? sub = null, string? npi = null, Guid? memberId = null)
    {
        var client = _factory.CreateClient();
        var config = _factory.Server.Services.GetRequiredService<IConfiguration>();
        var token = JwtTestTokenFactory.BuildToken(config, role, sub, npi, memberId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Seeded_member_is_found_by_id()
    {
        _factory.EnsureDatabaseSeeded();
        var client = _factory.CreateClient();
        var query = $$"""query { memberById(id: "{{_factory.SeededMemberId}}") { id firstName } }""";
        var res = await client.PostAsJsonAsync("/graphql", new { query });
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("errors", out _).Should().BeFalse("response should have no GraphQL errors");
        var member = json.GetProperty("data").GetProperty("memberById");
        member.ValueKind.Should().NotBe(JsonValueKind.Null, "seeded member should be found");
        member.GetProperty("firstName").GetString().Should().Be("Test");
    }

    [Fact]
    public async Task GraphQL_ping_returns_pong_without_auth()
    {
        var client = _factory.CreateClient();
        var body = new { query = "query { ping }" };
        var res = await client.PostAsJsonAsync("/graphql", body);
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("data").GetProperty("ping").GetString().Should().Be("pong");
    }

    [Fact]
    public async Task Adjuster_sees_full_SSN()
    {
        _factory.EnsureDatabaseSeeded();
        var client = CreateClientWithToken(role: "Adjuster", sub: Guid.NewGuid());
        var query = $$"""query { memberById(id: "{{_factory.SeededMemberId}}") { id ssn } }""";
        var res = await client.PostAsJsonAsync("/graphql", new { query });
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
        if (json.TryGetProperty("errors", out var err))
            throw new InvalidOperationException("GraphQL errors: " + err.ToString());
        var member = json.GetProperty("data").GetProperty("memberById");
        member.ValueKind.Should().NotBe(JsonValueKind.Null, "memberById should be found for seeded member");
        var ssn = member.GetProperty("ssn").GetString();
        ssn.Should().Be("123-45-6789");
    }

    [Fact]
    public async Task Provider_without_consent_sees_masked_SSN_and_no_raw()
    {
        _factory.EnsureDatabaseSeeded();
        var client = CreateClientWithToken(role: "Provider", sub: Guid.NewGuid(), npi: _factory.SeededProviderNpi);
        var query = $$"""query { memberById(id: "{{_factory.SeededMemberId}}") { id ssn email phone } }""";
        var res = await client.PostAsJsonAsync("/graphql", new { query });
        res.EnsureSuccessStatusCode();
        var content = await res.Content.ReadAsStringAsync();
        content.Should().NotContain("123-45-6789");
        var json = JsonDocument.Parse(content).RootElement;
        if (json.TryGetProperty("errors", out var err))
            throw new InvalidOperationException("GraphQL errors: " + err.ToString());
        var member = json.GetProperty("data").GetProperty("memberById");
        if (member.ValueKind == JsonValueKind.Null)
            throw new InvalidOperationException("memberById was null. Response: " + content);
        var ssn = member.GetProperty("ssn").GetString();
        ssn.Should().NotBeNullOrEmpty();
        ssn.Should().Contain("***");
    }

    [Fact]
    public async Task Member_sees_own_SSN()
    {
        _factory.EnsureDatabaseSeeded();
        var client = CreateClientWithToken(role: "Member", sub: Guid.NewGuid(), memberId: _factory.SeededMemberId);
        var query = $$"""query { memberById(id: "{{_factory.SeededMemberId}}") { id ssn } }""";
        var res = await client.PostAsJsonAsync("/graphql", new { query });
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
        if (json.TryGetProperty("errors", out var err))
            throw new InvalidOperationException("GraphQL errors: " + err.ToString());
        var member = json.GetProperty("data").GetProperty("memberById");
        member.ValueKind.Should().NotBe(JsonValueKind.Null, "memberById should be found");
        var ssn = member.GetProperty("ssn").GetString();
        ssn.Should().Be("123-45-6789");
    }

    [Fact]
    public async Task Member_ssn_field_never_returns_raw_ssn_when_unauthorized()
    {
        _factory.EnsureDatabaseSeeded();
        var client = _factory.CreateClient();
        var query = $$"""query { memberById(id: "{{_factory.SeededMemberId}}") { id ssn } }""";
        var res = await client.PostAsJsonAsync("/graphql", new { query });
        res.EnsureSuccessStatusCode();
        var content = await res.Content.ReadAsStringAsync();
        content.Should().NotMatchRegex(@"\d{3}-\d{2}-\d{4}");
    }

    [Fact]
    public void Masking_utilities_produce_expected_formats()
    {
        Masking.MaskSsn(null).Should().Be("***-**-****");
        Masking.MaskPhone(null).Should().Match("***-***-****");
        Masking.MaskEmail(null).Should().Contain("@");
        Masking.MaskDob().Should().Be("****-**-**");
    }
}
