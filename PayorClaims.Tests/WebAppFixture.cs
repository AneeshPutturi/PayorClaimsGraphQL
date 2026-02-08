using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Tests;

public class WebAppFixture : WebApplicationFactory<Program>
{
    private bool _dbInitialized;
    private readonly object _seedLock = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seed:Enabled"] = "false",
                ["Auth:SigningKey"] = "DEV_ONLY_32_CHARS_LONG_SECRET_KEY!!",
                ["Auth:Issuer"] = "payorclaims",
                ["Auth:Audience"] = "payorclaims"
            });
        });
        // When UseEnvironment("Testing") is set, AddInfrastructure uses SQLite in-memory (see ServiceCollectionExtensions).
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        return base.CreateHost(builder);
    }

    public void EnsureDatabaseSeeded()
    {
        if (_dbInitialized) return;
        lock (_seedLock)
        {
            if (_dbInitialized) return;
            using var scope = Server.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
            db.Database.EnsureCreated();
            SeedMinimalData(db);
            _dbInitialized = true;
        }
    }

    private void SeedMinimalData(ClaimsDbContext db)
    {
        if (db.Members.Any()) return;
        var memberId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var rowVersion = new byte[8];
        db.Members.Add(new Member
        {
            Id = memberId,
            ExternalMemberNumber = "MEM001",
            FirstName = "Test",
            LastName = "Member",
            Dob = new DateOnly(1990, 1, 1),
            Status = "Active",
            SsnPlain = "123-45-6789",
            CreatedAt = now,
            UpdatedAt = now,
            RowVersion = rowVersion
        });
        db.Providers.Add(new Provider
        {
            Id = Guid.NewGuid(),
            Npi = "1234567890",
            Name = "Test Provider",
            ProviderType = "Individual",
            ProviderStatus = "Active",
            CreatedAt = now,
            UpdatedAt = now,
            RowVersion = new byte[8]
        });
        db.MemberConsents.Add(new MemberConsent
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            ConsentType = "ProviderAccessPHI",
            Granted = false,
            GrantedAt = now,
            Source = "Test",
            CreatedAt = now,
            UpdatedAt = now,
            RowVersion = new byte[8]
        });
        db.SaveChanges();
        SeededMemberId = memberId;
        SeededProviderNpi = "1234567890";
    }

    public Guid SeededMemberId { get; private set; }
    public string SeededProviderNpi { get; private set; } = "";
}
