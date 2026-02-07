using Microsoft.Extensions.Logging;
using PayorClaims.Application.Abstractions;

namespace PayorClaims.Infrastructure.Seed;

public class SeedRunner : ISeedRunner
{
    private readonly ILogger<SeedRunner> _logger;

    public SeedRunner(ILogger<SeedRunner> logger)
    {
        _logger = logger;
    }

    public Task RunAsync(CancellationToken ct)
    {
        _logger.LogInformation("Seed skipped (no entities yet)");
        return Task.CompletedTask;
    }
}
