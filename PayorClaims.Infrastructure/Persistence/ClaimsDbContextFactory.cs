using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PayorClaims.Infrastructure.Persistence;

public class ClaimsDbContextFactory : IDesignTimeDbContextFactory<ClaimsDbContext>
{
    public ClaimsDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("CLAIMSDB_CONNECTION")
            ?? "Server=localhost;Database=PayorClaimsGraphQL;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseSqlServer(cs);

        return new ClaimsDbContext(options.Options);
    }
}
