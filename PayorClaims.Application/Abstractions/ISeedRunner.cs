namespace PayorClaims.Application.Abstractions;

public interface ISeedRunner
{
    Task RunAsync(CancellationToken ct);
}
